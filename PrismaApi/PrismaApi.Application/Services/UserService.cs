using Microsoft.Extensions.Caching.Memory;
using Microsoft.Graph;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure.Caching;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrismaApi.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly GraphServiceClient _graphServiceClient;
    private readonly IMemoryCache _memoryCache;

    public UserService(IUserRepository userRepository, GraphServiceClient graphServiceClient, IMemoryCache memoryCache)
    {
        _userRepository = userRepository;
        _graphServiceClient = graphServiceClient;
        _memoryCache = memoryCache;
    }

    public async Task<List<UserOutgoingDto>> GetAsync(List<int> ids)
    {
        var users = await _userRepository.GetByIdsAsync(ids, withTracking: false);
        return users.ToOutgoingDtos();
    }

    public async Task<List<UserOutgoingDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync(withTracking: false);
        return users.ToOutgoingDtos();
    }

    public async Task<UserOutgoingDto?> GetByAzureIdAsync(string azureId)
    {
        var user = await _userRepository.GetByAzureIdAsync(azureId);
        return user != null ? user.ToOutgoingDto() : null;
    }

    public async Task<UserOutgoingDto> GetOrCreateUserByAzureIdAsync(UserIncomingDto dto)
    {
        var user = await GetByAzureIdAsync(dto.AzureId);
        user ??= (await _userRepository.AddAsync(dto.ToEntity())).ToOutgoingDto();
        return user;
    }

    public async Task<UserOutgoingDto> GetOrCreateUserFromGraphMeAsync(string? cacheKey)
    {
        if (cacheKey != null && _memoryCache.TryGetValue(cacheKey, out UserOutgoingDto? cachedUser) && cachedUser != null)
        {
            return cachedUser;
        }
        var graphUser = await _graphServiceClient.Me.GetAsync();
        if (graphUser == null || graphUser.Id == null) throw new Exception("User not found");
        var userDto = new UserIncomingDto
        {
            AzureId = graphUser.Id,
            Name = graphUser.DisplayName ?? "",
        };
        var user = (await _userRepository.GetOrAddByAzureIdAsync(userDto)).ToOutgoingDto();

        if (cacheKey != null)
            _memoryCache.AddCacheItem(new CacheItem { CacheKey = cacheKey }, TimeSpan.FromMinutes(30), user);

        return user;

    }
    public async Task<List<UserOutgoingDto>> SearchUsersFromGraphAsync(string query)
    {
        string sanitizedQuery = SanitizeSearchQuery(query);
        var users = await _graphServiceClient.Users
            .GetAsync(config =>
            {
                config.QueryParameters.Search = $"\"displayName:{sanitizedQuery}\" OR \"mail:{sanitizedQuery}\"";
                config.QueryParameters.Select = new[] { "id", "displayName", "mail", "userPrincipalName" };
                config.QueryParameters.Top = 100;
                config.Headers.Add("ConsistencyLevel", "eventual");
            });

        return users?.Value?.Select(u => new UserOutgoingDto
        {
            Id = 0,
            Name = u.DisplayName ?? u.UserPrincipalName ?? "",
        }).ToList() ?? new List<UserOutgoingDto>();
    }
    private static string SanitizeSearchQuery(string query)
    {
        var sanitized = query.Trim();
        sanitized = Regex.Replace(sanitized, @"\s+", " ");
        sanitized = Regex.Replace(sanitized, @"[""'\\(){}[\];]", string.Empty);
        return sanitized;
    }
}

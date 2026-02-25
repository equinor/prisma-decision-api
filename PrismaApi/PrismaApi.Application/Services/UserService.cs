using Microsoft.Extensions.Caching.Memory;
using Microsoft.Graph;
using PrismaApi.Application.Mapping;
using PrismaApi.Application.Repositories;
using PrismaApi.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrismaApi.Application.Services;

public class UserService
{
    private readonly UserRepository _userRepository;
    private readonly GraphServiceClient _graphServiceClient;
    private readonly IMemoryCache _memoryCache;

    public UserService(UserRepository userRepository, GraphServiceClient graphServiceClient, IMemoryCache memoryCache)
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
        
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .SetSlidingExpiration(TimeSpan.FromMinutes(10));
        if (cacheKey != null)
            _memoryCache.Set(cacheKey, user, cacheOptions);

        return user;

    }
}

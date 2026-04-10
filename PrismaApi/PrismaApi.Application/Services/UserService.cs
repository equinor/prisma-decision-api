using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Graph;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Constants;
using PrismaApi.Infrastructure.Caching;
using Scampi.Domain.Extensions;
using Microsoft.Identity.Web;

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
    public async Task<List<UserOutgoingDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync(withTracking: false);
        return users.ToOutgoingDtos();
    }

    public async Task<UserOutgoingDto> GetOrCreateUserByIdAsync(UserIncomingDto dto)
    {
        var user = (await _userRepository.GetOrAddByIdAsync(dto)).ToOutgoingDto();
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
            Id = graphUser.Id.ToString(),
            Name = graphUser.DisplayName ?? "",
        };
        var user = (await _userRepository.GetOrAddByIdAsync(userDto)).ToOutgoingDto();

        if (cacheKey != null)
            _memoryCache.AddCacheItem(new CacheItem { CacheKey = cacheKey }, TimeSpan.FromMinutes(30), user);

        return user;
    }

    public async Task<UserOutgoingDto> GetOrCreateUserFromContextAsync(HttpContext context)
    {
        var oid = context.User.Claims.FirstOrDefault(c => c.Type == ClaimConstants.Oid)?.Value 
            ?? context.User.Claims.FirstOrDefault(c => c.Type == ClaimConstants.ObjectId)?.Value;

        if (string.IsNullOrEmpty(oid))
        {
            throw new InvalidOperationException("No Id found in Claims");
        }
        var user = await GetOrCreateUserFromGraphMeAsync(oid);
        return user;
    }
    public async Task<List<UserOutgoingDto>> GetByIdsAsync(IEnumerable<string> ids)
    {
        var users = await _userRepository.GetByIdsAsync(ids, withTracking: false);
        return users.ToOutgoingDtos();
    }

    public async Task<List<UserOutgoingDto>> SearchUsersFromGraphAsync(string query)
    {
        string sanitizedQuery = query.SanitizeQuery();
        var users = await _graphServiceClient.Users
            .GetAsync(config =>
            {
                config.QueryParameters.Search = $"\"displayName:{sanitizedQuery}\" OR \"mail:{sanitizedQuery}\"";
                config.QueryParameters.Select = GraphApiConstants.UserSearchSelectFields;
                config.QueryParameters.Top = GraphApiConstants.DefaultSearchTop;
                config.Headers.Add(GraphApiConstants.ConsistencyLevelHeader, GraphApiConstants.ConsistencyLevelEventual);
            });

        return users?.Value?.Select(u => new UserOutgoingDto
        {
            Id = u.Id ?? "",
            Name = u.DisplayName ?? u.UserPrincipalName ?? "",
        }).ToList() ?? new List<UserOutgoingDto>();
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure.Caching;
using PrismaApi.Domain.Extensions;
using Microsoft.Graph.Models;

namespace PrismaApi.Application.Services;

public class InternalUserService : IUserProvider
{
    private readonly IUserRepository _userRepository;
    private readonly GraphServiceClient _graphServiceClient;
    private readonly IMemoryCache _memoryCache;

    public InternalUserService(IUserRepository userRepository, GraphServiceClient graphServiceClient, IMemoryCache memoryCache)
    {
        _userRepository = userRepository;
        _graphServiceClient = graphServiceClient;
        _memoryCache = memoryCache;
    }

    public async Task<UserOutgoingDto> ResolveUserFromContextAsync(HttpContext context)
    {
        var oid = context.User.Claims.FirstOrDefault(c => c.Type == ClaimConstants.Oid)?.Value
            ?? context.User.Claims.FirstOrDefault(c => c.Type == ClaimConstants.ObjectId)?.Value;

        if (string.IsNullOrEmpty(oid))
        {
            throw new InvalidOperationException("No Id found in Claims");
        }
        var cacheKey = CacheKeys.GetUserKey(oid);
        if (_memoryCache.TryGetValue(cacheKey, out UserOutgoingDto? cachedUser) && cachedUser != null)
        {
            return cachedUser;
        }

        var graphUser = await _graphServiceClient.Me.GetAsync();
        if (graphUser == null || graphUser.Id == null) throw new InvalidOperationException("User not found in Graph API.");

        var userDto = new UserIncomingDto
        {
            Id = graphUser.Id.ToString(),
            Name = graphUser.DisplayName ?? "",
        };
        var user = (await _userRepository.GetOrAddByIdAsync(userDto)).ToOutgoingDto();

        _memoryCache.AddCacheItem(new CacheItem { CacheKey = cacheKey }, CacheConstants.DefaultLongQueryCacheInTimeSpan, user);

        return user;
    }

    public async Task<List<UserOutgoingDto>> SearchUsersAsync(string query)
    {
        string sanitizedQuery = query.SanitizeQuery();
        var users = await _graphServiceClient.Users
            .GetAsync(config =>
            {
                config.QueryParameters.Search = $"\"displayName:{sanitizedQuery}\" OR \"mail:{sanitizedQuery}\"";
                config.QueryParameters.Count = true;
                config.QueryParameters.Select = GraphApiConstants.UserSearchSelectFields;
                config.QueryParameters.Top = GraphApiConstants.DefaultSearchTop;
                config.Headers.Add(GraphApiConstants.ConsistencyLevelHeader, GraphApiConstants.ConsistencyLevelEventual);
            });

        var filteredUsers = users?.Value?
            .Where(u => !(u.UserPrincipalName ?? "").Contains("StatoilSRM.onmicrosoft.com", StringComparison.OrdinalIgnoreCase))
            .Select(u => new UserOutgoingDto
            {
                Id = u.Id ?? "",
                Name = u.DisplayName ?? u.UserPrincipalName ?? "",
            })
            .ToList() ?? new List<UserOutgoingDto>();

        return filteredUsers;
    }
}

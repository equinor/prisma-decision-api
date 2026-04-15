using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Graph;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Constants;
using PrismaApi.Infrastructure.Caching;
using Scampi.Domain.Extensions;
using Microsoft.Identity.Web;

namespace PrismaApi.Application.Services;

public class UserService : BaseUserService
{
    private readonly GraphServiceClient _graphServiceClient;
    private readonly IMemoryCache _memoryCache;

    public UserService(IUserRepository userRepository, GraphServiceClient graphServiceClient, IMemoryCache memoryCache)
        : base(userRepository)
    {
        _graphServiceClient = graphServiceClient;
        _memoryCache = memoryCache;
    }

    protected override async Task<UserOutgoingDto> ResolveUserAsync(string? cacheKey)
    {
        if (cacheKey != null && _memoryCache.TryGetValue(cacheKey, out UserOutgoingDto? cachedUser) && cachedUser != null)
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
        var user = (await UserRepository.GetOrAddByIdAsync(userDto)).ToOutgoingDto();

        if (cacheKey != null)
            _memoryCache.AddCacheItem(new CacheItem { CacheKey = cacheKey }, TimeSpan.FromMinutes(30), user);

        return user;
    }

    public override async Task<UserOutgoingDto> GetOrCreateUserFromContextAsync(HttpContext context)
    {
        var oid = context.User.Claims.FirstOrDefault(c => c.Type == ClaimConstants.Oid)?.Value
            ?? context.User.Claims.FirstOrDefault(c => c.Type == ClaimConstants.ObjectId)?.Value;

        if (string.IsNullOrEmpty(oid))
        {
            throw new InvalidOperationException("No Id found in Claims");
        }
        var user = await ResolveUserAsync(oid);
        return user;
    }

    public override async Task<List<UserOutgoingDto>> SearchUsersAsync(string query)
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

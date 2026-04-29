using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure.Caching;
using Scampi.Domain.Extensions;

namespace PrismaApi.Application.Services;

public class PublicUserService : IUserProvider
{
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _memoryCache;

    public PublicUserService(IUserRepository userRepository, IMemoryCache memoryCache)
    {
        _userRepository = userRepository;
        _memoryCache = memoryCache;
    }

    public async Task<UserOutgoingDto> ResolveUserFromContextAsync(HttpContext context)
    {
        var userName = context.Request.Headers[AppConstants.PublicUsernameHeader].FirstOrDefault();
        if (string.IsNullOrEmpty(userName))
        {
            throw new InvalidOperationException("Username is required");
        }

        var cacheKey = $"public_user_{userName.ToLower()}";
        if (_memoryCache.TryGetValue(cacheKey, out UserOutgoingDto? cachedUser) && cachedUser != null)
        {
            return cachedUser;
        }

        var dto = new UserIncomingDto { Id = Guid.NewGuid().ToString(), Name = userName };
        var user = (await _userRepository.GetOrAddByUserNameAsync(dto)).ToOutgoingDto();

        _memoryCache.AddCacheItem(new CacheItem { CacheKey = cacheKey }, TimeSpan.FromMinutes(30), user);

        return user;
    }

    public async Task<List<UserOutgoingDto>> SearchUsersAsync(string query)
    {
        string sanitizedQuery = query.SanitizeQuery();
        var users = await _userRepository.GetAllAsync(
            withTracking: false,
            filterPredicate: u => u.Name.ToLower().Contains(sanitizedQuery.ToLower()));

        return users.ToOutgoingDtos();
    }
}

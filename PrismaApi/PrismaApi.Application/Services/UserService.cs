using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrismaApi.Application.Mapping;
using PrismaApi.Application.Repositories;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class UserService
{
    private readonly UserRepository _userRepository;

    public UserService(UserRepository userRepository)
    {
        _userRepository = userRepository;
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
}

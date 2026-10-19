using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services
{
    public interface IStakeholderMatrixEntryService
    {
        Task<List<StakeholderMatrixEntryDto>> CreateAsync(List<StakeholderMatrixEntryIncomingDto> dtos, UserOutgoingDto user, CancellationToken ct = default);
        Task<List<StakeholderMatrixEntryDto>> UpdateAsync(List<StakeholderMatrixEntryIncomingDto> dtos, UserOutgoingDto user, CancellationToken ct = default);
        Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
        Task<List<StakeholderMatrixEntryDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
        Task<List<StakeholderMatrixEntryDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
        Task<List<StakeholderMatrixEntryDto>> GetByProjectAsync(Guid projectId, UserOutgoingDto user, CancellationToken ct = default);
    }
}

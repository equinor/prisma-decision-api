using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services
{
    public interface IStakeholderMatrixEntryService
    {
        Task<List<StakeholderMatrixEntryOutgoingDto>> CreateAsync(List<StakeholderMatrixEntryIncomingDto> dtos, UserOutgoingDto user, CancellationToken ct = default);
        Task<List<StakeholderMatrixEntryOutgoingDto>> UpdateAsync(List<StakeholderMatrixEntryIncomingDto> dtos, UserOutgoingDto user, CancellationToken ct = default);
        Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
        Task<List<StakeholderMatrixEntryOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
        Task<List<StakeholderMatrixEntryOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
        Task<List<StakeholderMatrixEntryOutgoingDto>> GetByProjectAsync(Guid projectId, UserOutgoingDto user, CancellationToken ct = default);
    }
}

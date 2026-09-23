using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services
{
    public interface IStakeholderMatrixService
    {
        Task<List<StakeholderMatrixDto>> CreateAsync(List<StakeholderMatrixIncomingDto> dtos, UserOutgoingDto user, CancellationToken ct = default);
        Task<List<StakeholderMatrixDto>> UpdateAsync(List<StakeholderMatrixIncomingDto> dtos, UserOutgoingDto user, CancellationToken ct = default);
        Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
        Task<List<StakeholderMatrixDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default);
        Task<List<StakeholderMatrixDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default);
        Task<List<StakeholderMatrixDto>> GetByProjectAsync(Guid projectId, UserOutgoingDto user, CancellationToken ct = default);
    }
}

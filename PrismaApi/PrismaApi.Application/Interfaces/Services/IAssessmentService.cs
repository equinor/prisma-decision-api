using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services
{
    public interface IAssessmentService
    {
        Task<List<AssessmentOutgoingDto>?> CreateAsync(List<AssessmentIncomingDto> dtos, UserOutgoingDto user);
        Task<List<AssessmentOutgoingDto>?> GetAsync(List<Guid> ids, UserOutgoingDto user);
        Task<List<AssessmentOutgoingDto>> GetAllAsync(UserOutgoingDto user);
        Task UpdateRangeAsync(List<AssessmentIncomingDto> dtos, UserOutgoingDto user);
        Task DeleteAsync(List<Guid> ids, UserOutgoingDto user);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services
{
    public interface ISpiderAssessmentService
    {
        Task<SpiderAssessmentOutgoingDto> GetAsync(Guid id, UserOutgoingDto user);
        Task<List<SpiderAssessmentOutgoingDto>> GetAllAsync(UserOutgoingDto user);
        Task<List<SpiderAssessmentOutgoingDto>> CreateAsync(List<SpiderAssessmentIncomingDto> dtos, UserOutgoingDto user);
        Task UpdateAsync(List<SpiderAssessmentIncomingDto> dtos, UserOutgoingDto user);
        Task DeleteAsync(List<Guid> ids, UserOutgoingDto user);
    }
}

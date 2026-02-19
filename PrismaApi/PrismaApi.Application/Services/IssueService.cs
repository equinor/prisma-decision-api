using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Application.Mapping;
using PrismaApi.Application.Repositories;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class IssueService
{
    private readonly IssueRepository _issueRepository;

    public IssueService(IssueRepository issueRepository)
    {
        _issueRepository = issueRepository;
    }

    public async Task<List<IssueOutgoingDto>> CreateAsync(List<IssueIncomingDto> dtos, UserOutgoingDto userDto)
    {
        EnsureNodeDefaults(dtos);
        var entities = dtos.ToEntities(userDto);
        await _issueRepository.AddRangeAsync(entities);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<IssueOutgoingDto>> UpdateAsync(List<IssueIncomingDto> dtos, UserOutgoingDto userDto)
    {
        EnsureNodeDefaults(dtos);
        var entities = dtos.ToEntities(userDto);
        await _issueRepository.UpdateRangeAsync(entities);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _issueRepository.GetByIdsAsync(ids, withTracking: false);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _issueRepository.DeleteByIdsAsync(ids);
    }

    public async Task<List<IssueOutgoingDto>> GetAsync(List<Guid> ids)
    {
        var issues = await _issueRepository.GetByIdsAsync(ids, withTracking: false);
        return issues.ToOutgoingDtos();
    }

    public async Task<List<IssueOutgoingDto>> GetAllAsync()
    {
        var issues = await _issueRepository.GetAllAsync(withTracking: false);
        return issues.ToOutgoingDtos();
    }

    private static void EnsureNodeDefaults(IEnumerable<IssueIncomingDto> dtos)
    {
        foreach (var dto in dtos)
        {
            if (dto.Node != null)
            {
                if (dto.Node.NodeStyle == null)
                {
                    dto.Node.NodeStyle = new NodeStyleIncomingDto
                    {
                        Id = Guid.NewGuid(),
                        NodeId = dto.Node.Id
                    };
                }

                continue;
            }

            var nodeId = Guid.NewGuid();
            dto.Node = new NodeIncomingDto
            {
                Id = nodeId,
                ProjectId = dto.ProjectId,
                IssueId = dto.Id,
                NodeStyle = new NodeStyleIncomingDto
                {
                    Id = Guid.NewGuid(),
                    NodeId = nodeId
                }
            };
        }
    }
}

using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace PrismaApi.Domain.Entities;

public class User : BaseEntity, IBaseEntity<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AzureId { get; set; } = string.Empty;

    public ICollection<ProjectRole> ProjectRoles { get; set; } = new List<ProjectRole>();
}

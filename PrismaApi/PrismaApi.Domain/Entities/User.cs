using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class User : BaseEntity, IBaseEntity<int>
{
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("azure_id")]
    public string AzureId { get; set; } = string.Empty;

    public ICollection<ProjectRole> ProjectRoles { get; set; } = new List<ProjectRole>();
}

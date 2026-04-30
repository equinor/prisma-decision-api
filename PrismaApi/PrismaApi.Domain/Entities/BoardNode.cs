using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class BoardNode : AuditableEntity, IBaseEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Type { get; set; } = string.Empty;
    public double Height { get; set; }
    public double Width { get; set; }
    public double XPosition { get; set; }
    public double YPosition { get; set; }
    public double Rotation { get; set; }
    public string Data { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public Project? Project { get; set; }
}
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class BoardNode : AuditableEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid ProjectId { get; set; }
    public string Type { get; set; } = string.Empty;
    public double Height { get; set; }
    public double Width { get; set; }
    public double XPosition { get; set; }
    public double YPosition { get; set; }
    public double Rotation { get; set; }
    public string Data { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public float StrokeWidth { get; set; } = 8;
    public string StrokeStyle { get; set; } = BoardNodeStrokeStyles.Solid.ToString();
    // opacity: between 0 and 100
    public int Opacity { get; set; } = 100;
    public Project? Project { get; set; }
}

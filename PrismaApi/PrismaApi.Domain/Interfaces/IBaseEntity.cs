namespace PrismaApi.Domain.Interfaces;

public interface IBaseEntity<TId>
{
    TId Id { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}

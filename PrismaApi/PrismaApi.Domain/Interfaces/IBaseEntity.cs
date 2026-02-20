namespace PrismaApi.Domain.Interfaces;

public interface IBaseEntity<TId>
{
    TId Id { get; set; }
    DateTimeOffset? CreatedAt { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
}

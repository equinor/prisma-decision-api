using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;
public interface IEntityHandlingPolicy
{
    [NotMapped]
    TransferBehavior IsTransferable { get; }
}

public enum TransferBehavior
{
    Transferable,
    NotTransferable
}
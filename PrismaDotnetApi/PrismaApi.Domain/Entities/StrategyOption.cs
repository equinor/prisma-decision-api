using PrismaApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class StrategyOption
{
    public required Guid StrategyId { get; set; }
    public required Guid OptionId { get; set; }

    public Strategy? Strategy { get; set; }
    public Option? Option { get; set; }
}

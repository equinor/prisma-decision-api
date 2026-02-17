using PrismaApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class StrategyOption
{
    [Column("strategy_id")]
    public Guid StrategyId { get; set; }
    [Column("option_id")]
    public Guid OptionId { get; set; }

    public Strategy? Strategy { get; set; }
    public Option? Option { get; set; }
}

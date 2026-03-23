using PrismaApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class StrategyOption
{
    public Guid StrategyId { get; set; }
    public Guid OptionId { get; set; }

    public Strategy? Strategy { get; set; }
    public Option? Option { get; set; }
}

using System;

namespace PrismaApi.Domain.Constants;

public static class DomainConstants
{
    public const int MaxShortStringLength = 60;
    public const int MaxLongStringLength = 600;
    public const int FloatPrecision = 53;
    public const int DecimalPlaces = 14;

    public static readonly Guid DefaultValueMetricId =
        Guid.Parse("288e0811-7ab6-5d24-b80c-9fa925b848a6");
    public static readonly string DefaultValueMetricName = "value";
}

public enum AccessRole
{
    PrismaDecisionUser
}

public enum IssueType
{
    Unassigned,
    Decision,
    Uncertainty,
    Fact,
    Utility
}

public enum ObjectiveType
{
    Strategic,
    Fundamental,
    Mean
}

public enum ProjectRoleType
{
    DecisionMaker,
    Facilitator,
    Member
}

public enum Boundary
{
    In,
    On,
    Out
}

public enum DecisionHierarchy
{
    Policy,
    Focus,
    Tactical
}

public enum NodeState
{
    Option,
    Outcome
}

using System;

namespace PrismaApi.Domain.Constants;

public static class DomainConstants
{
    public const int MaxShortStringLength = 1000;
    public const int MaxLongStringLength = 6000;
    public const int FloatPrecision = 53;
    public const int DecimalPlaces = 14;
    public const float DefaultStrokeWidth = 8;
    public const int MaxOpacity = 100;
    public const int MinOpacity = 0;
    public const int DefaultTextSize = 24;
    public static readonly Guid DefaultValueMetricId =
        Guid.Parse("288e0811-7ab6-5d24-b80c-9fa925b848a6");
    public static readonly string DefaultValueMetricName = "value";
    public const double DefaultRestrictionValue = 1.0;
}

public static class SqlCustomConstraintNames
{
    public const string RestrictionParentConstraintName = "CK_RestrictionEntry_Parent";
    public const string RestrictionChildConstraintName = "CK_RestrictionEntry_Child";
}

public static class AppConstants
{
    public const string CurrentUserKey = "CurrentUser";
    public const string PublicUsernameHeader = "X-Username";
}

public static class GraphApiConstants
{
    public const string ConsistencyLevelHeader = "ConsistencyLevel";
    public const string ConsistencyLevelEventual = "eventual";
    public const int DefaultSearchTop = 100;
    public static readonly string[] UserSearchSelectFields = ["id", "displayName", "mail", "userPrincipalName"];
}

public static class AppRoles
{
    public const string PrismaDecisionUser = "PrismaDecisionUser";
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

public enum BoardNodeTypes
{
    Rectangle,
    Freehand,
    Text,
    Arrow,
    Circle,
    Issue
}

public enum BoardNodeStrokeStyles
{
    Solid,
    Dashed,
    Dotted,
}

public static class ExceptionMessages
{
    public const string MinimumFacilitatorRequirement = "Projects must have at least one Facilitator.";
};

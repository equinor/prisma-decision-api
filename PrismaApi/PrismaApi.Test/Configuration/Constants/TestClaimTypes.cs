namespace PrismaApi.Test.Configuration.Constants;

public class TestClaimTypes
{
    public const string UserPrincipalName =
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";

    public const string AzureUniquePersonId =
        "http://schemas.microsoft.com/identity/claims/objectidentifier";

    public const string ApplicationId = "appid";

    public const string Mail = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";

    public const string Scope = "http://schemas.microsoft.com/identity/claims/scope";
}

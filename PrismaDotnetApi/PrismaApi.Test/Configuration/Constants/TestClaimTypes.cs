using Microsoft.Identity.Web;

namespace PrismaApi.Test.Configuration.Constants;

public class TestClaimTypes
{
    public const string AzureUniquePersonId = ClaimConstants.Oid;

    public const string ApplicationId = "appid";

    public const string Scope = ClaimConstants.Scp;
}

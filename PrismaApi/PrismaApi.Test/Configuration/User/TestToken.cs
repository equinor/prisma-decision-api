namespace PrismaApi.Test.Configuration.User;

public class TestToken
{
    public Guid? AzureUniqueId { get; set; }
    public string? Name { get; set; }
    public string? Upn { get; set; }
    public string[]? Roles { get; set; }
    public string[]? Scopes { get; set; }
    public bool IsAppToken { get; set; }
    public Guid? AppId { get; set; }
}

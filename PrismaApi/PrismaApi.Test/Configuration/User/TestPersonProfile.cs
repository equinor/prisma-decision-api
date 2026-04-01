namespace PrismaApi.Test.Configuration.User;

public class TestPersonProfile
{
    public Guid? AzureUniqueId { get; set; }
    public string? Name { get; set; }
    public string? Mail { get; set; }
    public string[]? Roles { get; set; }
    public string[]? Scopes { get; set; }
}

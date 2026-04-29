using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using PrismaApi.Test.Configuration.Constants;
using PrismaApi.Test.Configuration.User;

namespace PrismaApi.Test.Configuration.Schemas.IntegrationTest;

internal class IntegrationTestAuthHandler : AuthenticationHandler<IntegrationTestAuthOptions>
{
    private readonly IConfiguration _config;

    public IntegrationTestAuthHandler(IOptionsMonitor<IntegrationTestAuthOptions> options,
        IConfiguration config,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) =>
        _config = config;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var claims = GatherTestUserClaims();

            var testIdentity =
                new ClaimsIdentity(claims, IntegrationTestAuthDefaults.AuthenticationScheme);
            var testUser = new ClaimsPrincipal(testIdentity);

            var ticket = new AuthenticationTicket(testUser, new AuthenticationProperties(),
                IntegrationTestAuthDefaults.AuthenticationScheme);

            // Don't think there is any scenario we want to return 401, as if headers is set, the user is requested.
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            return Task.FromResult(AuthenticateResult.Fail(ex));
        }
    }

    private IEnumerable<Claim> GatherTestUserClaims()
    {
        TestToken tokenDeserialized;

        try
        {
            var token = Request.Headers["Authorization"].ToString();
            var tokenPart = token.Split(' ')[1];
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(tokenPart));
            tokenDeserialized = JsonSerializer.Deserialize<TestToken>(decoded)!;
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Unable to extract test auth token from [Authorization] header. See inner exception for details.",
                ex);
        }

        var uniqueId = tokenDeserialized.Id;
        var authType = tokenDeserialized.IsAppToken ? AuthType.Application : AuthType.Delegated;

        var claims = new List<Claim> { new(TestClaimTypes.AzureUniquePersonId, uniqueId!) };

        if (tokenDeserialized.Scopes != null)
        {
            foreach (var scp in tokenDeserialized.Scopes)
            {
                claims.Add(new Claim(TestClaimTypes.Scope, scp));
            }
        }

        switch (authType)
        {
            case AuthType.Delegated:
                {
                    AddNameClaims(claims, tokenDeserialized.Name!);
                    if (tokenDeserialized.Roles != null)
                    {
                        foreach (var role in tokenDeserialized.Roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }
                    }
                }


                break;
            case AuthType.Application:
                if (tokenDeserialized.Roles != null)
                {
                    foreach (var role in tokenDeserialized.Roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }

                if (tokenDeserialized.AppId.HasValue)
                {
                    claims.Add(new Claim(TestClaimTypes.ApplicationId,
                        $"{tokenDeserialized.AppId.Value}"));
                }
                else
                {
                    // Try look for configured
                    var appId = _config[IntegrationTestEnvVariables.DefaultAppid];

                    if (string.IsNullOrEmpty(appId))
                    {
                        throw new InvalidOperationException(
                            $"Application test users must include an appid claim or set the env variable {IntegrationTestEnvVariables.DefaultAppid}");
                    }

                    claims.Add(new Claim(TestClaimTypes.ApplicationId, $"{appId}"));
                }

                break;
        }

        return claims;
    }

    private static void AddNameClaims(List<Claim> claims, string fullName)
    {
        var tokens = fullName.Split(' ');
        if (tokens.Length > 1)
        {
            claims.Add(new Claim(ClaimTypes.Surname, tokens.Last()));

            var givenName = string.Join(" ", tokens.Take(tokens.Length - 1));
            claims.Add(new Claim(ClaimTypes.GivenName, givenName));
            claims.Add(new Claim(ClaimTypes.Name, fullName));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Name, fullName));
        }
    }

    private enum AuthType
    {
        Application,
        Delegated
    }
}

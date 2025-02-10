using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace App.Utils;

// Custom Authentication Handler
public class MyCustomAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{

    // Constructor where we inject TimeProvider instead of ISystemClock
    public MyCustomAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                                         ILoggerFactory logger,
                                         UrlEncoder encoder
                                         )
        : base(options, logger, encoder)
    {
    }

    // Custom logic for authentication (this is just a placeholder)
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = Context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
        }

        // Here, you would typically validate the token (e.g., using JWT)
        if (token == "valid-token") // Simulate a valid token for demonstration
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "test-user"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Bearer");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
    }
}
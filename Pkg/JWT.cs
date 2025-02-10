using System.Text;
using System.Text.Json.Serialization;
using JWT.Algorithms;
using JWT.Builder;
using Newtonsoft.Json;

namespace App.Pkg;

public class JWTClaim
{
    [JsonPropertyName("exp")]
    public long Expiration { get; set; }

    [JsonPropertyName("username")]
    public required string Username { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }
}

public interface IJWTPkg
{
    (string, DateTimeOffset) GenerateJwtToken(string username, string type);
    public JWTClaim? DecodeJwtToken(string token);
}

public class JWTPkg(string secretKey, int expiresInMinutes) : IJWTPkg
{
    public string SecretKey { get; } = secretKey;
    public int ExpiresInMinutes { get; } = expiresInMinutes;

    public (string, DateTimeOffset) GenerateJwtToken(string username, string type)
    {
        var minute = ExpiresInMinutes;
        if (type == "refresh_token")
            minute = ExpiresInMinutes * 30;
        var exp = DateTimeOffset.UtcNow.AddMinutes(minute);
        var token = JwtBuilder.Create()
                       .WithAlgorithm(new HMACSHA256Algorithm())
                       .WithSecret(Encoding.UTF8.GetBytes(SecretKey))
                       .AddClaim("exp", exp.ToUnixTimeSeconds())
                       .AddClaim("username", username)
                       .AddClaim("type", type)
                       .Encode();
        return (token, exp);
    }

    public JWTClaim? DecodeJwtToken(string token)
    {
        var json = JwtBuilder.Create()
                                    .WithAlgorithm(new HMACSHA256Algorithm())
                                    .WithSecret(Encoding.UTF8.GetBytes(SecretKey))
                                    .MustVerifySignature()
                                    .Decode(token);
        var claim = JsonConvert.DeserializeObject<JWTClaim>(json);
        return claim;
    }
}
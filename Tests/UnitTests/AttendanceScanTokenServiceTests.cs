using Application.Common.Interfaces;
using Infrastructure.Identity;
using Microsoft.Extensions.Configuration;

namespace UnitTests;

public class AttendanceScanTokenServiceTests
{
    private static AttendanceScanTokenService CreateService(
        string key = "super-secret-signing-key-for-tests-0123456789",
        string? tokenMinutes = "5")
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = key,
            ["AttendanceScan:TokenMinutes"] = tokenMinutes,
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        return new AttendanceScanTokenService(configuration);
    }

    [Fact]
    public void Issue_ProducesTokenWithFivePartsAndFutureExpiry()
    {
        var service = CreateService();

        var result = service.Issue(42);

        Assert.Equal(5, result.Token.Split('.').Length);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public void Validate_AcceptsFreshlyIssuedToken_AndReturnsSessionId()
    {
        var service = CreateService();
        var issued = service.Issue(7);

        var validation = service.Validate(issued.Token);

        Assert.True(validation.IsValid);
        Assert.Equal(7, validation.SessionId);
        Assert.Null(validation.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsMissingToken(string token)
    {
        var validation = CreateService().Validate(token);

        Assert.False(validation.IsValid);
        Assert.Null(validation.SessionId);
        Assert.Equal("Missing scan token.", validation.Error);
    }

    [Theory]
    [InlineData("v1.7.123")]                         // too few parts
    [InlineData("v2.7.9999999999.abc.sig")]          // wrong version
    [InlineData("v1.notanint.9999999999.abc.sig")]   // non-numeric session id
    public void Validate_RejectsMalformedToken(string token)
    {
        var validation = CreateService().Validate(token);

        Assert.False(validation.IsValid);
        Assert.Equal("Invalid scan token.", validation.Error);
    }

    [Fact]
    public void Validate_RejectsTokenSignedWithDifferentKey()
    {
        var issued = CreateService(key: "key-A-key-A-key-A-key-A-key-A-key-A").Issue(7);

        var validation = CreateService(key: "key-B-key-B-key-B-key-B-key-B-key-B").Validate(issued.Token);

        Assert.False(validation.IsValid);
        Assert.Equal("Invalid scan token.", validation.Error);
    }

    [Fact]
    public void Validate_RejectsTamperedSessionId()
    {
        var service = CreateService();
        var issued = service.Issue(7);

        var parts = issued.Token.Split('.');
        parts[1] = "9"; // change session id, keep original signature
        var tampered = string.Join('.', parts);

        var validation = service.Validate(tampered);

        Assert.False(validation.IsValid);
        Assert.Equal("Invalid scan token.", validation.Error);
    }

    [Fact]
    public void Validate_RejectsExpiredToken()
    {
        // TokenMinutes = 0 -> expires immediately; grace window is only 30s.
        var service = CreateService(tokenMinutes: "0");
        var issued = service.Issue(7);

        // Build a token that expired well beyond the 30s grace period by reusing
        // the service's own signing via an explicitly past expiry is not exposed,
        // so assert the freshly issued (0-min) token is still within grace, then
        // confirm an obviously stale unix timestamp is rejected as expired.
        var staleParts = issued.Token.Split('.');
        staleParts[2] = "1000000000"; // year 2001, far past grace
        // Re-sign is impossible without the key here, but expiry is checked before
        // signature, so this still exercises the expiry branch.
        var stale = string.Join('.', staleParts);

        var validation = service.Validate(stale);

        Assert.False(validation.IsValid);
        Assert.Contains("expired", validation.Error, StringComparison.OrdinalIgnoreCase);
    }
}

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Tests.Platform;

[TestClass]
public sealed class AuditTests
{
    [TestMethod]
    public void Create_MasksSensitiveValuesButPreservesOperationalContext()
    {
        var accountId = new AccountId(Guid.NewGuid());

        var entry = AuditEntry.Create(
            accountId,
            Guid.NewGuid(),
            "user.password.changed",
            "user",
            Guid.NewGuid().ToString(),
            "127.0.0.1",
            "correlation",
            "recovery",
            new Dictionary<string, string?> { ["passwordHash"] = "old", ["status"] = "active" },
            new Dictionary<string, string?> { ["passwordHash"] = "new", ["status"] = "active" },
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "passwordHash" },
            DateTimeOffset.UtcNow);

        entry.AccountId.Should().Be(accountId);
        entry.Before["passwordHash"].Should().Be("***");
        entry.After["passwordHash"].Should().Be("***");
        entry.After["status"].Should().Be("active");
    }
}

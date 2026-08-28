using Mova.Domain.Entities;
using Xunit;

namespace Mova.UnitTests.Domain.Entities;

public class AuditLogTests
{
    [Fact]
    public void Create_WithValidData_ReturnsAuditLog()
    {
        var userId = Guid.NewGuid();
        var complexId = Guid.NewGuid();
        var entityId = Guid.NewGuid().ToString();

        var auditLog = AuditLog.Create(userId, complexId, "Court.Create", "Court", entityId);

        Assert.NotEqual(Guid.Empty, auditLog.Id);
        Assert.Equal(userId, auditLog.UserId);
        Assert.Equal(complexId, auditLog.SportsComplexId);
        Assert.Equal("Court.Create", auditLog.Action);
        Assert.Equal("Court", auditLog.EntityType);
        Assert.Equal(entityId, auditLog.EntityId);
        Assert.True(auditLog.CreatedAt <= DateTime.UtcNow);
        Assert.Null(auditLog.Metadata);
    }

    [Fact]
    public void Create_WithNullUserIdAndComplexId_AllowsNullValues()
    {
        var entityId = Guid.NewGuid().ToString();

        var auditLog = AuditLog.Create(null, null, "System.Startup", "System", entityId);

        Assert.Null(auditLog.UserId);
        Assert.Null(auditLog.SportsComplexId);
        Assert.Equal("System.Startup", auditLog.Action);
    }

    [Fact]
    public void Create_WithMetadata_SerializesAsJson()
    {
        var metadata = new { name = "Court 1", surface = "Synthetic" };

        var auditLog = AuditLog.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Court.Create",
            "Court",
            Guid.NewGuid().ToString(),
            metadata);

        Assert.NotNull(auditLog.Metadata);
        Assert.Contains("Court 1", auditLog.Metadata);
        Assert.Contains("Synthetic", auditLog.Metadata);
    }

    [Fact]
    public void Create_WithNullMetadata_LeavesMetadataNull()
    {
        var auditLog = AuditLog.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Court.Create",
            "Court",
            Guid.NewGuid().ToString(),
            null);

        Assert.Null(auditLog.Metadata);
    }

    [Theory]
    [InlineData("", "Court", "entity-id")]
    [InlineData("  ", "Court", "entity-id")]
    public void Create_WithEmptyAction_ThrowsArgumentException(string action, string entityType, string entityId)
    {
        Assert.Throws<ArgumentException>(() =>
            AuditLog.Create(Guid.NewGuid(), Guid.NewGuid(), action, entityType, entityId));
    }

    [Theory]
    [InlineData("Court.Create", "", "entity-id")]
    [InlineData("Court.Create", "  ", "entity-id")]
    public void Create_WithEmptyEntityType_ThrowsArgumentException(string action, string entityType, string entityId)
    {
        Assert.Throws<ArgumentException>(() =>
            AuditLog.Create(Guid.NewGuid(), Guid.NewGuid(), action, entityType, entityId));
    }

    [Theory]
    [InlineData("Court.Create", "Court", "")]
    [InlineData("Court.Create", "Court", "  ")]
    public void Create_WithEmptyEntityId_ThrowsArgumentException(string action, string entityType, string entityId)
    {
        Assert.Throws<ArgumentException>(() =>
            AuditLog.Create(Guid.NewGuid(), Guid.NewGuid(), action, entityType, entityId));
    }

    [Fact]
    public void Create_TrimsWhitespaceFromStringFields()
    {
        var auditLog = AuditLog.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "  Court.Create  ",
            "  Court  ",
            "  entity-id  ");

        Assert.Equal("Court.Create", auditLog.Action);
        Assert.Equal("Court", auditLog.EntityType);
        Assert.Equal("entity-id", auditLog.EntityId);
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        var log1 = AuditLog.Create(Guid.NewGuid(), null, "Action1", "Type1", "id1");
        var log2 = AuditLog.Create(Guid.NewGuid(), null, "Action2", "Type2", "id2");

        Assert.NotEqual(log1.Id, log2.Id);
    }
}

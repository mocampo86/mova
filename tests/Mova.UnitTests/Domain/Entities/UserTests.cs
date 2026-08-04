using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Domain.Exceptions;
using Xunit;

namespace Mova.UnitTests.Domain.Entities;

public class UserTests
{
    [Fact]
    public void CreateFromGoogle_WithValidData_ReturnsActiveUser()
    {
        var id = Guid.NewGuid();
        var user = User.CreateFromGoogle(id, "google-subject", "user@test.com", "John Doe");

        Assert.Equal(id, user.Id);
        Assert.Equal("google-subject", user.GoogleSubjectId);
        Assert.Equal("user@test.com", user.Email);
        Assert.Equal("John Doe", user.FullName);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.False(user.PhoneVerified);
        Assert.Null(user.PhoneNumber);
    }

    [Theory]
    [InlineData("", "email@test.com", "Name")]
    [InlineData("sub", "", "Name")]
    [InlineData("sub", "email@test.com", "")]
    public void CreateFromGoogle_WithInvalidData_ThrowsException(string subject, string email, string name)
    {
        Assert.ThrowsAny<ArgumentException>(() => User.CreateFromGoogle(Guid.NewGuid(), subject, email, name));
    }

    [Fact]
    public void UpdateProfile_WithNewName_UpdatesFullNameAndUpdatedAt()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), "sub", "email@test.com", "John Doe");

        user.UpdateProfile("Jane Doe");

        Assert.Equal("Jane Doe", user.FullName);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void CompleteProfile_WithPhoneNumber_SetsPhoneAndUnverified()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), "sub", "email@test.com", "John Doe");

        user.CompleteProfile("+541112345678");

        Assert.Equal("+541112345678", user.PhoneNumber);
        Assert.False(user.PhoneVerified);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void Block_SetsStatusToBlocked()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), "sub", "email@test.com", "John Doe");

        user.Block();

        Assert.Equal(UserStatus.Blocked, user.Status);
    }

    [Fact]
    public void CreateFromGoogle_AssignsUserRole()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), "sub", "email@test.com", "John Doe");

        Assert.Contains(Role.User, user.Roles);
    }

    [Fact]
    public void AddRole_WithNewRole_AddsRole()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), "sub", "email@test.com", "John Doe");

        user.AddRole(Role.ComplexAdmin);

        Assert.Contains(Role.ComplexAdmin, user.Roles);
    }

    [Fact]
    public void AddRole_WithDuplicateRole_DoesNotDuplicate()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), "sub", "email@test.com", "John Doe");

        user.AddRole(Role.User);
        user.AddRole(Role.User);

        Assert.Single(user.Roles, r => r == Role.User);
    }

    [Fact]
    public void HasRole_WithAssignedRole_ReturnsTrue()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), "sub", "email@test.com", "John Doe");

        user.AddRole(Role.SuperAdmin);

        Assert.True(user.HasRole(Role.SuperAdmin));
    }
}

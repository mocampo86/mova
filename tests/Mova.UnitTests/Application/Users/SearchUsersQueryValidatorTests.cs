using FluentValidation.TestHelper;
using Mova.Application.Users.Queries;
using Mova.Application.Users.Validators;
using Xunit;

namespace Mova.UnitTests.Application.Users;

public sealed class SearchUsersQueryValidatorTests
{
    private readonly SearchUsersQueryValidator _validator = new();

    [Fact]
    public void Validate_ValidQuery_Passes()
    {
        var query = new SearchUsersQuery(Guid.NewGuid(), "john", 1, 10);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptySearch_IsValid()
    {
        var query = new SearchUsersQuery(Guid.NewGuid(), null, 1, 10);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShortSearch_Fails()
    {
        var query = new SearchUsersQuery(Guid.NewGuid(), "j", 1, 10);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Search);
    }

    [Fact]
    public void Validate_LongSearch_Fails()
    {
        var query = new SearchUsersQuery(Guid.NewGuid(), new string('a', 101), 1, 10);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Search);
    }

    [Theory]
    [InlineData("john@example.com")]
    [InlineData("+54 11 1234 5678")]
    [InlineData("John Doe")]
    [InlineData("user_name")]
    [InlineData("(123) 456-7890")]
    public void Validate_AllowedCharacters_Passes(string search)
    {
        var query = new SearchUsersQuery(Guid.NewGuid(), search, 1, 10);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.Search);
    }

    [Theory]
    [InlineData("john<script>")]
    [InlineData("SELECT * FROM Users")]
    [InlineData("user;drop")]
    [InlineData("user@example!com")]
    public void Validate_InvalidCharacters_Fails(string search)
    {
        var query = new SearchUsersQuery(Guid.NewGuid(), search, 1, 10);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Search);
    }

    [Fact]
    public void Validate_EmptySportsComplexId_Fails()
    {
        var query = new SearchUsersQuery(Guid.Empty, "john", 1, 10);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.SportsComplexId);
    }

    [Fact]
    public void Validate_PageLessThanOne_Fails()
    {
        var query = new SearchUsersQuery(Guid.NewGuid(), "john", 0, 10);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_PageSizeOutOfRange_Fails(int pageSize)
    {
        var query = new SearchUsersQuery(Guid.NewGuid(), "john", 1, pageSize);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
}

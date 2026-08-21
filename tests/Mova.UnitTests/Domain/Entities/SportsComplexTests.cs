using Mova.Domain.Entities;
using Mova.Domain.Enums;

namespace Mova.UnitTests.Domain.Entities;

public class SportsComplexTests
{
    [Fact]
    public void Create_WithValidData_ReturnsActiveComplexWithTimestamps()
    {
        var sportsComplex = SportsComplex.Create(
            "Club Padel",
            "A premium padel club",
            "Av. Libertador 1234",
            "Buenos Aires",
            -34.6m,
            -58.3m,
            "+54 11 1234 5678",
            "contact@clubpadel.com");

        Assert.NotEqual(Guid.Empty, sportsComplex.Id);
        Assert.Equal("Club Padel", sportsComplex.Name);
        Assert.Equal("A premium padel club", sportsComplex.Description);
        Assert.Equal("Av. Libertador 1234", sportsComplex.Address);
        Assert.Equal("Buenos Aires", sportsComplex.City);
        Assert.Equal(-34.6m, sportsComplex.Latitude);
        Assert.Equal(-58.3m, sportsComplex.Longitude);
        Assert.Equal("+54 11 1234 5678", sportsComplex.PhoneNumber);
        Assert.Equal("contact@clubpadel.com", sportsComplex.Email);
        Assert.Equal(ComplexStatus.Active, sportsComplex.Status);
        Assert.True(sportsComplex.CreatedAt > DateTime.MinValue);
        Assert.Null(sportsComplex.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ThrowsArgumentException(string? name)
    {
        var exception = name is null
            ? Assert.Throws<ArgumentNullException>(() => SportsComplex.Create(
                name!,
                "Description",
                "Address",
                "City",
                null,
                null,
                "+54 11 1234 5678",
                "email@test.com"))
            : Assert.Throws<ArgumentException>(() => SportsComplex.Create(
                name!,
                "Description",
                "Address",
                "City",
                null,
                null,
                "+54 11 1234 5678",
                "email@test.com"));

        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Create_WithInvalidLatitude_ThrowsArgumentException(decimal latitude)
    {
        var exception = Assert.Throws<ArgumentException>(() => SportsComplex.Create(
            "Name",
            "Description",
            "Address",
            "City",
            latitude,
            null,
            "+54 11 1234 5678",
            "email@test.com"));

        Assert.Equal("latitude", exception.ParamName);
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void Create_WithInvalidLongitude_ThrowsArgumentException(decimal longitude)
    {
        var exception = Assert.Throws<ArgumentException>(() => SportsComplex.Create(
            "Name",
            "Description",
            "Address",
            "City",
            null,
            longitude,
            "+54 11 1234 5678",
            "email@test.com"));

        Assert.Equal("longitude", exception.ParamName);
    }

    [Fact]
    public void Update_WithValidData_SetsUpdatedAtAndPreservesStatus()
    {
        var sportsComplex = SportsComplex.Create(
            "Old Name",
            "Old description",
            "Old address",
            "Old city",
            null,
            null,
            "+54 11 1111 1111",
            "old@test.com");

        sportsComplex.Update(
            "New Name",
            "New description",
            "New address",
            "New city",
            -34.6m,
            -58.3m,
            "+54 11 9999 9999",
            "new@test.com");

        Assert.Equal("New Name", sportsComplex.Name);
        Assert.Equal("New description", sportsComplex.Description);
        Assert.Equal("New address", sportsComplex.Address);
        Assert.Equal("New city", sportsComplex.City);
        Assert.Equal(-34.6m, sportsComplex.Latitude);
        Assert.Equal(-58.3m, sportsComplex.Longitude);
        Assert.Equal("+54 11 9999 9999", sportsComplex.PhoneNumber);
        Assert.Equal("new@test.com", sportsComplex.Email);
        Assert.Equal(ComplexStatus.Active, sportsComplex.Status);
        Assert.NotNull(sportsComplex.UpdatedAt);
    }

    [Fact]
    public void Deactivate_WhenActive_SetsStatusToInactive()
    {
        var sportsComplex = SportsComplex.Create(
            "Name",
            "Description",
            "Address",
            "City",
            null,
            null,
            "+54 11 1234 5678",
            "email@test.com");

        sportsComplex.Deactivate();

        Assert.Equal(ComplexStatus.Inactive, sportsComplex.Status);
        Assert.NotNull(sportsComplex.UpdatedAt);
    }

    [Fact]
    public void Activate_WhenInactive_SetsStatusToActive()
    {
        var sportsComplex = SportsComplex.Create(
            "Name",
            "Description",
            "Address",
            "City",
            null,
            null,
            "+54 11 1234 5678",
            "email@test.com");

        sportsComplex.Deactivate();
        sportsComplex.Activate();

        Assert.Equal(ComplexStatus.Active, sportsComplex.Status);
    }
}

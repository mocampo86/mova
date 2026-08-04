using Mova.Application.Complexes.Commands;
using Mova.Application.Complexes.Handlers;
using Mova.Application.Common.Exceptions;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Authentication;
using Xunit;

namespace Mova.UnitTests.Application.Complexes;

public class CreateComplexHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();
    private readonly FakeComplexAdministratorRepository _complexAdministratorRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateComplexHandler CreateHandler() =>
        new(_userRepository, _sportsComplexRepository, _complexAdministratorRepository, _unitOfWork);

    [Fact]
    public async Task HandleAsync_WithValidData_CreatesComplexAndLinksAdministrator()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), "google-subject", "user@test.com", "John Doe");
        await _userRepository.AddAsync(user);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new CreateComplexCommand(
            user.Id,
            "Club Padel",
            "A premium padel club",
            "Av. Libertador 1234",
            "Buenos Aires",
            -34.6m,
            -58.3m,
            "+54 11 1234 5678",
            "contact@clubpadel.com"));

        Assert.NotNull(result);
        Assert.Equal("Club Padel", result.Name);
        Assert.Equal("Active", result.Status);

        var administrators = await _complexAdministratorRepository.GetByUserIdAsync(user.Id);
        Assert.Single(administrators);
        var administrator = administrators.First();
        Assert.Equal(result.Id, administrator.SportsComplexId);
        Assert.Equal(user.Id, administrator.UserId);
        Assert.Equal(Role.ComplexAdmin, administrator.Role);

        Assert.True(user.HasRole(Role.ComplexAdmin));
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ThrowsNotFoundException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new CreateComplexCommand(
            Guid.NewGuid(),
            "Club Padel",
            "Description",
            "Address",
            "City",
            null,
            null,
            "+54 11 1234 5678",
            "email@test.com")));
    }
}

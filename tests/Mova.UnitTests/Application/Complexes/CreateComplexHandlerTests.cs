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
    private readonly FakeBusinessHoursRepository _businessHours = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateComplexHandler CreateHandler() =>
        new(_userRepository, _sportsComplexRepository, _complexAdministratorRepository, _businessHours, _unitOfWork);

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
            "contact@clubpadel.com",
            "America/Montevideo"));

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

        var hours = await _businessHours.GetBySportsComplexIdAsync(result.Id);
        Assert.Equal(7, hours.Count);

        Assert.Equal(1, _unitOfWork.TransactionCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
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
            "email@test.com",
            "America/Montevideo")));

        Assert.Equal(0, _unitOfWork.TransactionCallCount);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenBusinessHoursRepositoryThrows_DoesNotCallSaveChangesAndThrows()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), "google-subject", "user@test.com", "John Doe");
        await _userRepository.AddAsync(user);

        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateComplexHandler(
            _userRepository,
            _sportsComplexRepository,
            _complexAdministratorRepository,
            new ThrowingBusinessHoursRepository(),
            unitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new CreateComplexCommand(
            user.Id,
            "Club Padel",
            "A premium padel club",
            "Av. Libertador 1234",
            "Buenos Aires",
            -34.6m,
            -58.3m,
            "+54 11 1234 5678",
            "contact@clubpadel.com",
            "America/Montevideo")));

        Assert.Equal(1, unitOfWork.TransactionCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenComplexAdministratorRepositoryThrows_DoesNotCallSaveChangesAndThrows()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), "google-subject", "user@test.com", "John Doe");
        await _userRepository.AddAsync(user);

        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateComplexHandler(
            _userRepository,
            _sportsComplexRepository,
            new ThrowingComplexAdministratorRepository(),
            _businessHours,
            unitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new CreateComplexCommand(
            user.Id,
            "Club Padel",
            "A premium padel club",
            "Av. Libertador 1234",
            "Buenos Aires",
            -34.6m,
            -58.3m,
            "+54 11 1234 5678",
            "contact@clubpadel.com",
            "America/Montevideo")));

        Assert.Equal(1, unitOfWork.TransactionCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }
}

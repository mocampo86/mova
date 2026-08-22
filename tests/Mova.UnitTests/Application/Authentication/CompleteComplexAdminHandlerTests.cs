using Mova.Application.Authentication.Commands;
using Mova.Application.Authentication.Handlers;
using Mova.Application.Common.Exceptions;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.UnitTests.Application.Complexes;
using Xunit;

namespace Mova.UnitTests.Application.Authentication;

public class CompleteComplexAdminHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeSportsComplexRepository _sportsComplexRepository = new();
    private readonly FakeComplexAdministratorRepository _complexAdministratorRepository = new();
    private readonly FakeBusinessHoursRepository _businessHours = new();
    private readonly FakeJwtTokenService _jwtTokenService = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CompleteComplexAdminHandler CreateHandler() =>
        new(
            _userRepository,
            _sportsComplexRepository,
            _complexAdministratorRepository,
            _businessHours,
            _jwtTokenService,
            _unitOfWork);

    [Fact]
    public async Task HandleAsync_WithValidData_CreatesPendingComplexAndLinksAdministrator()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), "google-subject", "admin@test.com", "Admin User");
        await _userRepository.AddAsync(user);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new CompleteComplexAdminCommand(
            user.Id,
            "+54 11 1234 5678",
            "Club Padel",
            "A premium padel club",
            "Av. Libertador 1234",
            "Buenos Aires",
            -34.6m,
            -58.3m,
            "+54 11 1234 5678",
            "contact@clubpadel.com"));

        Assert.NotNull(result);
        Assert.Equal("test-access-token", result.AccessToken);
        Assert.Equal("+54 11 1234 5678", result.User.PhoneNumber);
        Assert.False(result.RequiresProfileCompletion);

        var complex = await _sportsComplexRepository.GetByIdAsync(result.ComplexId);
        Assert.NotNull(complex);
        Assert.Equal("Club Padel", complex!.Name);
        Assert.Equal(ComplexStatus.Pending, complex.Status);

        var administrators = await _complexAdministratorRepository.GetByUserIdAsync(user.Id);
        Assert.Single(administrators);
        Assert.Equal(complex.Id, administrators.First().SportsComplexId);
        Assert.Equal(user.Id, administrators.First().UserId);
        Assert.Equal(Role.ComplexAdmin, administrators.First().Role);

        Assert.True(user.HasRole(Role.ComplexAdmin));

        Assert.NotNull(_jwtTokenService.LastRoles);
        Assert.Contains(Role.User.ToString(), _jwtTokenService.LastRoles);
        Assert.Contains(Role.ComplexAdmin.ToString(), _jwtTokenService.LastRoles);

        Assert.NotNull(_jwtTokenService.LastComplexAssociations);
        Assert.Contains(
            _jwtTokenService.LastComplexAssociations,
            a => a.ComplexId == complex.Id && a.Role == Role.ComplexAdmin.ToString());

        var hours = await _businessHours.GetBySportsComplexIdAsync(complex.Id);
        Assert.Equal(7, hours.Count);

        Assert.Equal(1, _unitOfWork.TransactionCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ThrowsNotFoundException()
    {
        var handler = CreateHandler();

        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new CompleteComplexAdminCommand(
            Guid.NewGuid(),
            "+54 11 1234 5678",
            "Club Padel",
            "Description",
            "Address",
            "City",
            null,
            null,
            "+54 11 1234 5678",
            "email@test.com")));

        Assert.Equal(0, _unitOfWork.TransactionCallCount);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenBusinessHoursRepositoryThrows_DoesNotCallSaveChangesAndThrows()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), "google-subject", "admin@test.com", "Admin User");
        await _userRepository.AddAsync(user);

        var unitOfWork = new FakeUnitOfWork();
        var handler = new CompleteComplexAdminHandler(
            _userRepository,
            _sportsComplexRepository,
            _complexAdministratorRepository,
            new ThrowingBusinessHoursRepository(),
            _jwtTokenService,
            unitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new CompleteComplexAdminCommand(
            user.Id,
            "+54 11 1234 5678",
            "Club Padel",
            "A premium padel club",
            "Av. Libertador 1234",
            "Buenos Aires",
            -34.6m,
            -58.3m,
            "+54 11 1234 5678",
            "contact@clubpadel.com")));

        Assert.Equal(1, unitOfWork.TransactionCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenComplexAdministratorRepositoryThrows_DoesNotCallSaveChangesAndThrows()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), "google-subject", "admin@test.com", "Admin User");
        await _userRepository.AddAsync(user);

        var unitOfWork = new FakeUnitOfWork();
        var handler = new CompleteComplexAdminHandler(
            _userRepository,
            _sportsComplexRepository,
            new ThrowingComplexAdministratorRepository(),
            _businessHours,
            _jwtTokenService,
            unitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(new CompleteComplexAdminCommand(
            user.Id,
            "+54 11 1234 5678",
            "Club Padel",
            "A premium padel club",
            "Av. Libertador 1234",
            "Buenos Aires",
            -34.6m,
            -58.3m,
            "+54 11 1234 5678",
            "contact@clubpadel.com")));

        Assert.Equal(1, unitOfWork.TransactionCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_WithExistingUser_AddsComplexAdminRoleAndAssociation()
    {
        var user = User.CreateFromGoogle(Guid.NewGuid(), "google-subject", "admin@test.com", "Admin User");
        user.CompleteProfile("+54 11 0000 0000");
        user.AddRole(Role.ComplexAdmin);
        await _userRepository.AddAsync(user);

        var existingComplexId = Guid.NewGuid();
        var existingComplex = SportsComplex.Create(
            "Existing Club",
            "Description",
            "Address",
            "City",
            null,
            null,
            "+54 11 0000 0000",
            "existing@test.com",
            ComplexStatus.Pending);
        await _sportsComplexRepository.AddAsync(existingComplex);
        await _complexAdministratorRepository.AddAsync(
            ComplexAdministrator.Create(existingComplex.Id, user.Id, Role.ComplexAdmin));

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new CompleteComplexAdminCommand(
            user.Id,
            "+54 11 1234 5678",
            "Second Club",
            "A second club",
            "Av. Rivadavia 1234",
            "Buenos Aires",
            null,
            null,
            "+54 11 1234 5678",
            "second@test.com"));

        Assert.NotEqual(Guid.Empty, result.ComplexId);

        var administrators = await _complexAdministratorRepository.GetByUserIdAsync(user.Id);
        Assert.Equal(2, administrators.Count);

        Assert.NotNull(_jwtTokenService.LastComplexAssociations);
        Assert.Equal(2, _jwtTokenService.LastComplexAssociations!.Count);
    }
}

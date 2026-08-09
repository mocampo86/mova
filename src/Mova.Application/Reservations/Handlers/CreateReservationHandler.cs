using Mova.Application.Abstractions.Persistence;
using Mova.Application.Common.Exceptions;
using Mova.Application.Reservations.Commands;
using Mova.Contracts.Reservations;
using Mova.Domain.Entities;
using Mova.Domain.Enums;
using Mova.Domain.Exceptions;

namespace Mova.Application.Reservations.Handlers;

public sealed class CreateReservationHandler : ICreateReservationHandler
{
    private readonly ISportsComplexRepository _sportsComplexes;
    private readonly ICourtRepository _courts;
    private readonly IUserRepository _users;
    private readonly IBlockedUserRepository _blockedUsers;
    private readonly IReservationRepository _reservations;
    private readonly ICourtBlockRepository _courtBlocks;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReservationHandler(
        ISportsComplexRepository sportsComplexes,
        ICourtRepository courts,
        IUserRepository users,
        IBlockedUserRepository blockedUsers,
        IReservationRepository reservations,
        ICourtBlockRepository courtBlocks,
        IUnitOfWork unitOfWork)
    {
        _sportsComplexes = sportsComplexes;
        _courts = courts;
        _users = users;
        _blockedUsers = blockedUsers;
        _reservations = reservations;
        _courtBlocks = courtBlocks;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReservationInfo> HandleAsync(CreateReservationCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _sportsComplexes.GetByIdAsync(command.SportsComplexId, cancellationToken)
            ?? throw new NotFoundException("Sports complex not found.");

        var court = await _courts.GetByIdAsync(command.CourtId, cancellationToken);

        if (court is null || court.SportsComplexId != command.SportsComplexId)
        {
            throw new NotFoundException("Court not found.");
        }

        if (court.Status != CourtStatus.Active)
        {
            throw new ConflictException("The selected court is not active.");
        }

        var user = await _users.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null || user.Status != UserStatus.Active)
        {
            throw new NotFoundException("User not found.");
        }

        if (await _blockedUsers.IsUserBlockedAsync(command.SportsComplexId, command.UserId, cancellationToken))
        {
            throw new UserBlockedException();
        }

        if (await _reservations.HasOverlappingActiveReservationAsync(command.CourtId, command.StartAt, command.EndAt, cancellationToken: cancellationToken))
        {
            throw new ConflictException("The selected time is no longer available.");
        }

        var blocks = await _courtBlocks.GetForCourtAsync(command.CourtId, command.StartAt, command.EndAt, cancellationToken);

        if (blocks.Count > 0)
        {
            throw new ConflictException("The selected time is blocked.");
        }

        var reservation = Reservation.Create(
            command.SportsComplexId,
            command.CourtId,
            command.UserId,
            command.StartAt,
            command.EndAt,
            ReservationSource.Admin,
            command.Notes);

        reservation.Confirm();

        await _reservations.AddAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ReservationMapper.ToInfo(reservation);
    }
}

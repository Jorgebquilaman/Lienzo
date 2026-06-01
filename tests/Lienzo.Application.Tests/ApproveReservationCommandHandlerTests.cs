using AutoMapper;
using FluentAssertions;
using Lienzo.Application.Commands.ApproveReservation;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using Moq;
using Xunit;

namespace Lienzo.Application.Tests;

public class ApproveReservationCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly ApproveReservationCommandHandler _handler;
    private static readonly Guid AdminId = Guid.NewGuid();
    private static readonly Guid ReservationId = Guid.NewGuid();

    public ApproveReservationCommandHandlerTests()
    {
        _currentUserMock.Setup(x => x.UserId).Returns(AdminId);
        _currentUserMock.Setup(x => x.Role).Returns(UserRole.Admin.ToString());
        _handler = new ApproveReservationCommandHandler(
            _unitOfWorkMock.Object, _mapperMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ApprovesReservation()
    {
        var reservation = CreatePendingReservation();
        var command = new ApproveReservationCommand(ReservationId);

        SetupReservationRepository(reservation);
        _mapperMock.Setup(x => x.Map<ReservationDto>(It.IsAny<Reservation>()))
            .Returns(new ReservationDto(ReservationId, Guid.NewGuid(), "", Guid.NewGuid(), "",
                "Test", null, DateTime.UtcNow, new TimeOnly(8, 0), new TimeOnly(10, 0),
                ReservationStatus.Approved.ToString(), DateTime.UtcNow, AdminId));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        reservation.Status.Should().Be(ReservationStatus.Approved);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReservationNotFound_ReturnsFailure()
    {
        var command = new ApproveReservationCommand(ReservationId);

        _unitOfWorkMock.Setup(x => x.Reservations.GetByIdAsync(ReservationId))
            .ReturnsAsync((Reservation?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_AlreadyApproved_ReturnsFailure()
    {
        var reservation = CreatePendingReservation();
        reservation.Approve(AdminId);
        var command = new ApproveReservationCommand(ReservationId);

        SetupReservationRepository(reservation);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("INVALID_STATE");
    }

    private static Reservation CreatePendingReservation()
    {
        return Reservation.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Test Title", null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            new TimeOnly(8, 0), new TimeOnly(10, 0));
    }

    private void SetupReservationRepository(Reservation reservation)
    {
        _unitOfWorkMock.Setup(x => x.Reservations.GetByIdAsync(ReservationId))
            .ReturnsAsync(reservation);
    }
}

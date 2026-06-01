using FluentAssertions;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Events;
using MediatR;
using Xunit;

namespace Lienzo.Domain.Tests;

public class ReservationTests
{
    private static readonly Guid ClassroomId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid AdminId = Guid.NewGuid();
    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    [Fact]
    public void Create_ValidParameters_SetsPendingStatus()
    {
        var reservation = Reservation.Create(
            ClassroomId, UserId, "Test Title", null,
            FutureDate, new TimeOnly(8, 0), new TimeOnly(10, 0));

        reservation.Status.Should().Be(ReservationStatus.Pending);
        reservation.Title.Should().Be("Test Title");
        reservation.ClassroomId.Should().Be(ClassroomId);
        reservation.UserId.Should().Be(UserId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrEmptyTitle_ThrowsException(string? title)
    {
        var act = () => Reservation.Create(
            ClassroomId, UserId, title!, null,
            FutureDate, new TimeOnly(8, 0), new TimeOnly(10, 0));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_StartTimeAfterEndTime_ThrowsException()
    {
        var act = () => Reservation.Create(
            ClassroomId, UserId, "Test", null,
            FutureDate, new TimeOnly(10, 0), new TimeOnly(8, 0));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Approve_PendingReservation_SetsApproved()
    {
        var reservation = CreateValidReservation();

        reservation.Approve(AdminId);

        reservation.Status.Should().Be(ReservationStatus.Approved);
        reservation.ApprovedById.Should().Be(AdminId);
    }

    [Fact]
    public void Approve_AlreadyApproved_ThrowsInvalidOperation()
    {
        var reservation = CreateValidReservation();
        reservation.Approve(AdminId);

        var act = () => reservation.Approve(AdminId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_PendingReservation_SetsRejected()
    {
        var reservation = CreateValidReservation();

        reservation.Reject(AdminId);

        reservation.Status.Should().Be(ReservationStatus.Rejected);
        reservation.ApprovedById.Should().Be(AdminId);
    }

    [Fact]
    public void Reject_AlreadyRejected_ThrowsInvalidOperation()
    {
        var reservation = CreateValidReservation();
        reservation.Reject(AdminId);

        var act = () => reservation.Reject(AdminId);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_PendingReservation_SetsCancelled()
    {
        var reservation = CreateValidReservation();

        reservation.Cancel();

        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ApprovedReservation_SetsCancelled()
    {
        var reservation = CreateValidReservation();
        reservation.Approve(AdminId);

        reservation.Cancel();

        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public void Cancel_RejectedReservation_ThrowsInvalidOperation()
    {
        var reservation = CreateValidReservation();
        reservation.Reject(AdminId);

        var act = () => reservation.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Create_RaisesDomainEvent()
    {
        var reservation = CreateValidReservation();

        reservation.DomainEvents.Should().ContainSingle(e =>
            e is ReservationCreatedEvent);
    }

    [Fact]
    public void Approve_RaisesStatusChangedEvent()
    {
        var reservation = CreateValidReservation();
        reservation.ClearDomainEvents();

        reservation.Approve(AdminId);

        reservation.DomainEvents.Should().ContainSingle(e =>
            e is ReservationStatusChangedEvent);
    }

    [Fact]
    public void Reject_RaisesStatusChangedEvent()
    {
        var reservation = CreateValidReservation();
        reservation.ClearDomainEvents();

        reservation.Reject(AdminId);

        reservation.DomainEvents.Should().ContainSingle(e =>
            e is ReservationStatusChangedEvent);
    }

    [Fact]
    public void Cancel_RaisesStatusChangedEvent()
    {
        var reservation = CreateValidReservation();
        reservation.ClearDomainEvents();

        reservation.Cancel();

        reservation.DomainEvents.Should().ContainSingle(e =>
            e is ReservationStatusChangedEvent);
    }

    private static Reservation CreateValidReservation()
    {
        return Reservation.Create(
            ClassroomId, UserId, "Valid Title", null,
            FutureDate, new TimeOnly(8, 0), new TimeOnly(10, 0));
    }
}

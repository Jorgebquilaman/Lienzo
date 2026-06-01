using FluentAssertions;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Domain.ValueObjects;
using Xunit;

namespace Lienzo.Domain.Tests;

public class ClassroomTests
{
    private static readonly Guid BuildingId = Guid.NewGuid();
    private static readonly DateOnly TestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    [Fact]
    public void IsAvailable_NoConflicts_ReturnsTrue()
    {
        var classroom = CreateValidClassroom();
        var time = new TimeRange(new TimeOnly(8, 0), new TimeOnly(10, 0));

        var result = classroom.IsAvailable(TestDate, time, []);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsAvailable_WithConflict_ReturnsFalse()
    {
        var classroom = CreateValidClassroom();
        var conflict = CreateReservation(classroom.Id, new TimeOnly(8, 0), new TimeOnly(10, 0));
        var time = new TimeRange(new TimeOnly(9, 0), new TimeOnly(11, 0));

        var result = classroom.IsAvailable(TestDate, time, [conflict]);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_ClassroomInactive_ReturnsFalse()
    {
        var classroom = CreateValidClassroom();
        classroom.Deactivate();
        var time = new TimeRange(new TimeOnly(8, 0), new TimeOnly(10, 0));

        var result = classroom.IsAvailable(TestDate, time, []);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_IgnoresCancelledReservations()
    {
        var classroom = CreateValidClassroom();
        var cancelled = CreateReservation(classroom.Id, new TimeOnly(8, 0), new TimeOnly(10, 0),
            ReservationStatus.Cancelled);
        var time = new TimeRange(new TimeOnly(9, 0), new TimeOnly(11, 0));

        var result = classroom.IsAvailable(TestDate, time, [cancelled]);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsAvailable_IgnoresRejectedReservations()
    {
        var classroom = CreateValidClassroom();
        var rejected = CreateReservation(classroom.Id, new TimeOnly(8, 0), new TimeOnly(10, 0),
            ReservationStatus.Rejected);
        var time = new TimeRange(new TimeOnly(9, 0), new TimeOnly(11, 0));

        var result = classroom.IsAvailable(TestDate, time, [rejected]);

        result.Should().BeTrue();
    }

    [Fact]
    public void Create_ValidParameters_SetsProperties()
    {
        var classroom = new Classroom("Room 101", BuildingId, 1, 30, ClassroomType.General,
            ["Projector", "Whiteboard"], "image.jpg");

        classroom.Name.Should().Be("Room 101");
        classroom.BuildingId.Should().Be(BuildingId);
        classroom.Floor.Should().Be(1);
        classroom.Capacity.Should().Be(30);
        classroom.Type.Should().Be(ClassroomType.General);
        classroom.Features.Should().BeEquivalentTo(["Projector", "Whiteboard"]);
        classroom.ImageUrl.Should().Be("image.jpg");
        classroom.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateDetails_ModifiesProperties()
    {
        var classroom = CreateValidClassroom();

        classroom.UpdateDetails("Room 202", 2, 40, ClassroomType.Dance,
            ["Mirrors", "Speakers"], "new.jpg");

        classroom.Name.Should().Be("Room 202");
        classroom.Floor.Should().Be(2);
        classroom.Capacity.Should().Be(40);
        classroom.Type.Should().Be(ClassroomType.Dance);
        classroom.Features.Should().BeEquivalentTo(["Mirrors", "Speakers"]);
        classroom.ImageUrl.Should().Be("new.jpg");
    }

    [Fact]
    public void ActivateDeactivate_TogglesIsActive()
    {
        var classroom = CreateValidClassroom();

        classroom.Deactivate();
        classroom.IsActive.Should().BeFalse();

        classroom.Activate();
        classroom.IsActive.Should().BeTrue();
    }

    private static Classroom CreateValidClassroom()
    {
        return new Classroom("Room 101", BuildingId, 1, 30, ClassroomType.General);
    }

    private static Reservation CreateReservation(Guid classroomId, TimeOnly start, TimeOnly end,
        ReservationStatus status = ReservationStatus.Pending)
    {
        var reservation = Reservation.Create(
            classroomId, Guid.NewGuid(), "Test", null,
            TestDate, start, end);

        if (status == ReservationStatus.Approved)
            reservation.Approve(Guid.NewGuid());
        else if (status == ReservationStatus.Rejected)
            reservation.Reject(Guid.NewGuid());
        else if (status == ReservationStatus.Cancelled)
            reservation.Cancel();

        return reservation;
    }
}

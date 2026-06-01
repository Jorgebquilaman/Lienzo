using AutoMapper;
using FluentAssertions;
using Lienzo.Application.Commands.CreateReservation;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using Moq;
using Xunit;

namespace Lienzo.Application.Tests;

public class CreateReservationCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly CreateReservationCommandHandler _handler;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ClassroomId = Guid.NewGuid();

    public CreateReservationCommandHandlerTests()
    {
        _currentUserMock.Setup(x => x.UserId).Returns(UserId);
        _handler = new CreateReservationCommandHandler(
            _unitOfWorkMock.Object, _mapperMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesReservation()
    {
        var classroom = CreateActiveClassroom();
        var request = CreateValidRequest();
        var command = new CreateReservationCommand(request);

        SetupClassroomRepository(classroom);
        _unitOfWorkMock.Setup(x => x.Reservations.HasConflictAsync(
                ClassroomId, It.IsAny<DateTime>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), null))
            .ReturnsAsync(false);
        _unitOfWorkMock.Setup(x => x.Reservations.AddAsync(It.IsAny<Reservation>()))
            .ReturnsAsync((Reservation r) => r);
        _mapperMock.Setup(x => x.Map<ReservationDto>(It.IsAny<Reservation>()))
            .Returns(new ReservationDto(Guid.NewGuid(), ClassroomId, classroom.Name, UserId, "",
                request.Title, request.Description, request.Date, request.StartTime, request.EndTime,
                ReservationStatus.Pending.ToString(), DateTime.UtcNow, null));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConflictExists_ReturnsFailure()
    {
        var classroom = CreateActiveClassroom();
        var command = new CreateReservationCommand(CreateValidRequest());

        SetupClassroomRepository(classroom);
        _unitOfWorkMock.Setup(x => x.Reservations.HasConflictAsync(
                ClassroomId, It.IsAny<DateTime>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), null))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("CONFLICT");
    }

    [Fact]
    public async Task Handle_ClassroomNotFound_ReturnsFailure()
    {
        var command = new CreateReservationCommand(CreateValidRequest());

        _unitOfWorkMock.Setup(x => x.Classrooms.GetWithReservationsAsync(ClassroomId))
            .ReturnsAsync((Classroom?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ClassroomInactive_ReturnsFailure()
    {
        var classroom = CreateActiveClassroom();
        classroom.Deactivate();
        var command = new CreateReservationCommand(CreateValidRequest());

        SetupClassroomRepository(classroom);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("INACTIVE");
    }

    private static CreateReservationRequest CreateValidRequest()
    {
        return new CreateReservationRequest(
            ClassroomId, "Test Title", null,
            DateTime.UtcNow.AddDays(1), new TimeOnly(8, 0), new TimeOnly(10, 0));
    }

    private static Classroom CreateActiveClassroom()
    {
        return new Classroom("Room 101", Guid.NewGuid(), 1, 30, ClassroomType.General);
    }

    private void SetupClassroomRepository(Classroom classroom)
    {
        _unitOfWorkMock.Setup(x => x.Classrooms.GetWithReservationsAsync(ClassroomId))
            .ReturnsAsync(classroom);
    }
}

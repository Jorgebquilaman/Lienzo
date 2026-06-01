using AutoMapper;
using FluentAssertions;
using Lienzo.Application.Commands.CreateAnnouncement;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using Moq;
using Xunit;

namespace Lienzo.Application.Tests;

public class SendAnnouncementCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly CreateAnnouncementCommandHandler _handler;
    private static readonly Guid TeacherId = Guid.NewGuid();

    public SendAnnouncementCommandHandlerTests()
    {
        _currentUserMock.Setup(x => x.UserId).Returns(TeacherId);
        _currentUserMock.Setup(x => x.Email).Returns("teacher@lienzo.edu");
        _handler = new CreateAnnouncementCommandHandler(
            _unitOfWorkMock.Object, _mapperMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesAnnouncement()
    {
        var request = CreateValidRequest();
        var command = new CreateAnnouncementCommand(request);

        _unitOfWorkMock.Setup(x => x.Announcements.AddAsync(It.IsAny<Announcement>()))
            .ReturnsAsync((Announcement a) => a);
        _mapperMock.Setup(x => x.Map<AnnouncementDto>(It.IsAny<Announcement>()))
            .Returns(new AnnouncementDto(Guid.NewGuid(), "teacher@lienzo.edu",
                request.Title, request.Body, request.Type, request.TargetAudience,
                null, DateTime.UtcNow, false, null));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyTitle_ReturnsFailure()
    {
        var request = CreateValidRequest() with { Title = "" };
        var command = new CreateAnnouncementCommand(request);

        _unitOfWorkMock.Setup(x => x.Announcements.AddAsync(It.IsAny<Announcement>()))
            .ReturnsAsync((Announcement a) => a);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_InvalidTargetAudience_ReturnsFailure()
    {
        var request = CreateValidRequest() with { TargetAudience = "InvalidAudience" };
        var command = new CreateAnnouncementCommand(request);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    private static CreateAnnouncementRequest CreateValidRequest()
    {
        return new CreateAnnouncementRequest(
            "Test Title", "Test Body", "General", "All", null, null);
    }
}

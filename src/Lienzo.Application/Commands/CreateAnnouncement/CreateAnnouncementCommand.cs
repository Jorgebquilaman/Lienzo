using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Commands.CreateAnnouncement;

public record CreateAnnouncementCommand(CreateAnnouncementRequest Request) : IRequest<Result<AnnouncementDto>>;

public class CreateAnnouncementCommandHandler : IRequestHandler<CreateAnnouncementCommand, Result<AnnouncementDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public CreateAnnouncementCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<Result<AnnouncementDto>> Handle(CreateAnnouncementCommand command, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AnnouncementType>(command.Request.Type, true, out var type))
            return Result<AnnouncementDto>.Failure("Invalid announcement type", "INVALID_TYPE");

        if (!Enum.TryParse<TargetAudience>(command.Request.TargetAudience, true, out var targetAudience))
            return Result<AnnouncementDto>.Failure("Invalid target audience", "INVALID_TYPE");

        var announcement = new Announcement(
            _currentUser.UserId,
            command.Request.Title,
            command.Request.Body,
            type,
            targetAudience,
            command.Request.RelatedReservationId);

        if (targetAudience == TargetAudience.SpecificStudents &&
            command.Request.SpecificStudentIds?.Count > 0)
        {
            announcement.AddRecipients(command.Request.SpecificStudentIds);
        }

        await _unitOfWork.Announcements.AddAsync(announcement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<AnnouncementDto>(announcement);
        dto.TeacherName = _currentUser.Email;
        dto.IsRead = false;

        return Result<AnnouncementDto>.Success(dto);
    }
}

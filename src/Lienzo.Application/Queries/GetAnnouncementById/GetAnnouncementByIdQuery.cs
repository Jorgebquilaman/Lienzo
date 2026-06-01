using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetAnnouncementById;

public record GetAnnouncementByIdQuery(Guid Id) : IRequest<Result<AnnouncementDto>>;

public class GetAnnouncementByIdQueryHandler : IRequestHandler<GetAnnouncementByIdQuery, Result<AnnouncementDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public GetAnnouncementByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<Result<AnnouncementDto>> Handle(GetAnnouncementByIdQuery query, CancellationToken cancellationToken)
    {
        var announcement = await _unitOfWork.Announcements.GetByIdAsync(query.Id);
        if (announcement is null || announcement.IsDeleted)
            return Result<AnnouncementDto>.Failure("Announcement not found", "NOT_FOUND");

        var isRead = false;
        DateTime? readAt = null;

        if (_currentUser.Role == UserRole.Student.ToString())
        {
            var recipient = announcement.Recipients.FirstOrDefault(r => r.StudentId == _currentUser.UserId);
            if (recipient is not null)
            {
                isRead = recipient.IsRead;
                readAt = recipient.ReadAt;
            }
        }

        var dto = _mapper.Map<AnnouncementDto>(announcement);
        dto.TeacherName = announcement.TeacherId.ToString();
        dto.IsRead = isRead;
        dto.ReadAt = readAt;

        return Result<AnnouncementDto>.Success(dto);
    }
}

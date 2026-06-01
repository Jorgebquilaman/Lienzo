using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetNotifications;

public record GetNotificationsQuery : IRequest<Result<List<NotificationDto>>>;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<List<NotificationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public GetNotificationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<Result<List<NotificationDto>>> Handle(GetNotificationsQuery query, CancellationToken cancellationToken)
    {
        var notifications = await _unitOfWork.Notifications.GetUserNotificationsAsync(_currentUser.UserId);
        var dtos = _mapper.Map<List<NotificationDto>>(notifications
            .OrderByDescending(n => n.CreatedAt)
            .ToList());
        return Result<List<NotificationDto>>.Success(dtos);
    }
}

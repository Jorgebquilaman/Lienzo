using AutoMapper;
using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetPendingSurveys;

public record GetPendingSurveysQuery : IRequest<Result<List<ReservationDto>>>;

public class GetPendingSurveysQueryHandler : IRequestHandler<GetPendingSurveysQuery, Result<List<ReservationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthService _authService;

    public GetPendingSurveysQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IAuthService authService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _authService = authService;
    }

    public async Task<Result<List<ReservationDto>>> Handle(GetPendingSurveysQuery query, CancellationToken ct)
    {
        var surveys = await _unitOfWork.ClassroomSurveys.GetAllAsync();
        var ratedReservationIds = surveys.Where(s => s.UserId == _currentUser.UserId)
            .Select(s => s.ReservationId)
            .ToHashSet();

        IEnumerable<Domain.Entities.Reservation> reservations;
        if (_currentUser.Role == UserRole.Teacher.ToString())
        {
            var myReservations = await _unitOfWork.Reservations.GetUserReservationsAsync(_currentUser.UserId);
            var allRes = await _unitOfWork.Reservations.GetAllWithDetailsAsync();
            var actividades = await _unitOfWork.Actividades.GetAllWithDetailsAsync();
            var userIdStr = _currentUser.UserId.ToString();
            var actividadIds = actividades
                .Where(a => a.Docentes.Any(d => d.DocenteId == userIdStr))
                .Select(a => a.Id)
                .ToHashSet();
            var docenteReservations = allRes.Where(r => r.ActividadId.HasValue && actividadIds.Contains(r.ActividadId.Value));
            reservations = myReservations.Concat(docenteReservations).DistinctBy(r => r.Id);
        }
        else
        {
            reservations = await _unitOfWork.Reservations.GetUserReservationsAsync(_currentUser.UserId);
        }

        var today = DateOnly.FromDateTime(DateTime.Now);
        var pending = reservations
            .Where(r => !r.IsDeleted && r.Status == ReservationStatus.Approved
                && (r.Date < today || (r.Date == today && r.EndTime <= TimeOnly.FromDateTime(DateTime.Now)))
                && !ratedReservationIds.Contains(r.Id))
            .OrderByDescending(r => r.Date)
            .ThenBy(r => r.StartTime)
            .ToList();

        var dtos = _mapper.Map<List<ReservationDto>>(pending);

        var usersResult = await _authService.GetAllUsersAsync();
        if (usersResult.IsSuccess)
        {
            var userMap = usersResult.Value.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}");
            foreach (var dto in dtos)
            {
                if (userMap.TryGetValue(dto.UserId, out var name))
                    dto.UserName = name;
            }
        }

        return Result<List<ReservationDto>>.Success(dtos);
    }
}

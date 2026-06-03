using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Interfaces;
using MediatR;

namespace Lienzo.Application.Queries.GetAllActividades;

public record GetAllActividadesQuery : IRequest<Result<List<ActividadDto>>>;

public class GetAllActividadesQueryHandler : IRequestHandler<GetAllActividadesQuery, Result<List<ActividadDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public GetAllActividadesQueryHandler(IUnitOfWork unitOfWork, IAuthService authService, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _currentUser = currentUser;
    }

    public async Task<Result<List<ActividadDto>>> Handle(GetAllActividadesQuery query, CancellationToken ct)
    {
        var items = await _unitOfWork.Actividades.GetAllWithDetailsAsync();
        var userIds = items.SelectMany(a => a.Docentes.Select(d => d.DocenteId)).Distinct().ToList();

        var userNames = new Dictionary<string, string>();
        var usersResult = await _authService.GetAllUsersAsync();
        if (usersResult.IsSuccess)
        {
            userNames = usersResult.Value
                .Where(u => userIds.Contains(u.Id.ToString()))
                .ToDictionary(u => u.Id.ToString(), u => $"{u.FirstName} {u.LastName}");
        }

        var dtos = items.Select(a =>
        {
            var docNames = string.Join(", ", a.Docentes
                .Select(d => userNames.TryGetValue(d.DocenteId, out var name) ? name : d.DocenteId));
            return new ActividadDto(
                a.Id, a.Nombre, a.CodigoMateria,
                a.PeriodoId, a.Periodo?.Nombre,
                a.CarreraId, a.Carrera?.Nombre,
                a.AulaId, a.Aula?.Name,
                a.DiaSemana, a.HoraInicio?.ToString("HH:mm"), a.HoraFin?.ToString("HH:mm"),
                a.Docentes.Select(d => d.DocenteId).ToList(),
                docNames,
                a.ComisionNombre,
                a.DiasDictado
            );
        }).ToList();

        if (_currentUser.Role == UserRole.Teacher.ToString())
        {
            var userIdStr = _currentUser.UserId.ToString();
            dtos = dtos.Where(a => a.DocenteIds.Contains(userIdStr)).ToList();
        }

        return Result<List<ActividadDto>>.Success(dtos);
    }
}

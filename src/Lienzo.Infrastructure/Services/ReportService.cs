using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly LienzoDbContext _context;

    public ReportService(LienzoDbContext context) => _context = context;

    public async Task<Result<UsageReportResponse>> GetUsageReportAsync(UsageReportFilter filter)
    {
        var reservationsQuery = _context.Reservations
            .Include(r => r.Classroom)
            .Where(r => !r.IsDeleted);

        if (filter.FromDate.HasValue)
            reservationsQuery = reservationsQuery.Where(r => r.Date >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            reservationsQuery = reservationsQuery.Where(r => r.Date <= filter.ToDate.Value);
        if (filter.ClassroomId.HasValue)
            reservationsQuery = reservationsQuery.Where(r => r.ClassroomId == filter.ClassroomId.Value);

        if (filter.CarreraId.HasValue || !string.IsNullOrEmpty(filter.DocenteId))
            reservationsQuery = reservationsQuery.Where(r => r.ActividadId != null);

        var reservations = await reservationsQuery.ToListAsync();

        IEnumerable<IGrouping<string, Reservation>> groups;

        if (filter.CarreraId.HasValue)
        {
            reservations = reservations.Where(r => r.ActividadId.HasValue).ToList();
            var actividadIds = reservations.Select(r => r.ActividadId!.Value).Distinct().ToList();
            var actividads = await _context.Actividades
                .Include(a => a.Carrera)
                .Where(a => actividadIds.Contains(a.Id) && a.CarreraId == filter.CarreraId.Value)
                .ToListAsync();
            var validIds = actividads.Select(a => a.Id).ToHashSet();
            reservations = reservations.Where(r => r.ActividadId.HasValue && validIds.Contains(r.ActividadId.Value)).ToList();
            groups = reservations.GroupBy(r => actividads.FirstOrDefault(a => a.Id == r.ActividadId)?.Carrera?.Nombre ?? "Sin carrera");
        }
        else if (!string.IsNullOrEmpty(filter.DocenteId))
        {
            var actividadDocentes = await _context.Set<ActividadDocente>()
                .Where(ad => ad.DocenteId == filter.DocenteId && !ad.IsDeleted)
                .ToListAsync();
            var actividadIds = actividadDocentes.Select(ad => ad.ActividadId).ToHashSet();
            reservations = reservations.Where(r => r.ActividadId.HasValue && actividadIds.Contains(r.ActividadId.Value)).ToList();
            var user = await _context.Users.FindAsync([Guid.Parse(filter.DocenteId)]);
            var docenteName = user is not null ? $"{user.FirstName} {user.LastName}" : filter.DocenteId;
            groups = reservations.GroupBy(_ => docenteName);
        }
        else
        {
            groups = reservations.GroupBy(r => r.Classroom?.Name ?? "Sin aula");
        }

        var items = groups.Select(g =>
        {
            var total = g.Count();
            var approved = g.Count(r => r.Status == ReservationStatus.Approved);
            var cancelled = g.Count(r => r.Status == ReservationStatus.Cancelled);
            var totalHours = g.Sum(r => (r.EndTime.ToTimeSpan() - r.StartTime.ToTimeSpan()).TotalHours);
            var cancellationRate = total > 0 ? (double)cancelled / total * 100.0 : 0.0;
            var usagePercentage = total > 0 ? (double)approved / total * 100.0 : 0.0;
            return new UsageReportItem(g.Key, total, approved, cancelled, 0,
                Math.Round(totalHours, 2), Math.Round(cancellationRate, 2), Math.Round(usagePercentage, 2));
        }).ToList();

        var grandTotal = reservations.Count;
        var grandHours = reservations.Sum(r => (r.EndTime.ToTimeSpan() - r.StartTime.ToTimeSpan()).TotalHours);
        var overallCancellation = grandTotal > 0
            ? Math.Round((double)reservations.Count(r => r.Status == ReservationStatus.Cancelled) / grandTotal * 100.0, 2)
            : 0.0;

        return Result<UsageReportResponse>.Success(new(items, grandTotal, Math.Round(grandHours, 2), overallCancellation));
    }

    public async Task<Result<DemandMetricsResponse>> GetDemandMetricsAsync(DateOnly? fromDate, DateOnly? toDate)
    {
        var reservationsQuery = _context.Reservations
            .Include(r => r.Classroom)
            .Where(r => !r.IsDeleted && r.Status == ReservationStatus.Approved);

        if (fromDate.HasValue)
            reservationsQuery = reservationsQuery.Where(r => r.Date >= fromDate.Value);
        if (toDate.HasValue)
            reservationsQuery = reservationsQuery.Where(r => r.Date <= toDate.Value);

        var reservations = await reservationsQuery.ToListAsync();

        var byHourAndType = reservations
            .GroupBy(r => new { r.StartTime.Hour, Type = r.Classroom?.Type ?? ClassroomType.General })
            .Select(g => new DemandMetricItem(
                g.Key.Hour,
                g.Key.Type.ToString(),
                g.Count(),
                reservations.Count > 0 ? Math.Round((double)g.Count() / reservations.Count * 100.0, 2) : 0.0))
            .OrderBy(d => d.Hour)
            .ThenBy(d => d.ClassroomType)
            .ToList();

        var peakHours = byHourAndType
            .GroupBy(d => d.Hour)
            .Select(g => new DemandMetricItem(
                g.Key, "Todos",
                g.Sum(x => x.ReservationCount),
                reservations.Count > 0 ? Math.Round((double)g.Sum(x => x.ReservationCount) / reservations.Count * 100.0, 2) : 0.0))
            .OrderByDescending(d => d.ReservationCount)
            .Take(5)
            .ToList();

        var totalDays = 1;
        if (fromDate.HasValue && toDate.HasValue)
            totalDays = toDate.Value.DayNumber - fromDate.Value.DayNumber + 1;

        var byType = reservations
            .GroupBy(r => r.Classroom?.Type ?? ClassroomType.General)
            .Select(g =>
            {
                var totalHours = g.Sum(r => (r.EndTime.ToTimeSpan() - r.StartTime.ToTimeSpan()).TotalHours);
                return new ClassroomDemandSummary(
                    g.Key.ToString(), g.Count(),
                    Math.Round(totalHours, 2),
                    totalDays * 12 > 0 ? Math.Round(totalHours / (totalDays * 12) * 100.0, 2) : 0.0);
            })
            .OrderByDescending(d => d.TotalReservations)
            .ToList();

        return Result<DemandMetricsResponse>.Success(new(byHourAndType, peakHours, byType));
    }

    public async Task<Result<UsageByProposalResponse>> GetUsageByProposalAsync(UsageByProposalFilter filter)
    {
        var reservationsQuery = _context.Reservations
            .Include(r => r.Classroom)
            .Where(r => !r.IsDeleted && r.ActividadId != null);

        if (filter.FromDate.HasValue)
            reservationsQuery = reservationsQuery.Where(r => r.Date >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            reservationsQuery = reservationsQuery.Where(r => r.Date <= filter.ToDate.Value);

        var reservations = await reservationsQuery.ToListAsync();

        IEnumerable<IGrouping<string, Reservation>> groups;

        if (filter.GroupBy == "docente")
        {
            var actividadIds = reservations.Select(r => r.ActividadId!.Value).Distinct().ToList();
            var actividadDocentes = await _context.Set<ActividadDocente>()
                .Where(ad => actividadIds.Contains(ad.ActividadId) && !ad.IsDeleted)
                .ToListAsync();
            var docMap = actividadDocentes
                .GroupBy(ad => ad.ActividadId)
                .ToDictionary(g => g.Key, g => g.Select(ad => ad.DocenteId).Distinct().ToList());

            var users = await _context.Users.ToListAsync();
            var userNameMap = users.ToDictionary(u => u.Id.ToString(), u => $"{u.FirstName} {u.LastName}");

            var flat = new List<(string DocenteName, Reservation Res)>();
            foreach (var r in reservations)
            {
                if (r.ActividadId.HasValue && docMap.TryGetValue(r.ActividadId.Value, out var docIds))
                {
                    foreach (var docId in docIds)
                    {
                        var name = userNameMap.GetValueOrDefault(docId, docId);
                        flat.Add((name, r));
                    }
                }
            }
            groups = flat.GroupBy(x => x.DocenteName, x => x.Res);
        }
        else
        {
            var actividadIds = reservations.Select(r => r.ActividadId!.Value).Distinct().ToList();
            var actividads = await _context.Actividades
                .Where(a => actividadIds.Contains(a.Id))
                .ToListAsync();
            var actividadNameMap = actividads.ToDictionary(a => a.Id, a => a.Nombre);
            groups = reservations.GroupBy(r => actividadNameMap.GetValueOrDefault(r.ActividadId!.Value, "Sin propuesta"));
        }

        var items = groups.Select(g =>
        {
            var total = g.Count();
            var approved = g.Count(r => r.Status == ReservationStatus.Approved);
            var cancelled = g.Count(r => r.Status == ReservationStatus.Cancelled);
            var totalHours = g.Sum(r => (r.EndTime.ToTimeSpan() - r.StartTime.ToTimeSpan()).TotalHours);
            var cancellationRate = total > 0 ? (double)cancelled / total * 100.0 : 0.0;
            var usagePercentage = total > 0 ? (double)approved / total * 100.0 : 0.0;
            return new UsageByProposalItem(g.Key, total, approved, cancelled,
                Math.Round(totalHours, 2), Math.Round(cancellationRate, 2), Math.Round(usagePercentage, 2));
        })
        .OrderByDescending(i => i.TotalReservations)
        .ToList();

        var grandTotal = reservations.Count;
        var grandHours = reservations.Sum(r => (r.EndTime.ToTimeSpan() - r.StartTime.ToTimeSpan()).TotalHours);
        var overallCancellation = grandTotal > 0
            ? Math.Round((double)reservations.Count(r => r.Status == ReservationStatus.Cancelled) / grandTotal * 100.0, 2)
            : 0.0;

        return Result<UsageByProposalResponse>.Success(new(items, grandTotal, Math.Round(grandHours, 2), overallCancellation));
    }
}

using Lienzo.Application.Common.Models;
using Lienzo.Application.DTOs;
using Lienzo.Application.Interfaces;
using Lienzo.Domain.Enums;
using Lienzo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly LienzoDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DashboardService(LienzoDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<DashboardStatsDto>> GetStatsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var totalClassrooms = await _context.Classrooms.CountAsync();
        var activeClassrooms = await _context.Classrooms.CountAsync(c => c.IsActive);
        var reservationsToday = await _context.Reservations
            .CountAsync(r => r.Date == today && r.Status != ReservationStatus.Cancelled && r.Status != ReservationStatus.Rejected);
        var pendingApprovals = await _context.Reservations
            .CountAsync(r => r.Status == ReservationStatus.Pending);

        var totalReservations = await _context.Reservations.CountAsync();
        var totalCapacity = await _context.Classrooms.SumAsync(c => c.Capacity);
        double occupancyRate = totalCapacity > 0
            ? Math.Round((double)totalReservations / totalCapacity * 100, 2)
            : 0;

        var totalUsers = await _context.Users.CountAsync();

        return Result<DashboardStatsDto>.Success(new DashboardStatsDto(
            totalClassrooms,
            activeClassrooms,
            reservationsToday,
            pendingApprovals,
            occupancyRate,
            totalUsers));
    }

    public async Task<Result<List<OccupancyHeatmapDto>>> GetOccupancyHeatmapAsync(DateTime? date = null)
    {
        var targetDate = date.HasValue
            ? DateOnly.FromDateTime(date.Value)
            : DateOnly.FromDateTime(DateTime.UtcNow);

        var reservations = await _context.Reservations
            .Where(r => r.Date == targetDate && r.Status == ReservationStatus.Approved)
            .ToListAsync();

        var heatmap = reservations
            .GroupBy(r => r.StartTime.Hour)
            .Select(g => new OccupancyHeatmapDto(
                targetDate.ToString("yyyy-MM-dd"),
                g.Key,
                g.Count()))
            .ToList();

        return Result<List<OccupancyHeatmapDto>>.Success(heatmap);
    }
}

using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Enums;
using Lienzo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lienzo.Infrastructure.Services;

public class ReservationReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationReminderService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    public ReservationReminderService(IServiceScopeFactory scopeFactory, ILogger<ReservationReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReservationReminderService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing reservation reminders");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessRemindersAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LienzoDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var tomorrow = today.AddDays(1);

        // 24-hour reminders: approved reservations for tomorrow
        var tomorrowReservations = await context.Reservations
            .Where(r => r.Date == tomorrow
                && r.Status == ReservationStatus.Approved
                && !r.IsDeleted
                && !context.Set<ReservationReminder>().Any(rr =>
                    rr.ReservationId == r.Id
                    && rr.ReminderType == ReminderType.TwentyFourHours
                    && !rr.IsDeleted))
            .ToListAsync(ct);

        foreach (var reservation in tomorrowReservations)
        {
            await notificationService.SendAsync(
                reservation.UserId,
                "Recordatorio: reserva mañana",
                $"Tienes una reserva mañana ({reservation.Date:dd/MM/yyyy}) de {reservation.StartTime:hh\\:mm} a {reservation.EndTime:hh\\:mm}.",
                "Info",
                reservation.Id,
                "Reservation");

            context.Set<ReservationReminder>().Add(new ReservationReminder(
                reservation.Id, reservation.UserId, ReminderType.TwentyFourHours));
        }

        // 30-minute reminders: approved reservations for today starting within the next 35 minutes
        var upcomingReservations = await context.Reservations
            .Where(r => r.Date == today
                && r.Status == ReservationStatus.Approved
                && !r.IsDeleted
                && !context.Set<ReservationReminder>().Any(rr =>
                    rr.ReservationId == r.Id
                    && rr.ReminderType == ReminderType.ThirtyMinutes
                    && !rr.IsDeleted))
            .ToListAsync(ct);

        var startWindow = TimeOnly.FromDateTime(now.AddMinutes(5));
        var endWindow = TimeOnly.FromDateTime(now.AddMinutes(35));

        foreach (var reservation in upcomingReservations)
        {
            if (reservation.StartTime >= startWindow && reservation.StartTime <= endWindow)
            {
                await notificationService.SendAsync(
                    reservation.UserId,
                    "Recordatorio: reserva en 30 minutos",
                    $"Tu reserva en {reservation.Classroom?.Name ?? "el aula"} comienza a las {reservation.StartTime:hh\\:mm}.",
                    "Warning",
                    reservation.Id,
                    "Reservation");

                context.Set<ReservationReminder>().Add(new ReservationReminder(
                    reservation.Id, reservation.UserId, ReminderType.ThirtyMinutes));
            }
        }

        if (tomorrowReservations.Count > 0 || upcomingReservations.Count > 0)
        {
            await context.SaveChangesAsync(ct);
            _logger.LogInformation("Sent {Count24h} 24h reminders and {Count30m} 30min reminders",
                tomorrowReservations.Count,
                upcomingReservations.Count(r => r.StartTime >= startWindow && r.StartTime <= endWindow));
        }
    }
}

using Lienzo.Domain.Common;
using Lienzo.Domain.Enums;

namespace Lienzo.Domain.Entities;

public class ReservationAccessory : BaseEntity
{
    public Guid ReservationId { get; private set; }
    public Reservation Reservation { get; private set; } = null!;
    public string Name { get; private set; }
    public AccessoryOrigin Origin { get; private set; }
    public bool IsRequested { get; private set; }
    public bool? IsGranted { get; private set; }

    private ReservationAccessory() { }

    public ReservationAccessory(Guid reservationId, string name, AccessoryOrigin origin)
    {
        Id = Guid.NewGuid();
        ReservationId = reservationId;
        Name = name;
        Origin = origin;
        IsRequested = false;
        IsGranted = null;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Request()
    {
        IsRequested = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Decide(bool granted)
    {
        IsGranted = granted;
        UpdatedAt = DateTime.UtcNow;
    }
}

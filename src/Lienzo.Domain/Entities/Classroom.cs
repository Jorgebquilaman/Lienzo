using Lienzo.Domain.Common;
using Lienzo.Domain.Enums;
using Lienzo.Domain.ValueObjects;

namespace Lienzo.Domain.Entities;

public class Classroom : BaseEntity
{
    private readonly List<Reservation> _reservations = [];

    public string Name { get; private set; }
    public Guid BuildingId { get; private set; }
    public Building Building { get; private set; } = null!;
    public int Floor { get; private set; }
    public int Capacity { get; private set; }
    public ClassroomType Type { get; private set; }
    public List<string> Features { get; private set; } = [];
    public bool IsActive { get; private set; }
    public string? ImageUrl { get; private set; }
    public float? MapPositionX { get; set; }
    public float? MapPositionY { get; set; }
    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();

    private Classroom() { }

    public Classroom(
        string name,
        Guid buildingId,
        int floor,
        int capacity,
        ClassroomType type,
        List<string>? features = null,
        string? imageUrl = null)
    {
        Id = Guid.NewGuid();
        SetName(name);
        BuildingId = buildingId;
        SetFloor(floor);
        SetCapacity(capacity);
        Type = type;
        Features = features ?? [];
        ImageUrl = imageUrl;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsAvailable(DateOnly date, TimeRange time, IEnumerable<Reservation> existingReservations)
    {
        if (!IsActive) return false;

        return !existingReservations.Any(r =>
            r.ClassroomId == Id &&
            r.Date == date &&
            r.Status != ReservationStatus.Cancelled &&
            r.Status != ReservationStatus.Rejected &&
            new TimeRange(r.StartTime, r.EndTime).OverlapsWith(time));
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(
        string name,
        int floor,
        int capacity,
        ClassroomType type,
        List<string>? features = null,
        string? imageUrl = null)
    {
        SetName(name);
        SetFloor(floor);
        SetCapacity(capacity);
        Type = type;
        Features = features ?? [];
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddFeature(string feature)
    {
        if (string.IsNullOrWhiteSpace(feature))
            throw new ArgumentException("Feature cannot be empty.", nameof(feature));

        if (!Features.Contains(feature))
        {
            Features.Add(feature);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveFeature(string feature)
    {
        if (Features.Remove(feature))
            UpdatedAt = DateTime.UtcNow;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Classroom name cannot be empty.", nameof(name));
        Name = name;
    }

    private void SetFloor(int floor)
    {
        if (floor < 0)
            throw new ArgumentException("Floor cannot be negative.", nameof(floor));
        Floor = floor;
    }

    private void SetCapacity(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));
        Capacity = capacity;
    }
}

using Lienzo.Domain.Common;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Events;

namespace Lienzo.Domain.Entities;

public class Reservation : BaseEntity
{
    public Guid ClassroomId { get; private set; }
    public Classroom Classroom { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public ReservationStatus Status { get; private set; }
    public Guid? ApprovedById { get; private set; }
    public Guid? RecurringGroupId { get; private set; }
    public string? RecurrenceRule { get; private set; }
    public Guid? ActividadId { get; private set; }
    public string? SourceEmailUid { get; private set; }
    public string? SourceEmailFrom { get; private set; }
    public string? SourceEmailSubject { get; private set; }
    public DateTime? SourceEmailDate { get; private set; }
    public string? EvidenceFilePath { get; private set; }
    public bool RequiresAccessoryConfirmation { get; private set; }
    public string? AccessoryConfirmationToken { get; private set; }
    public DateTime? AccessoriesConfirmedAt { get; private set; }
    private readonly List<ReservationAccessory> _reservationAccessories = [];
    public IReadOnlyCollection<ReservationAccessory> ReservationAccessories => _reservationAccessories.AsReadOnly();

    private Reservation() { }

    private Reservation(
        Guid classroomId,
        Guid userId,
        string title,
        string? description,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? recurringGroupId = null,
        string? recurrenceRule = null,
        Guid? actividadId = null,
        string? sourceEmailUid = null,
        string? sourceEmailFrom = null,
        string? sourceEmailSubject = null,
        DateTime? sourceEmailDate = null,
        string? evidenceFilePath = null)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time.");

        if (date < DateOnly.FromDateTime(DateTime.Now))
            throw new ArgumentException("Reservation date cannot be in the past.");

        Id = Guid.NewGuid();
        ClassroomId = classroomId;
        UserId = userId;
        SetTitle(title);
        Description = description;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        Status = ReservationStatus.Pending;
        RecurringGroupId = recurringGroupId;
        RecurrenceRule = recurrenceRule;
        ActividadId = actividadId;
        SourceEmailUid = sourceEmailUid;
        SourceEmailFrom = sourceEmailFrom;
        SourceEmailSubject = sourceEmailSubject;
        SourceEmailDate = sourceEmailDate;
        EvidenceFilePath = evidenceFilePath;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReservationCreatedEvent(
            Id,
            classroomId,
            userId,
            date.ToDateTime(TimeOnly.MinValue),
            startTime,
            endTime,
            DateTime.UtcNow));
    }

    public static Reservation Create(
        Guid classroomId,
        Guid userId,
        string title,
        string? description,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? recurringGroupId = null,
        string? recurrenceRule = null,
        Guid? actividadId = null,
        string? sourceEmailUid = null,
        string? sourceEmailFrom = null,
        string? sourceEmailSubject = null,
        DateTime? sourceEmailDate = null,
        string? evidenceFilePath = null)
    {
        return new Reservation(classroomId, userId, title, description, date, startTime, endTime, recurringGroupId, recurrenceRule, actividadId, sourceEmailUid, sourceEmailFrom, sourceEmailSubject, sourceEmailDate, evidenceFilePath);
    }

    public void Approve(Guid adminId)
    {
        if (Status != ReservationStatus.Pending)
            throw new InvalidOperationException($"Cannot approve a reservation with status '{Status}'. Only pending reservations can be approved.");

        Status = ReservationStatus.Approved;
        ApprovedById = adminId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReservationStatusChangedEvent(
            Id,
            ClassroomId,
            UserId,
            Status.ToString(),
            DateTime.UtcNow));
    }

    public void Reject(Guid adminId)
    {
        if (Status != ReservationStatus.Pending)
            throw new InvalidOperationException($"Cannot reject a reservation with status '{Status}'. Only pending reservations can be rejected.");

        Status = ReservationStatus.Rejected;
        ApprovedById = adminId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReservationStatusChangedEvent(
            Id,
            ClassroomId,
            UserId,
            Status.ToString(),
            DateTime.UtcNow));
    }

    public void Cancel()
    {
        if (Status is ReservationStatus.Rejected or ReservationStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel a reservation with status '{Status}'.");

        Status = ReservationStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReservationStatusChangedEvent(
            Id,
            ClassroomId,
            UserId,
            Status.ToString(),
            DateTime.UtcNow));
    }

    public void RequireAccessoryConfirmation(string token, IEnumerable<(string Name, AccessoryOrigin Origin)> accessories)
    {
        RequiresAccessoryConfirmation = true;
        AccessoryConfirmationToken = token;
        _reservationAccessories.Clear();
        foreach (var (name, origin) in accessories)
        {
            _reservationAccessories.Add(new ReservationAccessory(Id, name, origin));
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmAccessories(IEnumerable<string> requestedNames)
    {
        if (!RequiresAccessoryConfirmation || string.IsNullOrEmpty(AccessoryConfirmationToken))
            throw new InvalidOperationException("Esta reserva no requiere confirmación de accesorios.");

        if (AccessoriesConfirmedAt.HasValue)
            throw new InvalidOperationException("La confirmación de accesorios ya fue recibida.");

        foreach (var accessory in _reservationAccessories)
        {
            if (requestedNames.Contains(accessory.Name))
                accessory.Request();
        }

        AccessoriesConfirmedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecideAccessory(string name, bool granted)
    {
        var accessory = _reservationAccessories.FirstOrDefault(a => a.Name == name);
        if (accessory is null)
            throw new ArgumentException($"El accesorio '{name}' no pertenece a esta reserva.");
        accessory.Decide(granted);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string title, string? description)    {
        if (Status != ReservationStatus.Pending)
            throw new InvalidOperationException("Only pending reservations can be updated.");

        var previousTitle = Title;
        var previousDescription = Description;

        SetTitle(title);
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReservationUpdatedEvent(
            Id,
            UserId,
            previousTitle,
            previousDescription,
            DateTime.UtcNow));
    }

    public void ChangeClassroom(Guid newClassroomId)
    {
        if (Status != ReservationStatus.Approved)
            throw new InvalidOperationException("Solo reservas aprobadas pueden cambiarse de aula.");

        var oldClassroomId = ClassroomId;
        ClassroomId = newClassroomId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ReservationClassroomChangedEvent(
            Id,
            oldClassroomId,
            newClassroomId,
            UserId,
            DateTime.UtcNow));
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        Title = title;
    }
}

using Lienzo.Domain.Common;

namespace Lienzo.Domain.Entities;

public class ClassroomSurvey : BaseEntity
{
    public Guid ReservationId { get; private set; }
    public Reservation Reservation { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public decimal ConditionRating { get; private set; }
    public decimal EquipmentRating { get; private set; }
    public decimal CleanlinessRating { get; private set; }
    public decimal OverallRating { get; private set; }
    public string? Comment { get; private set; }

    private ClassroomSurvey() { }

    public ClassroomSurvey(
        Guid reservationId,
        Guid userId,
        decimal conditionRating,
        decimal equipmentRating,
        decimal cleanlinessRating,
        string? comment)
    {
        Id = Guid.NewGuid();
        ReservationId = reservationId;
        UserId = userId;
        SetRatings(conditionRating, equipmentRating, cleanlinessRating);
        Comment = comment;
        OverallRating = Math.Round((conditionRating + equipmentRating + cleanlinessRating) / 3, 1);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetRatings(decimal condition, decimal equipment, decimal cleanliness)
    {
        if (condition < 1 || condition > 5)
            throw new ArgumentException("Condition rating must be between 1 and 5");
        if (equipment < 1 || equipment > 5)
            throw new ArgumentException("Equipment rating must be between 1 and 5");
        if (cleanliness < 1 || cleanliness > 5)
            throw new ArgumentException("Cleanliness rating must be between 1 and 5");

        ConditionRating = Math.Round(condition, 1);
        EquipmentRating = Math.Round(equipment, 1);
        CleanlinessRating = Math.Round(cleanliness, 1);
    }
}

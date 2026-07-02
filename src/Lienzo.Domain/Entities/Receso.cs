using Lienzo.Domain.Common;

namespace Lienzo.Domain.Entities;

public class Receso : BaseEntity
{
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string Description { get; private set; } = string.Empty;

    private Receso() { }

    public Receso(DateOnly startDate, DateOnly endDate, string description)
    {
        Id = Guid.NewGuid();
        StartDate = startDate;
        EndDate = endDate;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

using Lienzo.Domain.Common;

namespace Lienzo.Domain.Entities;

public class Building : BaseEntity
{
    private readonly List<Classroom> _classrooms = [];

    public string Name { get; private set; }
    public string Address { get; private set; }
    public int FloorCount { get; private set; }
    public bool IsActive { get; private set; }
    public int? CodigoExterno { get; private set; }
    public string? FloorPlanUrl { get; private set; }
    public IReadOnlyCollection<Classroom> Classrooms => _classrooms.AsReadOnly();

    private Building() { }

    public Building(string name, string address, int floorCount, int? codigoExterno = null)
    {
        Id = Guid.NewGuid();
        SetName(name);
        SetAddress(address);
        SetFloorCount(floorCount);
        CodigoExterno = codigoExterno;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string address, int floorCount)
    {
        SetName(name);
        SetAddress(address);
        SetFloorCount(floorCount);
        UpdatedAt = DateTime.UtcNow;
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

    public void AddClassroom(Classroom classroom)
    {
        if (_classrooms.Any(c => c.Name == classroom.Name))
            throw new InvalidOperationException($"A classroom with the name '{classroom.Name}' already exists in this building.");

        _classrooms.Add(classroom);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCodigoExterno(int codigoExterno)
    {
        CodigoExterno = codigoExterno;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetFloorPlanUrl(string? floorPlanUrl)
    {
        FloorPlanUrl = floorPlanUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Building name cannot be empty.", nameof(name));
        Name = name;
    }

    private void SetAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Building address cannot be empty.", nameof(address));
        Address = address;
    }

    private void SetFloorCount(int floorCount)
    {
        if (floorCount <= 0)
            throw new ArgumentException("Floor count must be greater than zero.", nameof(floorCount));
        FloorCount = floorCount;
    }
}

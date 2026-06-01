using FluentAssertions;
using Lienzo.Domain.Entities;
using Xunit;

namespace Lienzo.Domain.Tests;

public class BuildingTests
{
    [Fact]
    public void Create_ValidParameters_SetsProperties()
    {
        var building = new Building("Main Building", "123 Main St", 5);

        building.Name.Should().Be("Main Building");
        building.Address.Should().Be("123 Main St");
        building.FloorCount.Should().Be(5);
        building.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ActivateDeactivate_TogglesIsActive()
    {
        var building = new Building("Main", "Addr", 3);

        building.Deactivate();
        building.IsActive.Should().BeFalse();

        building.Activate();
        building.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateDetails_ModifiesProperties()
    {
        var building = new Building("Main Building", "123 Main St", 5);

        building.UpdateDetails("Annex Building", "456 Oak Ave", 3);

        building.Name.Should().Be("Annex Building");
        building.Address.Should().Be("456 Oak Ave");
        building.FloorCount.Should().Be(3);
    }
}

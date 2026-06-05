namespace Lienzo.Domain.Entities;

public class KeyDeliveryAccessory
{
    public Guid KeyDeliveryId { get; private set; }
    public KeyDelivery KeyDelivery { get; private set; } = null!;
    public Guid AccessoryId { get; private set; }
    public Accessory Accessory { get; private set; } = null!;

    private KeyDeliveryAccessory() { }

    public KeyDeliveryAccessory(Guid keyDeliveryId, Guid accessoryId)
    {
        KeyDeliveryId = keyDeliveryId;
        AccessoryId = accessoryId;
    }
}

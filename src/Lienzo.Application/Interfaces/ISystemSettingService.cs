namespace Lienzo.Application.Interfaces;

public interface ISystemSettingService
{
    Task<string?> GetValueAsync(string key);
    Task SetValueAsync(string key, string value);
}

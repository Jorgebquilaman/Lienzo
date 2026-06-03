using Lienzo.Application.Interfaces;
using Lienzo.Domain.Entities;
using Lienzo.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lienzo.Infrastructure.Services;

public class SystemSettingService : ISystemSettingService
{
    private readonly IUnitOfWork _unitOfWork;

    public SystemSettingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var setting = await _unitOfWork.SystemSettings.Query()
            .FirstOrDefaultAsync(s => s.Key == key && !s.IsDeleted);
        return setting?.Value;
    }

    public async Task SetValueAsync(string key, string value)
    {
        var setting = await _unitOfWork.SystemSettings.Query()
            .FirstOrDefaultAsync(s => s.Key == key && !s.IsDeleted);

        if (setting is null)
        {
            setting = new SystemSetting(key, value);
            await _unitOfWork.SystemSettings.AddAsync(setting);
        }
        else
        {
            setting.UpdateValue(value);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}

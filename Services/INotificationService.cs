using System.Threading.Tasks;
using MsBox.Avalonia;

namespace Deer_o_matic.Services;


public interface INotificationService
{
    public Task ShowSuccessAsync(string message);
    public Task ShowErrorAsync(string message);
}

public class NotificationService : INotificationService
{
    public async Task ShowErrorAsync(string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Error", message, MsBox.Avalonia.Enums.ButtonEnum.Ok);
        
        await box.ShowAsync();
    }

    public async Task ShowSuccessAsync(string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Success", message, MsBox.Avalonia.Enums.ButtonEnum.Ok);
        
        await box.ShowAsync();
    }
}
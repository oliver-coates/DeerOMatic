using Avalonia;
using System;

// To read:
// Avalonia UI - https://docs.avaloniaui.net/docs/get-started/starter-tutorial/exercises
// MVVM Architecture - https://learn.microsoft.com/en-us/dotnet/architecture/maui/mvvm
// Processing XML - https://learn.microsoft.com/en-us/dotnet/standard/data/xml/xml-processing-options
// PDF procesing - https://github.com/QuestPDF/QuestPDF
// Deploying to executable - https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview?tabs=cli


namespace Deer_o_matic;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}

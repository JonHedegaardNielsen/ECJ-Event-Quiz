using System.Diagnostics;
using System.Windows;

namespace ECJ.EventQuiz.ServerUI;

/// <summary>
/// WPF application entry. Shows <see cref="SplashWindow"/> for at least 4 seconds,
/// then shows <see cref="MainWindow"/>.
/// </summary>
public partial class App : Application
{
    private static readonly TimeSpan MinSplashDuration = TimeSpan.FromSeconds(4);

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var splash = new SplashWindow();
        splash.Show();
        var started = Stopwatch.StartNew();

        MainWindow? mainWindow = null;
        try
        {
            splash.SetProgress("Starter quiz-server…", 8);
            await Task.Delay(400);

            splash.SetProgress("Indlæser indstillinger…", 35);
            await Task.Delay(400);
            mainWindow = new MainWindow();
            MainWindow = mainWindow;

            splash.SetProgress("Forbereder brugerflade…", 70);
            await Task.Delay(400);

            splash.SetProgress("Klar", 100);

            var remaining = MinSplashDuration - started.Elapsed;
            if (remaining > TimeSpan.Zero)
                await Task.Delay(remaining);

            await splash.CloseAnimatedAsync();

            mainWindow.Show();
            mainWindow.Activate();
        }
        catch (Exception ex)
        {
            splash.ForceClose();
            MessageBox.Show($"Kunne ikke starte ServerUI:\n\n{ex.Message}", "ECJ EventQuiz", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }
}

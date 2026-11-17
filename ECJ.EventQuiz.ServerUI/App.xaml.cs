using System.Windows;

namespace ECJ.EventQuiz.ServerUI;

/// <summary>
/// WPF application entry. Shows <see cref="SplashWindow"/> while <see cref="MainWindow"/> is created,
/// then fades the splash out. WebView2 initialization stays on MainWindow.Loaded.
/// </summary>
public partial class App : Application
{
    private static readonly TimeSpan StepDelay = TimeSpan.FromMilliseconds(350);

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var splash = new SplashWindow();
        splash.Show();

        try
        {
            splash.SetProgress("Starter quiz-server…", 8);
            await Task.Delay(StepDelay);

            splash.SetProgress("Indlæser indstillinger…", 35);
            await Task.Delay(StepDelay);
            var mainWindow = new MainWindow();
            MainWindow = mainWindow;

            splash.SetProgress("Forbereder brugerflade…", 70);
            await Task.Delay(StepDelay);
            mainWindow.Show();

            splash.SetProgress("Klar", 100);
            await Task.Delay(StepDelay);
            await splash.CloseAnimatedAsync();
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

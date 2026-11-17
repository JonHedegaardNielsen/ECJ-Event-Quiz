using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Animation;

namespace ECJ.EventQuiz.ServerUI;

/// <summary>
/// Startup splash shown while <see cref="MainWindow"/> is created.
/// Cannot be closed by the user (Alt+F4); the app closes it via <see cref="CloseAnimatedAsync"/>.
/// </summary>
public partial class SplashWindow : Window
{
    private const string ProductName = "ECJ EventQuiz";

    private bool _closeConfirmed;

    public SplashWindow()
    {
        InitializeComponent();
        VersionFooter.Text = BuildVersionFooter();
        Closing += OnClosing;
    }

    public void SetProgress(string status, int percent)
    {
        var clamped = Math.Clamp(percent, 0, 100);
        StatusText.Text = status;
        PercentText.Text = $"{clamped}%";
        ProgressFillColumn.Width = new GridLength(clamped, GridUnitType.Star);
        ProgressRestColumn.Width = new GridLength(100 - clamped, GridUnitType.Star);
    }

    public async Task CloseAnimatedAsync()
    {
        _closeConfirmed = true;
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(260))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        fade.Completed += (_, _) =>
        {
            Close();
            completed.TrySetResult();
        };
        BeginAnimation(OpacityProperty, fade);
        await completed.Task;
    }

    /// <summary>Closes immediately without animation, e.g. when startup fails.</summary>
    public void ForceClose()
    {
        _closeConfirmed = true;
        Close();
    }

    private static string BuildVersionFooter()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var text = version is null ? "1.0.0" : $"{version.Major}.{version.Minor}.{version.Build}";
        return $"Version {text}  •  © {DateTime.Now.Year} {ProductName}";
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!_closeConfirmed)
            e.Cancel = true;
    }
}

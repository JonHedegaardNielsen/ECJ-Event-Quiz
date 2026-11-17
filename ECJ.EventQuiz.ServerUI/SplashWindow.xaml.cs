using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ECJ.EventQuiz.ServerUI;

/// <summary>
/// Startup splash shown before <see cref="MainWindow"/>.
/// With <see cref="HoldUntilEscape"/> the splash stays until ESC (handy while tuning layout).
/// </summary>
public partial class SplashWindow : Window
{
    private const string ProductName = "ECJ EventQuiz";

    /// <summary>
    /// When true, splash waits for ESC after reaching 100% before fading out.
    /// Set to false when splash layout is finalized.
    /// </summary>
    public static bool HoldUntilEscape { get; set; } = false;

    private bool _closeConfirmed;
    private TaskCompletionSource? _escapeTcs;

    public SplashWindow()
    {
        InitializeComponent();
        Focusable = true;
        VersionFooter.Text = BuildVersionFooter();
        Closing += OnClosing;
        PreviewKeyDown += OnPreviewKeyDown;
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
        if (HoldUntilEscape)
            await WaitForEscapeAsync();

        _closeConfirmed = true;
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(260))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        fade.Completed += (_, _) =>
        {
            try { Close(); } catch { /* ignore */ }
            completed.TrySetResult();
        };
        BeginAnimation(OpacityProperty, fade);
        await completed.Task;
    }

    /// <summary>Closes immediately without animation, e.g. when startup fails.</summary>
    public void ForceClose()
    {
        _escapeTcs?.TrySetResult();
        _closeConfirmed = true;
        Close();
    }

    private Task WaitForEscapeAsync()
    {
        if (_escapeTcs?.Task.IsCompleted == true)
            return Task.CompletedTask;

        _escapeTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        SetProgress("Tryk ESC for at lukke splash…", 100);
        Topmost = true;
        Activate();
        Focus();
        return _escapeTcs.Task;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || _escapeTcs is null || _escapeTcs.Task.IsCompleted)
            return;

        e.Handled = true;
        _escapeTcs.TrySetResult();
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

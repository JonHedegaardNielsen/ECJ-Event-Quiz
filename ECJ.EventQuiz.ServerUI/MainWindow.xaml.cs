using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using ECJ.EventQuiz.ServerUI.Hosting;
using Microsoft.Web.WebView2.Core;

namespace ECJ.EventQuiz.ServerUI;

/// <summary>
/// Compact WPF shell for the quiz server.
/// Custom title bar (settings / minimize / close) + WebView2 UI.
/// HTML talks to C# with postMessage (server.start/stop, settings.*).
/// C# pushes status back with PostWebMessageAsJson.
/// </summary>
public partial class MainWindow : Window
{
    private const int TitleBarHeight = 48;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly QuizServerHost _server = new();
    private readonly AppSettings _settings;

    public MainWindow()
    {
        InitializeComponent();
        _settings = AppSettings.Load();
        _settings.Normalize();

        CaptionButtons.Children.Add(CreateCaptionButton("\uE921", false, OnMinimize));
        CaptionButtons.Children.Add(CreateCaptionButton("\uE8BB", true, OnCloseClick));

        WindowChrome.SetIsHitTestVisibleInChrome(SettingsButton, true);
        WebView.DefaultBackgroundColor = System.Drawing.Color.FromArgb(0x15, 0x20, 0x33);

        Loaded += OnLoaded;
        Closed += async (_, _) => await _server.DisposeAsync();
        _server.StatusChanged += () => Dispatcher.Invoke(() => PushStatus());
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await WebView.EnsureCoreWebView2Async();
        WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        WebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

        var hostUi = Path.Combine(AppContext.BaseDirectory, "wwwroot", "host-ui");
        WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "app.ecj",
            hostUi,
            CoreWebView2HostResourceAccessKind.Allow);

        WebView.CoreWebView2.Navigate("https://app.ecj/index.html");
        PushStatus();
    }

    private async void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            using var doc = JsonDocument.Parse(e.WebMessageAsJson);
            var root = doc.RootElement;
            var type = root.TryGetProperty("type", out var t) ? t.GetString() : null;

            switch (type)
            {
                case "server.start":
                    try
                    {
                        await _server.StartAsync(_settings.Port, _settings.BindAllInterfaces);
                    }
                    catch (Exception ex)
                    {
                        PushStatus(ex.Message);
                    }
                    break;

                case "server.stop":
                    await _server.StopAsync();
                    break;

                case "server.getStatus":
                    PushStatus();
                    break;

                case "settings.get":
                    PushSettings();
                    break;

                case "settings.save":
                    SaveSettingsFromMessage(root);
                    PushSettings();
                    PushStatus();
                    break;
            }
        }
        catch (Exception ex)
        {
            PushStatus(ex.Message);
        }
    }

    private void SaveSettingsFromMessage(JsonElement root)
    {
        if (_server.IsRunning)
        {
            PushStatus("Stop serveren før du ændrer indstillinger.");
            return;
        }

        if (root.TryGetProperty("port", out var portEl) && portEl.TryGetInt32(out var port))
            _settings.Port = port;

        if (root.TryGetProperty("bindAllInterfaces", out var bindEl))
            _settings.BindAllInterfaces = bindEl.ValueKind == JsonValueKind.True;

        _settings.Normalize();
        _settings.Save();
    }

    private void PushStatus(string? error = null)
    {
        if (WebView.CoreWebView2 is null)
            return;

        var bases = _server.IsRunning
            ? _server.GetClientBaseUrls()
            : NetworkAddresses.GetClientHosts(_settings.BindAllInterfaces)
                .Select(h => $"http://{h}:{_settings.Port}")
                .ToList();

        var payload = new
        {
            type = "server.status",
            running = _server.IsRunning,
            port = _server.IsRunning ? _server.Port : _settings.Port,
            bindUrl = _server.IsRunning
                ? _server.BindUrl
                : $"http://{(_settings.BindAllInterfaces ? "0.0.0.0" : "127.0.0.1")}:{_settings.Port}",
            bindAllInterfaces = _server.IsRunning ? _server.BindAllInterfaces : _settings.BindAllInterfaces,
            baseUrls = bases,
            hubUrls = bases.Select(b => $"{b}{QuizServerHost.HubPath}").ToList(),
            error
        };

        WebView.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private void PushSettings()
    {
        if (WebView.CoreWebView2 is null)
            return;

        var payload = new
        {
            type = "settings.state",
            port = _settings.Port,
            bindAllInterfaces = _settings.BindAllInterfaces,
            running = _server.IsRunning
        };

        WebView.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        if (WebView.CoreWebView2 is null)
            return;

        WebView.CoreWebView2.PostWebMessageAsJson(
            JsonSerializer.Serialize(new { type = "ui.openSettings" }, JsonOptions));
        PushSettings();
    }

    private void OnTitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void OnMinimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private Button CreateCaptionButton(string glyph, bool isClose, RoutedEventHandler onClick)
    {
        var button = new Button
        {
            Content = glyph,
            Width = 46,
            Height = TitleBarHeight,
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromRgb(0xE8, 0xEE, 0xF8)),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Focusable = false,
            Cursor = Cursors.Hand,
            Template = CreateCaptionButtonTemplate(isClose)
        };

        button.Click += onClick;
        WindowChrome.SetIsHitTestVisibleInChrome(button, true);
        return button;
    }

    private static ControlTemplate CreateCaptionButtonTemplate(bool isClose)
    {
        var template = new ControlTemplate(typeof(Button));
        var border = new FrameworkElementFactory(typeof(Border));
        border.Name = "Bd";
        border.SetValue(Border.BackgroundProperty, Brushes.Transparent);
        var content = new FrameworkElementFactory(typeof(ContentPresenter));
        content.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
        content.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
        border.AppendChild(content);
        template.VisualTree = border;

        var hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
        hover.Setters.Add(new Setter(
            Border.BackgroundProperty,
            isClose
                ? new SolidColorBrush(Color.FromRgb(0xC4, 0x2B, 0x1C))
                : new SolidColorBrush(Color.FromRgb(0x1F, 0x2F, 0x48)),
            "Bd"));
        template.Triggers.Add(hover);
        return template;
    }
}

using System.IO;
using System.Text.Json;

namespace ECJ.EventQuiz.ServerUI.Hosting;

/// <summary>
/// Saved server settings (port + bind mode).
/// Stored under %LocalAppData%\ECJ.EventQuiz\server-settings.json.
/// Call <see cref="Load"/> at startup and <see cref="Save"/> from the Settings modal.
/// </summary>
public sealed class AppSettings
{
    public int Port { get; set; } = 5188;

    /// <summary>
    /// true = listen on 0.0.0.0 (localhost, 127.0.0.1, and LAN IP work).
    /// false = loopback only.
    /// </summary>
    public bool BindAllInterfaces { get; set; } = true;

    private static string FilePath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ECJ.EventQuiz",
            "server-settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return new AppSettings();

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }

    public void Normalize()
    {
        if (Port is < 1 or > 65535)
            Port = 5188;
    }
}

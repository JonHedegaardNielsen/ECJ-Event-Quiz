using ECJ.EventQuiz.ServerUI.Hosting;

namespace ECJ.EventQuiz.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="ECJ.EventQuiz.ServerUI.Hosting.AppSettings"/>.
/// </summary>
public class AppSettingsTests
{
    [Fact]
    public void Normalize_resets_invalid_port()
    {
        var settings = new AppSettings { Port = 0 };
        settings.Normalize();
        Assert.Equal(5188, settings.Port);

        settings.Port = 70000;
        settings.Normalize();
        Assert.Equal(5188, settings.Port);
    }

    [Fact]
    public void Defaults_are_sensible()
    {
        var settings = new AppSettings();
        Assert.Equal(5188, settings.Port);
        Assert.True(settings.BindAllInterfaces);
    }
}

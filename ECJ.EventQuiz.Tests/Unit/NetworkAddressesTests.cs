using ECJ.EventQuiz.ServerUI.Hosting;

namespace ECJ.EventQuiz.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="ECJ.EventQuiz.ServerUI.Hosting.NetworkAddresses"/>.
/// </summary>
public class NetworkAddressesTests
{
    [Fact]
    public void Always_includes_localhost_and_loopback()
    {
        var hosts = NetworkAddresses.GetClientHosts(includeLan: false);
        Assert.Equal(["localhost", "127.0.0.1"], hosts);
    }

    [Fact]
    public void With_lan_still_starts_with_localhost_and_loopback()
    {
        var hosts = NetworkAddresses.GetClientHosts(includeLan: true);
        Assert.True(hosts.Count >= 2);
        Assert.Equal("localhost", hosts[0]);
        Assert.Equal("127.0.0.1", hosts[1]);
        Assert.DoesNotContain("0.0.0.0", hosts);
    }
}

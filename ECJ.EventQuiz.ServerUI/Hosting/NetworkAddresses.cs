using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ECJ.EventQuiz.ServerUI.Hosting;

/// <summary>
/// Builds client-friendly hostnames/IPs for the UI dropdown.
/// Always includes localhost and 127.0.0.1.
/// Optionally adds real LAN IPv4 addresses (filters Hyper-V/WSL/VPN adapters).
/// Note: 0.0.0.0 is a bind address only — never returned here.
/// </summary>
public static class NetworkAddresses
{
    public static IReadOnlyList<string> GetClientHosts(bool includeLan)
    {
        var hosts = new List<string> { "localhost", "127.0.0.1" };
        if (!includeLan)
            return hosts;

        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces().OrderBy(Priority))
        {
            if (!IsUsefulLanInterface(ni))
                continue;

            foreach (var uni in ni.GetIPProperties().UnicastAddresses)
            {
                if (uni.Address.AddressFamily != AddressFamily.InterNetwork)
                    continue;
                if (IPAddress.IsLoopback(uni.Address))
                    continue;

                var bytes = uni.Address.GetAddressBytes();
                if (bytes[0] == 169 && bytes[1] == 254)
                    continue;

                var text = uni.Address.ToString();
                if (!hosts.Contains(text, StringComparer.OrdinalIgnoreCase))
                    hosts.Add(text);
            }
        }

        return hosts;
    }

    private static int Priority(NetworkInterface ni) => ni.NetworkInterfaceType switch
    {
        NetworkInterfaceType.Wireless80211 => 0,
        NetworkInterfaceType.Ethernet => 1,
        _ => 5
    };

    private static bool IsUsefulLanInterface(NetworkInterface ni)
    {
        if (ni.OperationalStatus != OperationalStatus.Up)
            return false;
        if (ni.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
            return false;

        var name = $"{ni.Name} {ni.Description}";
        if (ContainsAny(name,
                "Hyper-V", "vEthernet", "WSL", "VirtualBox", "VMware", "VPN",
                "TAP-", "TUN", "Bluetooth", "Loopback", "Pseudo"))
            return false;

        return ni.NetworkInterfaceType is NetworkInterfaceType.Ethernet
            or NetworkInterfaceType.Wireless80211
            or NetworkInterfaceType.GigabitEthernet;
    }

    private static bool ContainsAny(string haystack, params string[] needles) =>
        needles.Any(n => haystack.Contains(n, StringComparison.OrdinalIgnoreCase));
}

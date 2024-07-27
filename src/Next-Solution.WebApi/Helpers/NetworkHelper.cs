using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Next_Solution.WebApi.Helpers
{
    public static class NetworkHelper
    {
        private static IPAddress? GetFirstAvailableIPAddress(
            Func<IPInterfaceProperties, IEnumerable<IPAddress>> selector,
            Func<IPAddress, bool> filter)
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(n => n.GetIPProperties())
                .Where(properties => properties != null)
                .SelectMany(properties => selector(properties))
                .Where(a => a != null && filter(a))
                .FirstOrDefault();
        }

        public static IPAddress? GetDefaultGateway()
        {
            return GetFirstAvailableIPAddress(
                properties => properties.GatewayAddresses.Select(g => g.Address),
                a => true
            );
        }

        public static IPAddress? GetDefaultIpAddress()
        {
            return GetFirstAvailableIPAddress(
                properties => properties.UnicastAddresses.Select(a => a.Address),
                a => a.AddressFamily == AddressFamily.InterNetwork
            );
        }
    }
}
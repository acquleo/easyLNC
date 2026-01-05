using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.ScreenStream.OMT
{
    internal class IpHelper
    {
        public static IPAddress GetIPv4OfDefaultGateway()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                var ipProps = ni.GetIPProperties();

                // Interface must have a default gateway
                if (!ipProps.GatewayAddresses.Any(g =>
                        g.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !g.Address.Equals(IPAddress.Any)))
                    continue;

                // Pick a valid IPv4 address
                var addr = ipProps.UnicastAddresses
                    .FirstOrDefault(a =>
                        a.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(a.Address) &&
                        !a.Address.ToString().StartsWith("169.254."));

                if (addr != null)
                    return addr.Address;
            }

            return null; // No suitable interface found
        }

        public static IPAddress? GetBestIntranetIPv6()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                    continue;

                var props = ni.GetIPProperties();

                foreach (var ua in props.UnicastAddresses)
                {
                    var ip = ua.Address;

                    if (ip.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    // Prefer ULA (fd00::/8)
                    if (ip.ToString().StartsWith("fd", StringComparison.OrdinalIgnoreCase))
                        return ip;

                    return ip;
                }
            }

            return null;
        }

    }
}

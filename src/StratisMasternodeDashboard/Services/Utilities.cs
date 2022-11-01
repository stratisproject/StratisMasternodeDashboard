using System.Net.Sockets;
using System;
using Stratis.FederatedSidechains.AdminDashboard.Settings;

namespace Stratis.FederatedSidechains.AdminDashboard.Services
{
    public static class Utilities
    {
        /// <summary>
        /// Perform connection check with the nodes
        /// </summary>
        /// <remarks>The ports can be changed in the future</remarks>
        /// <returns>True if the connection are succeed</returns>
        public static (bool, bool) PerformNodeCheck(DefaultEndpointsSettings defaultEndpointsSettings)
        {
            var mainNodeUp = PortCheck(new Uri(defaultEndpointsSettings.MainchainNodeEndpoint));
            var sidechainsNodeUp = PortCheck(new Uri(defaultEndpointsSettings.SidechainNodeEndpoint));
            return (mainNodeUp, sidechainsNodeUp);
        }

        /// <summary>
        /// Perform a TCP port scan
        /// </summary>
        /// <param name="port">Specify the port to scan</param>
        /// <returns>True if the port is opened</returns>
        private static bool PortCheck(Uri endpointToCheck)
        {
            using var tcpClient = new TcpClient();

            try
            {
                tcpClient.Connect(endpointToCheck.Host, endpointToCheck.Port);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

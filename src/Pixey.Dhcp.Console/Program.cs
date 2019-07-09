using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Pixey.Dhcp.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var logFactory = new NullLoggerFactory();

            var listener = new DhcpListener(logFactory.CreateLogger<DhcpListener>());
            var client = new DhcpClient(listener, logFactory.CreateLogger<DhcpClient>());

            //var result = client.RequestIpAddress("00-15-5D-00-50-31").Result;
            var parameters = new DhcpDiscoveryParameters();

            var result = client.DiscoverDhcpServers(parameters, CancellationToken.None).Result;

            System.Console.WriteLine("Success");

            foreach (var dhcpPacketView in result)
            {
                System.Console.WriteLine("IP: {0}", dhcpPacketView.YourIP);
            }

            System.Console.ReadKey();
        }
    }
}

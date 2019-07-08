using Pixey.Dhcp.HardwareAddressTypes;

namespace Pixey.Dhcp.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new DhcpClient();

            //var result = client.RequestIpAddress("00-15-5D-00-50-31").Result;
            var clientMac = new EthernetClientHardwareAddress("de:ad:c0:de:ca:fe");
            var result = client.DiscoverDhcpServers(clientMac).Result;

            System.Console.WriteLine("Success");
            //System.Console.WriteLine(result);
            System.Console.ReadKey();
        }
    }
}

namespace Pixey.Dhcp.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new DhcpClient();

            //var result = client.RequestIpAddress("00-15-5D-00-50-31").Result;
            var result = client.DiscoverDhcpServers("de:ad:c0:de:ca:fe").Result;

            System.Console.WriteLine("Success");
            //System.Console.WriteLine(result);
            System.Console.ReadKey();
        }
    }
}

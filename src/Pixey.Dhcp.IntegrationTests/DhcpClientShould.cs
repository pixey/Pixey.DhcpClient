using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pixey.Dhcp.HardwareAddressTypes;
using Xunit;
using Xunit.Abstractions;

namespace Pixey.Dhcp.IntegrationTests
{
    public class DhcpClientShould
    {
        private readonly ITestOutputHelper _testOutput;

        public DhcpClientShould(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        [Fact]
        public void Wrapper()
        {
            ReturnDhcpPackets().Wait();
        }

        public async Task ReturnDhcpPackets()
        {
            //using (var client = new DhcpClient())
            var logFactory = new NullLoggerFactory();

            using (var listener = new DhcpListener(logFactory.CreateLogger<DhcpListener>()))
            {
                var client = new DhcpClient(listener, logFactory.CreateLogger<DhcpClient>());

                _testOutput.WriteLine("Starting...");

                var parameters = new DhcpDiscoveryParameters();
                parameters.ClientHardwareAddress = new EthernetClientHardwareAddress("de:ad:c0:de:ca:fe");

                var results = await client.DiscoverDhcpServers(parameters, CancellationToken.None).ConfigureAwait(false);

                _testOutput.WriteLine("Count: " + results.Count);

                foreach (var dhcpPacketView in results)
                {
                    _testOutput.WriteLine("IP: " + dhcpPacketView.YourIP);
                }

                _testOutput.WriteLine("Finished");

                Assert.True(false);
            }
        }
    }

    //internal class Converter : TextWriter
    //{
    //    ITestOutputHelper _output;
    //    public Converter(ITestOutputHelper output)
    //    {
    //        _output = output;
    //    }
    //    public override Encoding Encoding
    //    {
    //        get { return Encoding.Default; }
    //    }
    //    public override void WriteLine(string message)
    //    {
    //        _output.WriteLine(message);
    //    }
    //    public override void WriteLine(string format, params object[] args)
    //    {
    //        _output.WriteLine(format, args);
    //    }
    //}
}
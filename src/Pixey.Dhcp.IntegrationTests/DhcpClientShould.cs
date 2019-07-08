using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
        public async Task ReturnDhcpPackets()
        {
            //using (var client = new DhcpClient())
            var client = new DhcpClient();
            {
                _testOutput.WriteLine("Starting");

                var macAddress = new EthernetClientHardwareAddress("de:ad:c0:de:ca:fe");
                var results = await client.DiscoverDhcpServers(macAddress).ConfigureAwait(false);

                _testOutput.WriteLine("Count: " + results.Count);
                _testOutput.WriteLine("IP1: " + results[0].YourIP);
                _testOutput.WriteLine("IP2: " + results[1].YourIP);

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
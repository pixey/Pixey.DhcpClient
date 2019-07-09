using System.IO;
using System.Net;
using System.Threading.Tasks;
using Pixey.Dhcp.Enums;

namespace Pixey.Dhcp.Options
{
    internal class DHCPOptionBroadcastAddress : DhcpOption
    {
        public IPAddress BroadcastAddress { get; set; } = IPAddress.Any;

        public DHCPOptionBroadcastAddress(IPAddress broadcastAddress)
        {
            BroadcastAddress = broadcastAddress;
        }

        public DHCPOptionBroadcastAddress(int optionLength, byte[] buffer, long offset)
        {
            BroadcastAddress = ReadIPAddress(buffer, offset);
        }

        public override string ToString()
        {
            return "Broadcast address : " + BroadcastAddress;
        }

        public override Task Serialize(Stream stream)
        {
            return SerializeIPAddress(stream, DhcpOptionType.BroadcastAddress, BroadcastAddress);
        }
    }
}

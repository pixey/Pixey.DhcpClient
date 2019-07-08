using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Pixey.Dhcp.Enums;
using Pixey.Dhcp.HardwareAddressTypes;
using Xunit;

namespace Pixey.Dhcp.UnitTests
{
    public class DhcpPacketViewShould
    {
        public static IEnumerable<object[]> ParseMessageTypeData = new List<object[]>
        {
            new object[]{ DhcpSamplePackets.Offer, DHCPMessageType.DHCPOFFER },
            new object[]{ DhcpSamplePackets.Discover, DHCPMessageType.DHCPDISCOVER }
        };

        [Theory]
        [MemberData(nameof(ParseMessageTypeData))]
        public void ParseMessageType(byte[] packetBytes, DHCPMessageType expectedMessageType)
        {
            var packetView = new DhcpPacketView(packetBytes);

            Assert.Equal(expectedMessageType, packetView.DHCPMessageType);
        }

        [Fact]
        public void ParseYourIp()
        {
            var packetView = new DhcpPacketView(DhcpSamplePackets.Offer);

            Assert.Equal(IPAddress.Parse("192.168.1.101"), packetView.YourIP);
        }

        [Fact]
        public void ParseNextServerIp()
        {
            var packetView = new DhcpPacketView(DhcpSamplePackets.Offer);

            Assert.Equal(IPAddress.Parse("192.168.1.13"), packetView.NextServerIP);
        }

        [Fact]
        public void ParseBootFile()
        {
            var packetView = new DhcpPacketView(DhcpSamplePackets.Offer);

            Assert.Equal("ipxe.efi", packetView.BootFile);
        }

        [Fact]
        public void ParseSubnet()
        {
            var packetView = new DhcpPacketView(DhcpSamplePackets.Offer);

            Assert.Equal(IPAddress.Parse("255.255.255.0"), packetView.SubnetMask);
        }

        [Fact]
        public void ParseDhcpServer()
        {
            var packetView = new DhcpPacketView(DhcpSamplePackets.Offer);

            Assert.Equal(IPAddress.Parse("192.168.1.2"), packetView.DHCPServerIdentifier);
        }

        [Fact]
        public void ParseDnsServers()
        {
            var packetView = new DhcpPacketView(DhcpSamplePackets.Offer);

            Assert.Contains(packetView.DomainNameServers, dns => dns.Equals(IPAddress.Parse("8.8.8.8")));
        }

        [Fact]
        public void ParseRouter()
        {
            var packetView = new DhcpPacketView(DhcpSamplePackets.Offer);

            Assert.Contains(packetView.Routers, dns => dns.Equals(IPAddress.Parse("192.168.1.1")));
        }

        [Fact]
        public void ParseTransactionId()
        {
            var packetView = new DhcpPacketView(DhcpSamplePackets.Offer);
            var expectedTransactionIdBytes = new byte[] { 0x00, 0x1b, 0x0c, 0x61 }.Reverse().ToArray();

            Assert.Equal(BitConverter.ToUInt32(expectedTransactionIdBytes), packetView.TransactionId);
        }

        [Fact]
        public void ParseYourIpLeaseTime()
        {
            var packetView = new DhcpPacketView(DhcpSamplePackets.Offer);

            Assert.Equal(TimeSpan.FromSeconds(600), packetView.IPAddressLeaseTime);
        }

        [Fact]
        public void ParseMacAddress()
        {
            var packetView = new DhcpPacketView(DhcpSamplePackets.Discover);

            var expectedAddress = new EthernetClientHardwareAddress(new byte[] { 0xde, 0xad, 0xc0, 0xde, 0xca, 0xfe });

            Assert.Equal(expectedAddress.AddressLength, packetView.ClientHardwareAddress.AddressLength);
            Assert.Equal(expectedAddress.AddressType, packetView.ClientHardwareAddress.AddressType);

            var actualEthernetAddress = Assert.IsType<EthernetClientHardwareAddress>(packetView.ClientHardwareAddress);
            Assert.Equal(expectedAddress.Address, actualEthernetAddress.Address);
        }

        //[Fact]
        //public void ParseUserClass()
        //{
        //    var packetView = new DHCPPacketView(DhcpSamplePackets.Offer);

        //    throw new NotImplementedException();
        //}

        [Fact]
        public async Task SerializeAndDeserializeWithEqualValues()
        {
            var expectedPacketView = new DhcpPacketView(DhcpSamplePackets.Offer);

            var expectedPacketBytes = await expectedPacketView.GetBytes();
            var actualPacketView = new DhcpPacketView(expectedPacketBytes);

            Assert.Equal(expectedPacketView.ClientHardwareAddress, actualPacketView.ClientHardwareAddress);
            Assert.Equal(expectedPacketView.DHCPServerIdentifier, actualPacketView.DHCPServerIdentifier);
            Assert.Equal(expectedPacketView.TransactionId, actualPacketView.TransactionId);
            Assert.Equal(expectedPacketView.UserClass, actualPacketView.UserClass);
            Assert.Equal(expectedPacketView.BootFile, actualPacketView.BootFile);
            Assert.Equal(expectedPacketView.NextServerIP, actualPacketView.NextServerIP);
            Assert.Equal(expectedPacketView.YourIP, actualPacketView.YourIP);
            Assert.Equal(expectedPacketView.SubnetMask, actualPacketView.SubnetMask);
            Assert.Equal(expectedPacketView.Routers, actualPacketView.Routers);
            Assert.Equal(expectedPacketView.DomainNameServers, actualPacketView.DomainNameServers);
        }
    }
}
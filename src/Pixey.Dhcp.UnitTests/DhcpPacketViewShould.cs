using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Pixey.Dhcp.Enums;
using Pixey.Dhcp.HardwareAddressTypes;
using Pixey.Dhcp.Options;
using Xunit;

namespace Pixey.Dhcp.UnitTests
{
    public class DhcpPacketViewShould
    {
        public static IEnumerable<object[]> ParseMessageTypeData = new List<object[]>
        {
            new object[]{ DhcpSamplePackets.Offer, DHCPMessageType.DHCPOFFER },
            new object[]{ DhcpSamplePackets.Discover, DHCPMessageType.DHCPDISCOVER },
            new object[]{ DhcpSamplePackets.Request, DHCPMessageType.DHCPREQUEST },
            new object[]{ DhcpSamplePackets.Ack, DHCPMessageType.DHCPACK }
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

        public static IEnumerable<object[]> ClientSystemTestData = new List<object[]>
        {
            new object[] { DhcpSamplePackets.DiscoverLegacyBiosX86, ClientSystem.Intelx86PC },
            new object[] { DhcpSamplePackets.DiscoverEfiX64, ClientSystem.EFI_BC }
        };

        [Theory]
        [MemberData(nameof(ClientSystemTestData))]
        public void ParseClientSystem(byte[] packetBytes, ClientSystem expectedClientSystem)
        {
            var packetView = new DhcpPacketView(packetBytes);

            Assert.Equal(expectedClientSystem, packetView.ClientSystem);
        }

        [Fact]
        public void ParseUserClass()
        {
            var packetView = new DhcpPacketView(DhcpSamplePackets.DiscoverIpxeUserClass);

            Assert.Equal("iPXE", packetView.UserClass);
        }

        [Fact]
        public async Task SerializeAndDeserializeWithEqualValues()
        {
            var expectedPacketView = new DhcpPacketView(new DhcpPacket());
            expectedPacketView.ClientSystem = ClientSystem.EFI_x86_64;
            expectedPacketView.ClientIP = IPAddress.Parse("192.168.1.1");
            expectedPacketView.YourIP = IPAddress.Parse("192.168.1.2");
            expectedPacketView.NextServerIP = IPAddress.Parse("192.168.1.3");
            expectedPacketView.RelayAgentIP = IPAddress.Parse("192.168.1.4");
            expectedPacketView.ClientHardwareAddress = new EthernetClientHardwareAddress("AA:BB:CC:DD:EE:FF");
            expectedPacketView.ServerHostname = "ServerHostname";
            expectedPacketView.BootFile = "BootFile";
            expectedPacketView.UserClass = "UserClass";
            expectedPacketView.DHCPMessageType = DHCPMessageType.DHCPDISCOVER;
            expectedPacketView.IPAddressLeaseTime = TimeSpan.FromSeconds(600);
            expectedPacketView.TransactionId = 123456789;
            expectedPacketView.AddressRequest = IPAddress.Parse("192.168.1.5");
            expectedPacketView.BroadcastAddress = IPAddress.Parse("192.168.1.6");
            expectedPacketView.BroadcastFlag = true;
            expectedPacketView.ClassId = new byte[] { 0x0c, 0xaa, 0x84 };
            expectedPacketView.ClasslessStaticRoutes = new List<DhcpOptionClasslessStaticRoute.RouteEntry>
            {
                new DhcpOptionClasslessStaticRoute.RouteEntry
                {
                    NextHop = IPAddress.Parse("192.168.1.7"),
                    Prefix = new DhcpOptionClasslessStaticRoute.NetworkPrefix
                    {
                        Length = 28,
                        Prefix = IPAddress.Parse("192.168.1.8")
                    }
                }
            };

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
            Assert.Equal(expectedPacketView.UserClass, actualPacketView.UserClass);
            Assert.Equal(expectedPacketView.ClientSystem, actualPacketView.ClientSystem);
            Assert.Equal(expectedPacketView.DHCPMessageType, actualPacketView.DHCPMessageType);
            Assert.Equal(expectedPacketView.IPAddressLeaseTime, actualPacketView.IPAddressLeaseTime);

            Assert.Equal(expectedPacketView.ClasslessStaticRoutes?.Count, actualPacketView.ClasslessStaticRoutes?.Count);

            for (int i = 0; i < expectedPacketView.ClasslessStaticRoutes.Count; i++)
            {
                Assert.Equal(expectedPacketView.ClasslessStaticRoutes[i], actualPacketView.ClasslessStaticRoutes[i]);
            }

            Assert.Equal(expectedPacketView.ClassId, actualPacketView.ClassId);
            Assert.Equal(expectedPacketView.BroadcastFlag, actualPacketView.BroadcastFlag);
            Assert.Equal(expectedPacketView.BroadcastAddress, actualPacketView.BroadcastAddress);
            Assert.Equal(expectedPacketView.AddressRequest, actualPacketView.AddressRequest);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Pixey.Dhcp.Enums;
using Pixey.Dhcp.HardwareAddressTypes;
using Pixey.Dhcp.Options;

namespace Pixey.Dhcp
{
    public class DhcpPacketView : IDhcpPacketView
    {
        public const int DhcpBroadcastFlag = 0x8000;

        public DhcpPacket Packet { get; set; }

        public DhcpPacketView(DHCPMessageType messageType)
        {
            Packet = new DhcpPacket();
            DHCPMessageType = messageType;
            Hops = 0;
            TimeElapsed = TimeSpan.Zero;
            BroadcastFlag = false;
            Packet.sname = string.Empty;
            Packet.file = string.Empty;
            Packet.magicNumber = DhcpPacket.DhcpMagicNumber;
            ClientIP = IPAddress.Any;
            YourIP = IPAddress.Any;
            NextServerIP = IPAddress.Any;
            RelayAgentIP = IPAddress.Any;
        }

        public DhcpPacketView(byte[] buffer)
        {
            Packet = DhcpPacketParser.Parse(buffer);
        }

        public DhcpPacketView(DhcpPacket packet)
        {
            Packet = packet;
        }

        public async Task Serialize(Stream stream)
        {
            var buffer = new byte[240];
            var index = 0;
            buffer[index++] = Convert.ToByte(Packet.op);
            buffer[index++] = Convert.ToByte(Packet.htype);
            buffer[index++] = Convert.ToByte(Packet.hlen);
            buffer[index++] = Convert.ToByte(Packet.hops);
            WriteUInt32(Packet.xid, ref buffer, ref index);
            WriteUInt16(Convert.ToUInt16(Packet.secs), ref buffer, ref index);
            WriteUInt16(Convert.ToUInt16(Packet.flags), ref buffer, ref index);
            WritePadded(Packet.ciaddr.GetAddressBytes(), 4, ref buffer, ref index);
            WritePadded(Packet.yiaddr.GetAddressBytes(), 4, ref buffer, ref index);
            WritePadded(Packet.siaddr.GetAddressBytes(), 4, ref buffer, ref index);
            WritePadded(Packet.giaddr.GetAddressBytes(), 4, ref buffer, ref index);
            WritePadded(Packet.chaddr.GetBytes(), 16, ref buffer, ref index);
            WritePaddedASCII(Packet.sname, 64, ref buffer, ref index);
            WritePaddedASCII(Packet.file, 128, ref buffer, ref index);
            WriteUInt32(DhcpPacket.DhcpMagicNumber, ref buffer, ref index);
            if (index != 240)
                throw new Exception("Packet format is just plain wrong!!!");

            await stream.WriteAsync(buffer, 0, buffer.Length);
            foreach (var option in Packet.Options)
                await option.Serialize(stream);

            var EndOption = new byte [] { 255 };
            await stream.WriteAsync(EndOption, 0, EndOption.Length);
        }

        public async Task<byte[]> GetBytes()
        {
            byte[] result;

            using (var stream = new MemoryStream())
            {
                await Serialize(stream);
                await stream.FlushAsync();
                stream.Position = 0;

                result = new byte[stream.Length];

                await stream.ReadAsync(result, 0, (Convert.ToInt32(stream.Length)));
            }

            return result;
        }

        static private void WritePadded(byte [] value, int fullLength, ref byte[] buffer, ref int offset)
        {
            Array.Copy(value, 0, buffer, offset, value.Length);
            offset += value.Length;

            var padding = Enumerable.Repeat((byte)0, fullLength - value.Length).ToArray();
            Array.Copy(padding, 0, buffer, offset, padding.Length);
            offset += padding.Length;
        }

        static private void WritePaddedASCII(string value, int fullLength, ref byte[] buffer, ref int offset)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            WritePadded(bytes, fullLength, ref buffer, ref offset);
        }

        static private void WriteUInt32(UInt32 value, ref byte[] buffer, ref int offset)
        {
            buffer[offset++] = Convert.ToByte(value >> 24);
            buffer[offset++] = Convert.ToByte((value >> 16) & 0xFF);
            buffer[offset++] = Convert.ToByte((value >> 8) & 0xFF);
            buffer[offset++] = Convert.ToByte(value & 0xFF);
        }

        static private void WriteUInt16(UInt32 value, ref byte[] buffer, ref int offset)
        {
            buffer[offset++] = Convert.ToByte((value >> 8) & 0xFF);
            buffer[offset++] = Convert.ToByte(value & 0xFF);
        }

        public IPAddress AddressRequest
        {
            get
            {
                var record = (DHCPOptionRequestedIPAddress)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionRequestedIPAddress)).FirstOrDefault();
                return (record == null) ? IPAddress.Any : record.RequestedIP;
            }
            set
            {
                var record = (DHCPOptionRequestedIPAddress)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionRequestedIPAddress)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionRequestedIPAddress(value));
                else
                    record.RequestedIP = value;
            }
        }

        public string BootFile
        {
            get { return Packet.file; }
            set { Packet.file = value; }
        }

        public IPAddress BroadcastAddress
        {
            get
            {
                var record = (DHCPOptionBroadcastAddress)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionBroadcastAddress)).FirstOrDefault();
                return (record == null) ? IPAddress.Any : record.BroadcastAddress;
            }
            set
            {
                var record = (DHCPOptionBroadcastAddress)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionBroadcastAddress)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionBroadcastAddress(value));
                else
                    record.BroadcastAddress = value;
            }
        }

        public bool BroadcastFlag
        {
            get { return (Packet.flags & DhcpBroadcastFlag) == DhcpBroadcastFlag; }
            set {
                if (value)
                    Packet.flags = Packet.flags | DhcpBroadcastFlag;
                else
                    Packet.flags = ~(~Packet.flags | DhcpBroadcastFlag);
            }
        }

        public byte[] ClassId
        {
            get
            {
                var record = (DHCPOptionClassId)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionClassId)).FirstOrDefault();
                return (record == null) ? null : record.ClassId;
            }
            set
            {
                var record = (DHCPOptionClassId)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionClassId)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionClassId(value));
                else
                    record.ClassId = value;
            }
        }

        public List<DHCPOptionClasslessStaticRoute.RouteEntry> ClasslessStaticRoutes
        {
            get
            {
                var record = (DHCPOptionClasslessStaticRoute)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionClasslessStaticRoute)).FirstOrDefault();
                return (record == null) ? null : record.Entries;
            }
            set
            {
                var record = (DHCPOptionClasslessStaticRoute)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionClasslessStaticRoute)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionClasslessStaticRoute(value));
                else
                    record.Entries = value;
            }
        }

        public void AddClasslessStaticRoute(IPAddress prefix, int prefixLength, IPAddress nextHop)
        {
            var newEntry = new DHCPOptionClasslessStaticRoute.RouteEntry
            {
                Prefix = new DHCPOptionClasslessStaticRoute.NetworkPrefix
                {
                    Prefix = prefix,
                    Length = prefixLength
                },
                NextHop = nextHop
            };

            var record = (DHCPOptionClasslessStaticRoute)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionClasslessStaticRoute)).FirstOrDefault();
            if (record == null)
                Packet.Options.Add(new DHCPOptionClasslessStaticRoute(new List<DHCPOptionClasslessStaticRoute.RouteEntry> { newEntry }));
            else
                record.Entries.Add(newEntry);
        }

        public ClientHardwareAddress ClientHardwareAddress
        {
            get { return Packet.chaddr; }
            set
            {
                Packet.chaddr = value;
                Packet.htype = value.AddressType;
                Packet.hlen = value.AddressLength;
            }
        }

        public ClientHardwareAddress ClientId
        {
            get
            {
                var record = (DHCPOptionClientId)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionClientId)).FirstOrDefault();
                return (record == null) ? null : record.ClientId;
            }
            set
            {
                var record = (DHCPOptionClientId)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionClientId)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionClientId(value));
                else
                    record.ClientId = value;

                Packet.htype = value.AddressType;
                Packet.hlen = value.AddressLength;
            }
        }

        public IPAddress ClientIP
        {
            get { return Packet.ciaddr; }
            set { Packet.ciaddr = value; }
        }

        public string DHCPMessage
        {
            get
            {
                var record = (DHCPOptionDHCPMessage)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionDHCPMessage)).FirstOrDefault();
                return (record == null) ? string.Empty : record.Message;
            }
            set
            {
                var record = (DHCPOptionDHCPMessage)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionDHCPMessage)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionDHCPMessage(value));
                else
                    record.Message = value;
            }
        }

        public DHCPMessageType DHCPMessageType
        {
            get
            {
                var record = (DHCPOptionDHCPMessageType)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionDHCPMessageType)).FirstOrDefault();
                return (record == null) ? DHCPMessageType.Unspecified : record.MessageType;
            }
            set
            {
                var record = (DHCPOptionDHCPMessageType)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionDHCPMessageType)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionDHCPMessageType(value));
                else
                    record.MessageType = value;

                switch (value)
                {
                    case DHCPMessageType.DHCPOFFER:
                    case DHCPMessageType.DHCPNAK:
                    case DHCPMessageType.DHCPACK:
                        Packet.op = MessageOpCode.BOOTREPLY;
                        break;
                    default:
                        Packet.op = MessageOpCode.BOOTREQUEST;
                        break;
                }
            }
        }

        public IPAddress DHCPServerIdentifier
        {
            get
            {
                var record = (DHCPOptionDHCPServerIdentifier)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionDHCPServerIdentifier)).FirstOrDefault();
                return (record == null) ? IPAddress.Any : record.ServerIdentifier;
            }
            set
            {
                var record = (DHCPOptionDHCPServerIdentifier)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionDHCPServerIdentifier)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionDHCPServerIdentifier(value));
                else
                    record.ServerIdentifier = value;
            }
        }

        public string DomainName
        {
            get
            {
                var record = (DHCPOptionDomainName)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionDomainName)).FirstOrDefault();
                return (record == null) ? string.Empty : record.DomainName;
            }
            set
            {
                var record = (DHCPOptionDomainName)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionDomainName)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionDomainName(value));
                else
                    record.DomainName = value;
            }
        }

        public List<IPAddress> DomainNameServers
        {
            get
            {
                var record = (DHCPOptionDomainNameServer)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionDomainNameServer)).FirstOrDefault();
                return (record == null) ? new List<IPAddress>() : record.DomainNameServers;
            }
            set
            {
                var record = (DHCPOptionDomainNameServer)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionDomainNameServer)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionDomainNameServer(value));
                else
                    record.DomainNameServers = value;
            }
        }

        public int Hops
        {
            get { return Packet.hops; }
            set { Packet.hops = value; }
        }

        public string Hostname
        {
            get
            {
                var record = (DHCPOptionHostname)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionHostname)).FirstOrDefault();
                return (record == null) ? string.Empty : record.Hostname;
            }
            set
            {
                var record = (DHCPOptionHostname)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionHostname)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionHostname(value));
                else
                    record.Hostname = value;
            }
        }

        public TimeSpan IPAddressLeaseTime
        {
            get
            {
                var record = (DHCPOptionIPAddressLeaseTime)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionIPAddressLeaseTime)).FirstOrDefault();
                return (record == null) ? TimeSpan.Zero : record.LeaseTime;
            }
            set
            {
                var record = (DHCPOptionIPAddressLeaseTime)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionIPAddressLeaseTime)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionIPAddressLeaseTime(value));
                else
                    record.LeaseTime = value;
            }
        }

        public int MaximumMessageSize
        {
            get
            {
                var record = (DHCPOptionMaximumMessageSize)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionMaximumMessageSize)).FirstOrDefault();
                return (record == null) ? -1 : record.MaximumMessageSize;
            }
            set
            {
                var record = (DHCPOptionMaximumMessageSize)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionMaximumMessageSize)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionMaximumMessageSize(value));
                else
                    record.MaximumMessageSize = value;
            }
        }

        public List<IPAddress> WINSServer
        {
            get
            {
                var record = (DHCPOptionNetBIOSOverTCPIPNameServer)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionNetBIOSOverTCPIPNameServer)).FirstOrDefault();
                return (record == null) ? new List<IPAddress>() : record.NameServers;
            }
            set
            {
                var record = (DHCPOptionNetBIOSOverTCPIPNameServer)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionNetBIOSOverTCPIPNameServer)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionNetBIOSOverTCPIPNameServer(value));
                else
                    record.NameServers = value;
            }
        }

        public IPAddress NextServerIP
        {
            get { return Packet.siaddr; }
            set { Packet.siaddr = value; }
        }

        public List<IPAddress> NTPServers
        {
            get
            {
                var record = (DHCPOptionNTPServer)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionNTPServer)).FirstOrDefault();
                return (record == null) ? new List<IPAddress>() : record.NTPServers;
            }
            set
            {
                var record = (DHCPOptionNTPServer)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionNTPServer)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionNTPServer(value));
                else
                    record.NTPServers = value;
            }
        }

        public List<DHCPOptionType> ParameterList
        {
            get
            {
                var record = (DHCPOptionParameterList)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionParameterList)).FirstOrDefault();
                return (record == null) ? new List<DHCPOptionType>() : record.ParameterList;
            }
            set
            {
                var record = (DHCPOptionParameterList)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionParameterList)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionParameterList(value));
                else
                    record.ParameterList = value;
            }
        }

        public IPAddress RelayAgentIP
        {
            get { return Packet.giaddr; }
            set { Packet.giaddr = value; }
        }

        public TimeSpan RebindingTimeValue
        {
            get
            {
                var record = (DHCPOptionRebindingTimeValue)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionRebindingTimeValue)).FirstOrDefault();
                return (record == null) ? TimeSpan.Zero : record.RebindingTime;
            }
            set
            {
                var record = (DHCPOptionRebindingTimeValue)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionRebindingTimeValue)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionRebindingTimeValue(value));
                else
                    record.RebindingTime = value;
            }
        }

        public byte[] RelayAgentCircuitId
        {
            get
            {
                var record = (DHCPOptionRelayAgentInformation)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionRelayAgentInformation)).FirstOrDefault();
                return (record == null) ? null : record.AgentCircuitId;
            }
            set
            {
                var record = (DHCPOptionRelayAgentInformation)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionRelayAgentInformation)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionRelayAgentInformation(value, null));
                else
                    record.AgentCircuitId = value;
            }
        }

        public void SetRelayAgentCircuitId(string circuitId)
        {
            var record = (DHCPOptionRelayAgentInformation)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionRelayAgentInformation)).FirstOrDefault();
            if (record == null)
                Packet.Options.Add(new DHCPOptionRelayAgentInformation(circuitId, string.Empty));
            else
                record.SetAgentCircuitId(circuitId);
        }

        public byte[] RelayAgentRemoteId
        {
            get
            {
                var record = (DHCPOptionRelayAgentInformation)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionRelayAgentInformation)).FirstOrDefault();
                return (record == null) ? null : record.AgentRemoteId;
            }
            set
            {
                var record = (DHCPOptionRelayAgentInformation)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionRelayAgentInformation)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionRelayAgentInformation(null, value));
                else
                    record.AgentRemoteId = value;
            }
        }

        public void SetRelayAgentRemoteId(string remoteId)
        {
            var record = (DHCPOptionRelayAgentInformation)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionRelayAgentInformation)).FirstOrDefault();
            if (record == null)
                Packet.Options.Add(new DHCPOptionRelayAgentInformation(string.Empty, remoteId));
            else
                record.SetAgentRemoteId(remoteId);
        }

        public TimeSpan RenewalTimeValue
        {
            get
            {
                var record = (DHCPOptionRenewalTimeValue)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionRenewalTimeValue)).FirstOrDefault();
                return (record == null) ? TimeSpan.Zero : record.RenewalTime;
            }
            set
            {
                var record = (DHCPOptionRenewalTimeValue)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionRenewalTimeValue)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionRenewalTimeValue(value));
                else
                    record.RenewalTime = value;
            }
        }

        public string RootPath
        {
            get
            {
                var record = (DHCPOptionRootPath)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionRootPath)).FirstOrDefault();
                return (record == null) ? string.Empty : record.RootPath;
            }
            set
            {
                var record = (DHCPOptionRootPath)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionRootPath)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionRootPath(value));
                else
                    record.RootPath = value;
            }
        }

        public List<IPAddress> Routers
        {
            get
            {
                var record = (DHCPOptionRouter)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionRouter)).FirstOrDefault();
                return (record == null) ? new List<IPAddress>() : record.Routers;
            }
            set
            {
                var record = (DHCPOptionRouter)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionRouter)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionRouter(value));
                else
                    record.Routers = value;
            }
        }

        public string ServerHostname
        {
            get { return Packet.sname; }
            set { Packet.sname = value; }
        }

        public IPAddress SubnetMask
        {
            get
            {
                var record = (DHCPOptionSubnetMask)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionSubnetMask)).FirstOrDefault();
                return (record == null) ? IPAddress.Any : record.SubnetMask;
            }
            set
            {
                var record = (DHCPOptionSubnetMask)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionSubnetMask)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionSubnetMask(value));
                else
                    record.SubnetMask = value;
            }
        }

        public string TFTPBootfile
        {
            get
            {
                var record = (DHCPOptionTFTPBootfile)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionTFTPBootfile)).FirstOrDefault();
                return (record == null) ? string.Empty : record.TFTPBootfile;
            }
            set
            {
                var record = (DHCPOptionTFTPBootfile)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionTFTPBootfile)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionTFTPBootfile(value));
                else
                    record.TFTPBootfile = value;
            }
        }

        public string TFTPServer
        {
            get
            {
                var record = (DHCPOptionTFTPServerName)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionTFTPServerName)).FirstOrDefault();
                return (record == null) ? string.Empty : record.TFTPServerName;
            }
            set
            {
                var record = (DHCPOptionTFTPServerName)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionTFTPServerName)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionTFTPServerName(value));
                else
                    record.TFTPServerName = value;
            }
        }

        public string UserClass
        {
            get
            {
                var record = Packet.Options.OfType<DHCPOptionUserClass>().FirstOrDefault();
                return (record == null) ? string.Empty : record.UserClass;
            }
            set
            {
                var record = Packet.Options.OfType<DHCPOptionUserClass>().FirstOrDefault();

                if (record == null)
                    Packet.Options.Add(new DHCPOptionUserClass(value));
                else
                    record.UserClass = value;
            }
        }

        public ClientSystem ClientSystem
        {
            get
            {
                var record = Packet.Options.OfType<DHCPOptionClientSystem>().FirstOrDefault();
                return (record == null) ? 0 : record.ClientSystem;
            }
            set
            {
                var record = Packet.Options.OfType<DHCPOptionClientSystem>().FirstOrDefault();

                if (record == null)
                    Packet.Options.Add(new DHCPOptionClientSystem(value));
                else
                    record.ClientSystem = value;
            }
        }

        // TODO : Come up with a better name
        public List<IPAddress> TFTPServer150
        {
            get
            {
                var record = (DHCPOptionTFTPServer)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionTFTPServer)).FirstOrDefault();
                return (record == null) ? new List<IPAddress>() : record.TFTPServers;
            }
            set
            {
                var record = (DHCPOptionTFTPServer)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionTFTPServer)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionTFTPServer(value));
                else
                    record.TFTPServers = value;
            }
        }

        public TimeSpan TimeElapsed
        {
            get { return TimeSpan.FromSeconds(Packet.secs); }
            set { Packet.secs = Convert.ToInt32(value.TotalSeconds); }
        }

        public TimeSpan TimeOffset
        {
            get
            {
                var record = (DHCPOptionTimeOffset)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionTimeOffset)).FirstOrDefault();
                return (record == null) ? TimeSpan.Zero : record.TimeOffset;
            }
            set
            {
                var record = (DHCPOptionTimeOffset)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionTimeOffset)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionTimeOffset(value));
                else
                    record.TimeOffset = value;
            }
        }

        public List<IPAddress> TimeServers
        {
            get
            {
                var record = (DHCPOptionTimeServer)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionTimeServer)).FirstOrDefault();
                return (record == null) ? new List<IPAddress>() : record.TimeServers;
            }
            set
            {
                var record = (DHCPOptionTimeServer)Packet.Options.Where(x => x.GetType() == typeof(DHCPOptionTimeServer)).FirstOrDefault();
                if (record == null)
                    Packet.Options.Add(new DHCPOptionTimeServer(value));
                else
                    record.TimeServers = value;
            }
        }

        public UInt32 TransactionId
        {
            get { return Packet.xid; }
            set { Packet.xid = value; }
        }

        public IPAddress YourIP
        {
            get { return Packet.yiaddr; }
            set { Packet.yiaddr = value; }
        }
    }
}

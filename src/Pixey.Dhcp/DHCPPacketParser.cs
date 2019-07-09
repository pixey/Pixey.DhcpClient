using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Pixey.Dhcp.Enums;
using Pixey.Dhcp.HardwareAddressTypes;
using Pixey.Dhcp.Options;

namespace Pixey.Dhcp
{
    internal class DhcpPacketParser
    {
        /// <summary>
        /// DHCP packet parser based on RFC2131 (https://tools.ietf.org/html/rfc2131)
        /// </summary>
        /// <param name="buffer">The input buffer to parse.</param>
        /// <returns>A parsed packet structure or null</returns>
        public static DhcpPacket Parse(byte[] buffer)
        {
            if (buffer.Length < 240)
                throw new DhcpParseException("The minimum DHCP packet size if 236 bytes");

            // Parse BOOTP Options
            var result = new DhcpPacket
            {
                op = (MessageOpCode)buffer[0],
                htype = (HardwareAddressType)buffer[1],
                hlen = Convert.ToInt32(buffer[2]),
                hops = Convert.ToInt32(buffer[3]),
                xid = Read32UnsignedBE(buffer, 4),
                secs = Convert.ToInt32(Read16UnsignedBE(buffer, 8)),
                flags = Convert.ToInt32(Read16UnsignedBE(buffer, 10)),
                ciaddr = ReadIPAddress(buffer, 12),
                yiaddr = ReadIPAddress(buffer, 16),
                siaddr = ReadIPAddress(buffer, 20),
                giaddr = ReadIPAddress(buffer, 24),
                chaddr = ReadClientHardwareAddress((HardwareAddressType)buffer[1], buffer, 28),
                sname = Encoding.ASCII.GetString(buffer, 44, 64).Trim(),
                file = Encoding.ASCII.GetString(buffer, 108, 128).Trim().TrimEnd('\0'),
                magicNumber = Read32UnsignedBE(buffer, 236)
            };

            if (result.magicNumber != DhcpPacket.DhcpMagicNumber)
            {
                throw new DhcpParseException("Received packet may be a BOOTP packet but is not a valid DHCP packet as it is missing the magic number at offset 236.");
            }

            var index = 240;
            while (index < buffer.Length)
            {
                int optionCode = Convert.ToInt32(buffer[index++]);
                if ((DhcpOptionType)optionCode == DhcpOptionType.End)
                    break;

                int length = Convert.ToInt32(buffer[index++]);

                // Parse the new option and add it to the list
                var dhcpOption = ParseDHCPOption(optionCode, length, buffer, index);
                result.Options.Add(dhcpOption);
                index += length;

                // For option 43 (Vendor Specific Information) and 60 (Vendor Class Id),
                // there is a "chicken and egg" possibility. As such, if 43 is seen before
                // 60 is seen, then the suboptions will be parsed before knowing the vendor class.
                // Then when option 60 is seen, option 43 will be reprocessed to find the right suboptions.
                if (dhcpOption is DHCPOptionVendorSpecificInformation)
                    (dhcpOption as DHCPOptionVendorSpecificInformation).VendorClassId = GetVendorClassId(result.Options);
                else if (dhcpOption is DHCPOptionClassId)
                    SetVendorClassId(result.Options, (dhcpOption as DHCPOptionClassId).ClassId);
            }

            return result;
        }

        private static byte[] GetVendorClassId(List<DhcpOption>options)
        {
            var classIdOption = options.Where(x => x is DHCPOptionClassId).FirstOrDefault();

            if (classIdOption == null)
                return new byte[0];

            return (classIdOption as DHCPOptionClassId).ClassId;
        }

        private static void SetVendorClassId(List<DhcpOption>options, byte [] classId)
        {
            var vsiOption = options.Where(x => x is DHCPOptionVendorSpecificInformation).FirstOrDefault();

            if (vsiOption == null)
                return;

            (vsiOption as DHCPOptionVendorSpecificInformation).VendorClassId = classId;
        }

        static IPAddress ReadIPAddress(byte[] buffer, long offset)
        {
            var addressBuffer = new byte[4];
            Array.Copy(buffer, offset, addressBuffer, 0, 4);
            return new IPAddress(addressBuffer);
        }

        static UInt32 Read32UnsignedBE(byte[] buffer, long offset)
        {
            return
                (Convert.ToUInt32(buffer[offset + 0]) << 24) |
                (Convert.ToUInt32(buffer[offset + 1]) << 16) |
                (Convert.ToUInt32(buffer[offset + 2]) << 8) |
                (Convert.ToUInt32(buffer[offset + 3]));
        }
        static UInt32 Read16UnsignedBE(byte[] buffer, long offset)
        {
            return
                (Convert.ToUInt32(buffer[offset + 0]) << 8) |
                (Convert.ToUInt32(buffer[offset + 1]));
        }

        static ClientHardwareAddress ReadClientHardwareAddress(HardwareAddressType htype, byte[] buffer, long offset)
        {
            switch (htype)
            {
                case HardwareAddressType.Ethernet:
                    if ((offset + 6) < buffer.Length)
                    {
                        var addressArray = new byte[6];
                        Array.Copy(buffer, offset, addressArray, 0, 6);
                        return new EthernetClientHardwareAddress(addressArray);
                    }
                    throw new IndexOutOfRangeException("Provided buffer and starting index is too short to supply a 6 byte hardware address");

                default:
                    throw new Exception("Unhandled hardware address type " + htype.ToString());
            }
        }

        static DhcpOption ParseDHCPOption(int optionNumber, int optionLength, byte [] buffer, long offset)
        {
            switch((DhcpOptionType)optionNumber)
            {
                case DhcpOptionType.AddressRequest:
                    return new DHCPOptionRequestedIPAddress(optionLength, buffer, offset);
                case DhcpOptionType.AddressTime:
                    return new DHCPOptionIPAddressLeaseTime(optionLength, buffer, offset);
                case DhcpOptionType.BootfileName:
                    return new DHCPOptionTFTPBootfile(optionLength, buffer, offset);
                case DhcpOptionType.BroadcastAddress:
                    return new DHCPOptionBroadcastAddress(optionLength, buffer, offset);
                case DhcpOptionType.ClassId:
                    return new DHCPOptionClassId(optionLength, buffer, offset);
                case DhcpOptionType.ClasslessStaticRouteOption:
                    return new DhcpOptionClasslessStaticRoute(optionLength, buffer, offset);
                case DhcpOptionType.ClientId:
                    return new DHCPOptionClientId(optionLength, buffer, offset);
                case DhcpOptionType.DHCPMaxMsgSize:
                    return new DHCPOptionMaximumMessageSize(optionLength, buffer, offset);
                case DhcpOptionType.DHCPMessage:
                    return new DHCPOptionDHCPMessage(optionLength, buffer, offset);
                case DhcpOptionType.DHCPMsgType:
                    return new DHCPOptionDHCPMessageType(optionLength, buffer, offset);
                case DhcpOptionType.DHCPServerId:
                    return new DHCPOptionDHCPServerIdentifier(optionLength, buffer, offset);
                case DhcpOptionType.DomainName:
                    return new DHCPOptionDomainName(optionLength, buffer, offset);
                case DhcpOptionType.DomainServer:
                    return new DHCPOptionDomainNameServer(optionLength, buffer, offset);
                case DhcpOptionType.Hostname:
                    return new DHCPOptionHostname(optionLength, buffer, offset);
                case DhcpOptionType.NETBIOSNameSrv:
                    return new DHCPOptionNetBIOSOverTCPIPNameServer(optionLength, buffer, offset);
                case DhcpOptionType.NTPServers:
                    return new DHCPOptionNTPServer(optionLength, buffer, offset);
                case DhcpOptionType.Pad:
                    return new DHCPOptionPad(optionLength, buffer, offset);
                case DhcpOptionType.ParameterList:
                    return new DHCPOptionParameterList(optionLength, buffer, offset);
                case DhcpOptionType.RebindingTime:
                    return new DHCPOptionRebindingTimeValue(optionLength, buffer, offset);
                case DhcpOptionType.RelayAgentInformation:
                    return new DHCPOptionRelayAgentInformation(optionLength, buffer, offset);
                case DhcpOptionType.RenewalTime:
                    return new DHCPOptionRenewalTimeValue(optionLength, buffer, offset);
                case DhcpOptionType.RootPath:
                    return new DHCPOptionRootPath(optionLength, buffer, offset);
                case DhcpOptionType.Router:
                    return new DHCPOptionRouter(optionLength, buffer, offset);
                case DhcpOptionType.ServerName:
                    return new DHCPOptionTFTPServerName(optionLength, buffer, offset);
                case DhcpOptionType.SubnetMask:
                    return new DHCPOptionSubnetMask(optionLength, buffer, offset);
                case DhcpOptionType.TFTPserveraddress:
                    return new DHCPOptionTFTPServer(optionLength, buffer, offset);
                case DhcpOptionType.TimeOffset:
                    return new DHCPOptionTimeOffset(optionLength, buffer, offset);
                case DhcpOptionType.TimeServer:
                    return new DHCPOptionTimeServer(optionLength, buffer, offset);
                case DhcpOptionType.UserClass:
                    return new DHCPOptionUserClass(optionLength, buffer, offset);
                case DhcpOptionType.ClientSystem:
                    return new DHCPOptionClientSystem(optionLength, buffer, offset);
                case DhcpOptionType.VIVendorSpecificInformation:
                    return new DHCPOptionVendorSpecificInformation(optionLength, buffer, offset);
                default:
                    return new DHCPOptionUnrecognized(optionNumber, optionLength, buffer, offset);
            }
        }
    }
}

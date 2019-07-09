using System;
using System.Collections.Generic;
using System.Net;
using Pixey.Dhcp.Enums;
using Pixey.Dhcp.HardwareAddressTypes;

namespace Pixey.Dhcp
{
    public interface IDhcpPacketView
    {
        IPAddress AddressRequest { get; set; }
        string BootFile { get; set; }
        IPAddress BroadcastAddress { get; set; }
        bool BroadcastFlag { get; set; }
        byte[] ClassId { get; set; }
        ClientHardwareAddress ClientHardwareAddress { get; set; }
        ClientHardwareAddress ClientId { get; set; }
        IPAddress ClientIP { get; set; }
        string DHCPMessage { get; set; }
        DHCPMessageType DHCPMessageType { get; set; }
        IPAddress DHCPServerIdentifier { get; set; }
        string DomainName { get; set; }
        List<IPAddress> DomainNameServers { get; set; }
        int Hops { get; set; }
        string Hostname { get; set; }
        TimeSpan IPAddressLeaseTime { get; set; }
        int MaximumMessageSize { get; set; }
        List<IPAddress> WINSServer { get; set; }
        IPAddress NextServerIP { get; set; }
        List<IPAddress> NTPServers { get; set; }
        List<DHCPOptionType> ParameterList { get; set; }
        IPAddress RelayAgentIP { get; set; }
        TimeSpan RebindingTimeValue { get; set; }
        byte[] RelayAgentCircuitId { get; set; }
        byte[] RelayAgentRemoteId { get; set; }
        TimeSpan RenewalTimeValue { get; set; }
        string RootPath { get; set; }
        List<IPAddress> Routers { get; set; }
        string ServerHostname { get; set; }
        IPAddress SubnetMask { get; set; }
        string TFTPBootfile { get; set; }
        string TFTPServer { get; set; }
        string UserClass { get; set; }
        ClientSystem ClientSystem { get; set; }
        List<IPAddress> TFTPServer150 { get; set; }
        TimeSpan TimeElapsed { get; set; }
        TimeSpan TimeOffset { get; set; }
        List<IPAddress> TimeServers { get; set; }
        UInt32 TransactionId { get; set; }
        IPAddress YourIP { get; set; }
    }
}
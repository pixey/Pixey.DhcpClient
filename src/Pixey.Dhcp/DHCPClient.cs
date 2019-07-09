using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pixey.Dhcp.Enums;
using Pixey.Dhcp.Extensions;

namespace Pixey.Dhcp
{
    public class DhcpClient : IDhcpClient
    {
        private const int DhcpServerPort = 67;

        private static readonly IPEndPoint BroadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, DhcpServerPort);

        private readonly IDhcpListener _dhcpListener;
        private readonly ILogger<DhcpClient> _logger;
        private readonly Random _random;

        public DhcpClient(IDhcpListener dhcpListener, ILogger<DhcpClient> logger)
        {
            _dhcpListener = dhcpListener;
            _logger = logger;
            _random = new Random();
        }

        /// <summary>
        /// This method is NOT thread safe
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<IDhcpPacketView>> DiscoverDhcpServers(DhcpDiscoveryParameters parameters, CancellationToken ct)
        {
            var results = new List<IDhcpPacketView>();
            var transactionId = GenerateTransactionId();

            var receptionDelegate = new EventHandler<DhcpPacketViewEventArgs>(
                (sender, args) =>
                {
                    if (args.DhcpPacketView.TransactionId == transactionId)
                    {
                        _logger.LogDebug("Received a DHCP Packet for transaction {0}", transactionId);

                        results.Add(args.DhcpPacketView);
                    }
                });

            _dhcpListener.PacketReceived += receptionDelegate;
            _dhcpListener.StartIfNotRunning();

            using (var udpClient = new UdpClient())
            {
                var broadcastPacket = CreateBroadcastPacket(parameters, transactionId);
                var broadcastPacketBytes = await broadcastPacket.GetBytes().ConfigureAwait(false);

                _logger.LogDebug("Sending broadcast DHCP Packet for transaction {0}", transactionId);

                await udpClient
                    .SendAsync(broadcastPacketBytes, broadcastPacketBytes.Length, BroadcastEndpoint)
                    .WithCancellation(ct)
                    .ConfigureAwait(false);
            }

            await Task.Delay(parameters.Timeout, ct).ConfigureAwait(false);

            _dhcpListener.PacketReceived -= receptionDelegate;

            if (results.Count == 0)
            {
                throw new TimeoutException($"No DHCP response recieved for transaction {transactionId}.");
            }

            return results;
        }

        private uint GenerateTransactionId()
        {
            return (uint) _random.Next(100000, 100000000);
        }

        private DhcpPacketView CreateBroadcastPacket(DhcpDiscoveryParameters parameters, uint transactionId)
        {
            var discoveryPacketView = new DhcpPacketView(DHCPMessageType.DHCPDISCOVER);

            discoveryPacketView.ClientHardwareAddress = parameters.ClientHardwareAddress;
            discoveryPacketView.TransactionId = transactionId;
            discoveryPacketView.BroadcastFlag = true;

            return discoveryPacketView;
        }
    }
}
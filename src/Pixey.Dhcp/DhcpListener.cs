using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pixey.Dhcp.Extensions;

namespace Pixey.Dhcp
{
    public class DhcpListener : IDhcpListener
    {
        private const int DhcpClientPort = 68;

        private Task _listeningTask;

        private readonly ILogger<DhcpListener> _logger;
        private readonly object _listeningTaskLock = new object();
        private readonly UdpClient _udpClient;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IPEndPoint _udpClientEndpoint;

        public DhcpListener(ILogger<DhcpListener> logger)
        {
            _logger = logger;

            _cancellationTokenSource = new CancellationTokenSource();

            _udpClientEndpoint = new IPEndPoint(IPAddress.Any, DhcpClientPort);

            _udpClient = new UdpClient();
            _udpClient.EnableBroadcast = true;
            _udpClient.ExclusiveAddressUse = false;
        }

        public event EventHandler<DhcpPacketViewEventArgs> PacketReceived;

        public void StartIfNotRunning()
        {
            if (_listeningTask == null)
            {
                lock (_listeningTaskLock)
                {
                    if (_listeningTask == null)
                    {
                        _udpClient.Client.Bind(_udpClientEndpoint);

                        _listeningTask = ListenContinously(_cancellationTokenSource.Token);
                    }
                }
            }
        }

        private async Task ListenContinously(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var receivedBytes = await _udpClient
                    .ReceiveAsync()
                    .WithCancellation(ct);

                try
                {
                    var dhcpPacket = new DhcpPacketView(receivedBytes.Buffer);

                    OnPacketReceived(dhcpPacket);
                }
                catch (DhcpParseException e)
                {
                    _logger.LogDebug(e, "Received invalid UDP packet which could not be parsed as DHCP packet.");
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();

            WaitForListeningTaskToComplete();

            _udpClient.Dispose();

            _cancellationTokenSource.Dispose();
        }

        private void WaitForListeningTaskToComplete()
        {
            try
            {
                _listeningTask?.Wait();
            }
            catch (AggregateException e)
            {
                if (!(e.InnerException is TaskCanceledException))
                {
                    throw;
                }
            }
        }

        private void OnPacketReceived(DhcpPacketView packet)
        {
            var args = new DhcpPacketViewEventArgs(packet);

            PacketReceived?.Invoke(this, args);
        }
    }
}
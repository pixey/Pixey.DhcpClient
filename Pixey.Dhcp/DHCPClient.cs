using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Pixey.Dhcp.Enums;

namespace Pixey.Dhcp
{
    public class DhcpClient : IDhcpClient
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

        public Task<DHCPPacketView> RequestIpAddress()
        {
            return RequestIpAddress(IPAddress.Any);
        }

        public async Task<DHCPPacketView> RequestIpAddress(IPAddress sourceIp)
        {
            try
            {
                var clientEndpoint = new IPEndPoint(sourceIp, 67);

                using (var udpClient = new UdpClient(clientEndpoint))
                {
                    await SendDiscoveryPacket(udpClient); // TODO: Add architecture

                    var listenTask = udpClient.ReceiveAsync();

                    if (!listenTask.Wait(Timeout))
                    {
                        throw new TimeoutException();
                    }

                    return new DHCPPacketView(listenTask.Result.Buffer);
                }
            }
            catch (Exception e)
            {
                // TODO: Throw custom exception
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task SendDiscoveryPacket(UdpClient udpClient)
        {
            var discoveryPacket = new DHCPPacketView(DHCPMessageType.DHCPDISCOVER);
            
            // Client architecture = Option 93      => ClientSystem
            // User class = 77                      => UserClass, might need to be added

            discoveryPacket.ClassId = new byte[20];

            var discoveryBytes = await discoveryPacket.GetBytes().ConfigureAwait(false);

            await udpClient.SendAsync(discoveryBytes, discoveryBytes.Length).ConfigureAwait(false);
        }
    }

    public interface IDhcpClient
    {
        Task<DHCPPacketView> RequestIpAddress();

        Task<DHCPPacketView> RequestIpAddress(IPAddress sourceIp);
    }
}
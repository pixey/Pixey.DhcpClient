using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Pixey.Dhcp.Enums;
using Pixey.Dhcp.HardwareAddressTypes;
using Pixey.Dhcp.Utility;

namespace Pixey.Dhcp
{
    public class DhcpClient : IDhcpClient
    {
        private const int DhcpServerPort = 67;
        private const int DhcpClientPort = 68;

        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(20);
        private static readonly IPEndPoint BroadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, DhcpServerPort);
        //private static readonly IPEndPoint BroadcastEndpoint = new IPEndPoint(IPAddress.Parse("192.168.1.2"), DhcpServerPort);

        private readonly IPEndPoint _udpClientEndpoint;

        public DhcpClient()
        {
            _udpClientEndpoint = new IPEndPoint(IPAddress.Any, DhcpClientPort);
        }

        public async Task<DHCPPacketView> DiscoverDhcpServers(string macAddress)
        {
            try
            {
                var udpClient = new UdpClient();
                {
                    udpClient.EnableBroadcast = true;
                    udpClient.ExclusiveAddressUse = false;

                    udpClient.Client.Bind(_udpClientEndpoint);
                    // udpClient.Connect(BroadcastEndpoint);

                    //var listenTask = udpClient.ReceiveAsync();

                    var from = new IPEndPoint(0, 0);

                    //Task.Run(() =>
                    //{
                    //    while (true)
                    //    {
                    //        var recvBuffer = udpClient.Receive(ref from);
                    //        Console.WriteLine(Encoding.UTF8.GetString(recvBuffer));
                    //    }
                    //});

                    //udpClient.BeginReceive(result =>
                    //{
                    //    var from = new IPEndPoint(0, 0);
                    //    var data = udpClient.EndReceive(result, ref from);

                    //    var dhcpResult = new DHCPPacketView(data);

                    //    Console.WriteLine("Dummy");
                    //}, null);


                    //Task.Factory.StartNew(() =>
                    //{
                    //    var bytes = udpClient.Receive(ref from);

                    //    Console.WriteLine("Message received");
                    //});

                    //udpClient.ReceiveAsync()
                    //    .ContinueWith(result =>
                    //    {
                    //        var bytes = result.Result;

                    //        Console.WriteLine("Received response...");
                    //    }).Start();

                    Console.WriteLine("Sending discovery packet...");
                    await SendDiscoveryPacket(udpClient, macAddress); // TODO: Add architecture

                    Console.WriteLine("Waiting to receive...");
                    var bytes = udpClient.Receive(ref from);

                    //IPEndPoint from = BroadcastEndpoint;
                    //var responseBytes = udpClient.Receive(ref from);

                    //if (!listenTask.Wait(Timeout))
                    //{
                    //    throw new TimeoutException();
                    //}

                    // return new DHCPPacketView(response.Buffer);

                    return new DHCPPacketView(bytes);
                }
            }
            catch (Exception e)
            {
                // TODO: Throw custom exception
                Console.WriteLine(e);
                throw;
            }
        }

        public void Dispose()
        {
        }

        private async Task SendDiscoveryPacket(UdpClient udpClient, string mac)
        {
            var discoveryPacketView = new DHCPPacketView(DHCPMessageType.DHCPDISCOVER);
            var random = new Random();

            var macBytes = ParserTools.ParseMacAddress(mac);
            discoveryPacketView.ClientHardwareAddress = new EthernetClientHardwareAddress(macBytes);
            discoveryPacketView.TransactionId = (uint)random.Next(100000, 10000000);
            discoveryPacketView.BroadcastFlag = true;


            // Client architecture = Option 93      => ClientSystem
            // User class = 77                      => UserClass, might need to be added

            // discoveryPacket.ClassId = new byte[20];

            var discoveryBytes = await discoveryPacketView.GetBytes();

            //await udpClient.SendAsync(discoveryBytes, discoveryBytes.Length, BroadcastEndpoint).ConfigureAwait(false);
            udpClient.Send(discoveryBytes, discoveryBytes.Length, BroadcastEndpoint);
        }
    }

    public interface IDhcpClient : IDisposable
    {
        Task<DHCPPacketView> DiscoverDhcpServers(string mac);
    }

    public class BroadcastParameters
    {
        public string PhysicalAddress { get; set; }

        public string ClientSystem { get; set; }
    }
}
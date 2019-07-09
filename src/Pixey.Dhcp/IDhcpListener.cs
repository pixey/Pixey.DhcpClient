using System;

namespace Pixey.Dhcp
{
    public interface IDhcpListener : IDisposable
    {
        event EventHandler<DhcpPacketViewEventArgs> PacketReceived;

        void StartIfNotRunning();
    }
}
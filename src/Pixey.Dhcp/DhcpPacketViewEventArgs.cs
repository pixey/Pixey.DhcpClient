using System;

namespace Pixey.Dhcp
{
    public class DhcpPacketViewEventArgs : EventArgs
    {
        public IDhcpPacketView DhcpPacketView { get; }

        public DhcpPacketViewEventArgs(IDhcpPacketView dhcpPacketView)
        {
            DhcpPacketView = dhcpPacketView;
        }
    }
}
using System;
using Pixey.Dhcp.HardwareAddressTypes;

namespace Pixey.Dhcp
{
    public class DhcpDiscoveryParameters
    {
        public DhcpDiscoveryParameters()
        {
            // TODO: Set defaults

            ClientHardwareAddress = new EthernetClientHardwareAddress("02:00:00:00:11:11");

            Timeout = TimeSpan.FromSeconds(5);
        }

        /// <summary>
        /// Client's MAC address
        /// </summary>
        public ClientHardwareAddress ClientHardwareAddress { get; set; }

        /// <summary>
        /// CPU Architecture of the client system according to TODO: Add link to the official list
        /// </summary>
        public string ClientSystemArchitecture { get; set; }

        /// <summary>
        /// User class (e.g. iPXE)
        /// </summary>
        public string UserClass { get; set; }

        public TimeSpan Timeout { get; set; }
    }
}
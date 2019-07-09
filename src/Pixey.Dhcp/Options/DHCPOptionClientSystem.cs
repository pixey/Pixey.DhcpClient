﻿using System;
using System.IO;
using System.Threading.Tasks;
using Pixey.Dhcp.Enums;

namespace Pixey.Dhcp.Options
{
    internal class DHCPOptionClientSystem : DhcpOption
    {
        public ClientSystem ClientSystem { get; set; }

        public DHCPOptionClientSystem(ClientSystem clientSystem)
        {
            ClientSystem = clientSystem;
        }

        public DHCPOptionClientSystem(int optionLength, byte[] buffer, long offset)
        {
            var value = Read16UnsignedBE(buffer, offset);

            ClientSystem = (ClientSystem) Convert.ToInt32(value);
        }

        public override string ToString()
        {
            return "ClientSystem - " + ClientSystem;
        }

        public override Task Serialize(Stream stream)
        {
            return SerializeUInt16(stream, DhcpOptionType.ClientSystem, (ushort)ClientSystem);
        }
    }
}

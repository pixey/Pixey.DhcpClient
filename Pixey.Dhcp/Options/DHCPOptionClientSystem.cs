using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Pixey.Dhcp.Enums;

namespace Pixey.Dhcp.Options
{
    public class DHCPOptionClientSystem : DHCPOption
    {
        public string ClientSystem { get; set; }

        public DHCPOptionClientSystem(string tftpBootfile)
        {
            ClientSystem = tftpBootfile;
        }

        public DHCPOptionClientSystem(int optionLength, byte[] buffer, long offset)
        {
            ClientSystem = Encoding.ASCII.GetString(buffer, Convert.ToInt32(offset), optionLength);
        }

        public override string ToString()
        {
            return "ClientSystem - " + ClientSystem;
        }

        public override Task Serialize(Stream stream)
        {
            return SerializeASCII(stream, DHCPOptionType.ClientSystem, ClientSystem);
        }
    }
}

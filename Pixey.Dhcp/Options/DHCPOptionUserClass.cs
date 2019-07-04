using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Pixey.Dhcp.Enums;

namespace Pixey.Dhcp.Options
{
    class DHCPOptionUserClass : DHCPOption
    {
        public DHCPOptionUserClass(string userClass)
        {
            UserClass = userClass;
        }

        public DHCPOptionUserClass(int optionLength, byte[] buffer, long offset)
        {
            UserClass = Encoding.ASCII.GetString(buffer, Convert.ToInt32(offset), optionLength);
        }

        public string UserClass { get; set; }

        public override string ToString()
        {
            return "UserClass - " + UserClass;
        }

        public override Task Serialize(Stream stream)
        {
            return SerializeASCII(stream, DHCPOptionType.UserClass, UserClass);
        }
    }
}

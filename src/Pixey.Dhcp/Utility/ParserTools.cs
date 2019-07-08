using System;
using System.Linq;

namespace Pixey.Dhcp.Utility
{
    public static class ParserTools
    {
        public static byte[] ParseMacAddress(string mac)
        {
            return mac.Split(':', '-')
                .Select(x => Convert.ToByte(x, 16))
                .ToArray();
        }
    }
}

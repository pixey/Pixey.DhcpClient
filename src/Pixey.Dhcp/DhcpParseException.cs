using System;

namespace Pixey.Dhcp
{
    public class DhcpParseException : Exception
    {
        public DhcpParseException(string message)
            : base(message)
        {
        }
    }
}
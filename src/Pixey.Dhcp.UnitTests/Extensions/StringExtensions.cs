using System;
using System.Linq;

namespace Pixey.Dhcp.UnitTests.Extensions
{
    public static class StringExtensions
    {
        public static byte[] AsHexBytes(this string hex)
        {
            return Enumerable.Range(0, hex.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                    .ToArray();
        }
    }
}
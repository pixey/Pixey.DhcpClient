using System.Linq;
using Pixey.Dhcp.HardwareAddressTypes;
using Xunit;

namespace Pixey.Dhcp.UnitTests.HardwareAddressTypes
{
    public class GenericClientHardwareAddressShould
    {
        [Fact]
        public void CreateAddressFromByteBuffer()
        {
            var macBytes = new byte[] { 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };
            var address = new GenericClientHardwareAddress(macBytes);

            Assert.Equal(macBytes, address.GetBytes());
        }

        [Fact]
        public void CreateAddressFromByteArraySubset()
        {
            var byteArray = new byte[] { 0x0, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x0 };
            var macBytes = new byte[] { 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };

            var address = new GenericClientHardwareAddress(byteArray, 1, 6);

            Assert.Equal(macBytes, address.GetBytes());
        }

        [Fact]
        public void FormatAddressToStringFromNonAsciiBytes()
        {
            var macBytes = new byte[] { 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };
            var address = new GenericClientHardwareAddress(macBytes);

            Assert.Contains("0A,0B,0C,0D,0E,0F", address.ToString());
        }

        [Fact]
        public void FormatAddressToStringFromAsciiBytes()
        {
            var macChars = new char[] { 'A', 'B', 'C', 'D', 'E', 'F' };

            var macBytes = macChars
                .Select(x => (byte) x)
                .ToArray();

            var address = new GenericClientHardwareAddress(macBytes);

            Assert.Contains("ABCDEF", address.ToString());
        }
    }
}
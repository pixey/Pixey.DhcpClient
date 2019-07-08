using System;
using Pixey.Dhcp.HardwareAddressTypes;
using Xunit;

namespace Pixey.Dhcp.UnitTests.HardwareAddressTypes
{
    public class EthernetClientHardwareAddressShould
    {
        [Fact]
        public void CreateAddressFromBytes()
        {
            var macBytes = new byte[] { 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };
            var address = new EthernetClientHardwareAddress(macBytes);

            Assert.Equal(macBytes, address.GetBytes());
        }

        [Theory]
        [InlineData("0a:0b:0c:0d:0e:0f")]
        [InlineData("0a:0B:0c:0D:0e:0F")]
        [InlineData("0a-0B-0c-0D-0e-0F")]
        public void CreateAddressFromString(string macString)
        {
            var macBytes = new byte[] { 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };
            var address = new EthernetClientHardwareAddress(macString);

            Assert.Equal(macBytes, address.GetBytes());
        }

        [Fact]
        public void FormatAddressToString()
        {
            var macBytes = new byte[] { 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };
            var address = new EthernetClientHardwareAddress(macBytes);

            Assert.Equal("0A:0B:0C:0D:0E:0F", address.ToString());
        }

        [Fact]
        public void CloneIntoNewInstance()
        {
            var macBytes = new byte[] { 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f };
            var address = new EthernetClientHardwareAddress(macBytes);
            var cloned = address.Clone();

            Assert.False(ReferenceEquals(address, cloned));
            Assert.Equal(address.GetBytes(), cloned.GetBytes());
        }
    }
}

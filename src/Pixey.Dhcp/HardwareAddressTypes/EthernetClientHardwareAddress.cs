using System;
using System.Linq;
using Pixey.Dhcp.Enums;

namespace Pixey.Dhcp.HardwareAddressTypes
{
    public class EthernetClientHardwareAddress : ClientHardwareAddress
    {
        private readonly byte[] _address;

        public EthernetClientHardwareAddress(string address)
            : this(ParseMacBytes(address))
        {
        }

        public EthernetClientHardwareAddress(byte [] address)
        {
            _address = new byte[6];

            if (address.Length < 6)
                throw new ArgumentException("Address must contain at least 6 bytes", nameof(address));

            Array.Copy(address, _address, 6);
        }

        public override HardwareAddressType AddressType
        {
            get => HardwareAddressType.Ethernet;
        }

        public byte[] Address
        {
            get => _address;
        }

        public override int AddressLength
        {
            get => _address.Length;
        }

        public override string ToString()
        {
            return string.Join(":", _address.ToList().Select(x => Convert.ToUInt32(x).ToString("X2")).ToArray());
        }

        public override byte[] GetBytes()
        {
            return _address;
        }

        public override bool Equals(object obj)
        {
            var other = obj as EthernetClientHardwareAddress;
            if (other == null)
                return false;

            return _address.SequenceEqual(other.Address);
        }

        public override int GetHashCode()
        {
            return _address.GetHashCode();
        }

        public override ClientHardwareAddress Clone()
        {
            return new EthernetClientHardwareAddress(GetBytes());
        }

        private static byte[] ParseMacBytes(string mac)
        {
            return mac.Split(':', '-')
                .Select(x => Convert.ToByte(x, 16))
                .ToArray();
        }
    }
}

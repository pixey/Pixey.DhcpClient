using System;
using System.Linq;
using System.Text;

namespace Pixey.Dhcp.HardwareAddressTypes
{
    public class GenericClientHardwareAddress : ClientHardwareAddress
    {
        private readonly byte[] _hardwareAddress;

        public GenericClientHardwareAddress(byte[] buffer)
        {
            _hardwareAddress = new byte[buffer.Length];

            Array.Copy(buffer, 0, _hardwareAddress, 0, buffer.Length);
        }

        public GenericClientHardwareAddress(byte[] buffer, long offset, long length)
        {
            _hardwareAddress = new byte[length];

            Array.Copy(buffer, offset, _hardwareAddress, 0, length);
        }

        public override int AddressLength
        {
            get { return HardwareAddress.Length; }
        }

        public byte[] HardwareAddress
        {
            get => _hardwareAddress;
        }

        public override string ToString()
        {
            var hasControlChars = Encoding.ASCII
                .GetChars(_hardwareAddress, 0, _hardwareAddress.Length)
                .Any(char.IsControl);

            if (hasControlChars)
            {
                return "Generic - " + string.Join(",", _hardwareAddress.Select(x => x.ToString("X2")));
            }

            return "Generic - " + Encoding.ASCII.GetString(_hardwareAddress);
        }

        public override byte[] GetBytes()
        {
            return _hardwareAddress;
        }

        public override bool Equals(object obj)
        {
            var other = obj as GenericClientHardwareAddress;
            if (other == null)
                return false;

            return _hardwareAddress.SequenceEqual(other.HardwareAddress);
        }

        public override int GetHashCode()
        {
            return _hardwareAddress.GetHashCode();
        }

        public override ClientHardwareAddress Clone()
        {
            return new GenericClientHardwareAddress(GetBytes());
        }
    }
}

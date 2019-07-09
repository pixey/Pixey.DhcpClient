using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pixey.Dhcp.Enums;

namespace Pixey.Dhcp.Options
{
    internal class DHCPOptionClassId : DhcpOption
    {
        public byte[] _classId;
        public byte[] ClassId
        {
            get { return _classId; }
            set
            {
                _classId = new byte[value.Length];
                Array.Copy(value, _classId, value.Length);
            }
        }

        public DHCPOptionClassId(byte [] classId)
        {
            ClassId = classId;
        }

        public DHCPOptionClassId(int optionLength, byte[] buffer, long offset)
        {
            _classId = new byte[optionLength];
            Array.Copy(buffer, offset, _classId, 0, optionLength);
        }

        public override string ToString()
        {
            return "Class Id- " + String.Join(",", (ClassId.Select(x => x.ToString("X2"))));
        }

        public override Task Serialize(Stream stream)
        {
            return SerializeBytes(stream, DhcpOptionType.ClassId, ClassId);
        }
    }
}

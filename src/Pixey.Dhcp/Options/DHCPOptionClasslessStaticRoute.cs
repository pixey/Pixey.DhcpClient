using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Pixey.Dhcp.Enums;

namespace Pixey.Dhcp.Options
{
    internal class DhcpOptionClasslessStaticRoute : DhcpOption
    {
        public DhcpOptionClasslessStaticRoute(List<ClasslessStaticRoute> entries)
        {
            Entries = entries;
        }

        public DhcpOptionClasslessStaticRoute(int optionLength, byte[] buffer, long offset)
        {
            Entries = new List<ClasslessStaticRoute>();

            int index = 0;
            while(index < optionLength)
            {
                var prefixLength = Convert.ToInt32(buffer[index + offset]);
                index++;

                var byteLength = (prefixLength / 8) + 1;
                byte [] addressBuffer = new byte[] { 0, 0, 0, 0 };
                for(var i=0; i<byteLength; i++,index++)
                    addressBuffer[i] = buffer[index + offset];

                var prefixAddress = new IPAddress(addressBuffer);
                var nextHop = ReadIPAddress(buffer, index + offset);
                index += 4;

                Entries.Add(new ClasslessStaticRoute(new NetworkPrefix(prefixAddress, prefixLength), nextHop));
            }
        }

        public List<ClasslessStaticRoute> Entries { get; }

        public override string ToString()
        {
            return "Classless static routes - " + string.Join(",", Entries.Select(x => "{" + x.Prefix.Prefix.ToString() + "/" + x.Prefix.Length.ToString() + "->" + x.NextHop.ToString() + "}"));
        }

        private byte[] SerializeEntry(ClasslessStaticRoute entry)
        {
            var byteLength = (entry.Prefix.Length / 8) + 1;
            var result = new byte[1 + byteLength + 4];
            result[0] = Convert.ToByte(entry.Prefix.Length);
            Array.Copy(entry.Prefix.Prefix.GetAddressBytes(), 0, result, 1, byteLength);
            Array.Copy(entry.NextHop.GetAddressBytes(), 0, result, 1 + byteLength, 4);

            return result;
        }

        public override async Task Serialize(Stream stream)
        {
            var encodedEntries = Entries.Select(x => SerializeEntry(x)).ToList();

            var header = new byte[]
            {
                Convert.ToByte(DhcpOptionType.ClasslessStaticRouteOption),
                Convert.ToByte(encodedEntries.Select(x => x.Length).Sum())
            };

            await stream.WriteAsync(header, 0, header.Length);

            foreach(var entry in encodedEntries)
                await stream.WriteAsync(entry, 0, entry.Length);
        }
    }
}

﻿/// The MIT License(MIT)
/// 
/// Copyright(c) 2017 Conscia Norway AS
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.

using System.IO;
using System.Net;
using System.Threading.Tasks;
using Pixey.Dhcp.Enums;

namespace Pixey.Dhcp.Options
{
    public class DHCPOptionRequestedIPAddress : DHCPOption
    {
        public IPAddress RequestedIP { get; set; } = IPAddress.Any;

        public DHCPOptionRequestedIPAddress(IPAddress requestedIP)
        {
            RequestedIP = requestedIP;
        }

        public DHCPOptionRequestedIPAddress(int optionLength, byte[] buffer, long offset)
        {
            RequestedIP = ReadIPAddress(buffer, offset);
        }

        public override string ToString()
        {
            return "Address request : " + RequestedIP.ToString();
        }

        public override Task Serialize(Stream stream)
        {
            return SerializeIPAddress(stream, DhcpOptionType.AddressRequest, RequestedIP);
        }
    }
}

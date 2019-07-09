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

using System;
using System.IO;
using System.Threading.Tasks;
using Pixey.Dhcp.Enums;

namespace Pixey.Dhcp.Options
{
    public class DHCPOptionUnrecognized : DHCPOption
    {
        DhcpOptionType OptionNumber;
        public byte[] Data { get; set; }

        public DHCPOptionUnrecognized(int optionNumber, int optionLength, byte [] buffer, long offset)
        {
            OptionNumber = (DhcpOptionType)optionNumber;
            Data = new byte[optionLength];
            Array.Copy(buffer, offset, Data, 0, optionLength);
        }

        public override string ToString()
        {
            return "Unrecognized - " + OptionNumber.ToString();
        }

        public override Task Serialize(Stream stream)
        {
            var buffer = new byte[2 + Data.Length];

            buffer[0] = Convert.ToByte(OptionNumber);
            buffer[1] = Convert.ToByte(Data.Length);
            Array.Copy(Data, 0, buffer, 2, Data.Length);

            return stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}

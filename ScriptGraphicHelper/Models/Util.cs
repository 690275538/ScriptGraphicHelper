using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models
{
    public static class Util
    {
        public static int ToInt(this byte[] src, int offset = 0)
        {
            int value;
            value = ((src[offset] & 0xFF) << 24)
                    | ((src[offset + 1] & 0xFF) << 16)
                    | ((src[offset + 2] & 0xFF) << 8)
                    | (src[offset + 3] & 0xFF);
            return value;
        }

        public static byte[] ToBytes(this int value)
        {
            var src = new byte[4];
            src[0] = (byte)((value >> 24) & 0xFF);
            src[1] = (byte)((value >> 16) & 0xFF);
            src[2] = (byte)((value >> 8) & 0xFF);
            src[3] = (byte)(value & 0xFF);
            return src;
        }

        public static List<string> GetLocalAddress()
        {
            var result = new List<string>();
            var addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            foreach (var address in addressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    result.Add(address.ToString().Trim());
                }
            }
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.Common.Helpers
{
    public static class InviteCodeGenerator
    {
        public static readonly char[] chars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        public static string GenerateInviteCode(int length)
        {
            var code = new char[length];
            var rng = RandomNumberGenerator.Create();
            var buffer = new byte[length];

            rng.GetBytes(buffer);

            for (int i = 0; i < length; i++)
            {
                var index = buffer[i] % chars.Length;
                code[i] = chars[index];
            }
            return new string(code);
        }
    }
}

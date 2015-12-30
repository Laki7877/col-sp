﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Colsp.Api.Helper
{
    public static class AutoGenerate
    {
        private static System.Func<char, int> v = c => (int)((c <= '9') ? (c - '0') : (c - 'A' + 10));
        private static System.Func<int, char> ch = d => (char)(d + ((d < 10) ? '0' : ('A' - 10)));

        public static string NextPID(string PID)
        {
            PID = PID.ToUpper();
            var sb = new System.Text.StringBuilder(PID.Length);
            sb.Length = PID.Length;

            int carry = 1;
            for (int i = PID.Length - 1; i >= 0; i--)
            {
                int x = v(PID[i]) + carry;
                carry = x / 36;
                sb[i] = ch(x % 36);
            }
            if (carry > 0)
                return ch(carry) + sb.ToString();
            else
                return sb.ToString();
        }
    }
}
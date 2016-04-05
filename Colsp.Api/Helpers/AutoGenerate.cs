using Colsp.Api.Constants;
using Colsp.Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Colsp.Api.Helper
{
    public static class AutoGenerate
    {
        private static Func<char, int> v = c => (c <= '9') ? (c - '0') : (c - 'A' + 10);
        private static Func<int, char> ch = d => (char)(d + ((d < 10) ? '0' : ('A' - 10)));

        public static void GeneratePid(ColspEntities db,ICollection<ProductStage> products)
        {
            if (products == null)
            {
                throw new Exception("No product selected");
            }
            var currentPid = db.Pids.FirstOrDefault();
            if(currentPid == null)
            {
                currentPid = new Pid()
                {
                   CurrentPid = Constant.START_PID 
                };
                db.Pids.Add(currentPid);
            }
            foreach (var pro in products)
            {
                if (!string.IsNullOrWhiteSpace(pro.Pid))
                {
                    continue;
                }
                currentPid.CurrentPid = Generater(currentPid.CurrentPid);
                while(currentPid.CurrentPid.Any(a=> Constant.IGNORE_PID.Contains(a)))
                {
                    currentPid.CurrentPid = Generater(currentPid.CurrentPid);
                }
                pro.Pid = currentPid.CurrentPid;
                if (string.IsNullOrWhiteSpace(pro.UrlEn))
                {
                    pro.UrlEn = pro.Pid;
                }
            }
        }

        private static string Generater(string input)
        {
            var sb = new System.Text.StringBuilder(input.Length);
            sb.Length = input.Length;

            int carry = 1;
            for (int i = input.Length - 1; i >= 0; i--)
            {
                int x = v(input[i]) + carry;
                carry = x / 36;
                sb[i] = ch(x % 36);
            }
            if (carry > 0)
            {
                input = ch(carry) + sb.ToString();
            }
            else {
                input = sb.ToString();
            }
            return input;
        }

    }
}
using Colsp.Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Colsp.Api.Helper
{
    public static class AutoGenerate
    {
        private static System.Func<char, int> v = c => (int)((c <= '9') ? (c - '0') : (c - 'A' + 10));
        private static System.Func<int, char> ch = d => (char)(d + ((d < 10) ? '0' : ('A' - 10)));

        public static string NextPID(ColspEntities db, int? CategoryId)
        {
            if(CategoryId == null) { return null; }
            GlobalCategoryPID CurrentPid = db.GlobalCategoryPIDs.Find(CategoryId);
            if(CurrentPid != null)
            {
                string pid = CurrentPid.CurrentKey;
                pid = pid.ToUpper();
                var sb = new System.Text.StringBuilder(pid.Length);
                sb.Length = pid.Length;

                int carry = 1;
                for (int i = pid.Length - 1; i >= 0; i--)
                {
                    int x = v(pid[i]) + carry;
                    carry = x / 36;
                    sb[i] = ch(x % 36);
                }
                if (carry > 0) {
                    pid = ch(carry) + sb.ToString();
                }
                else {
                    pid = sb.ToString();
                }
                CurrentPid.CurrentKey = pid;
                return string.Concat(CurrentPid.CategoryAbbreviation, pid);
            }
            else
            {
                return null;
            }
        }
    }
}
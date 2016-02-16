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
                pid = Generater(pid);
                while (pid.Contains("0") 
                    || pid.Contains("D") 
                    || pid.Contains("E") 
                    || pid.Contains("F") 
                    || pid.Contains("G") 
                    || pid.Contains("H") 
                    || pid.Contains("I") 
                    || pid.Contains("M") 
                    || pid.Contains("N") 
                    || pid.Contains("O") 
                    || pid.Contains("S"))
                {
                    pid = Generater(pid);
                }
                CurrentPid.CurrentKey = pid;
                return string.Concat(CurrentPid.CategoryAbbreviation, pid).Trim();
            }
            else
            {
                string ab = NextCatAbbre(db);
                GlobalCategoryPID pid = new GlobalCategoryPID();
                pid.CategoryAbbreviation = ab;
                pid.CategoryId = CategoryId.Value;
                pid.CurrentKey = "11111";
                db.GlobalCategoryPIDs.Add(pid);
                return string.Concat(pid.CategoryAbbreviation, pid.CurrentKey).Trim();
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

        public static string NextCatAbbre(ColspEntities db)
        {
            var abbr = db.GlobalCategoryAbbrevations.Where(w => w.Active == true).SingleOrDefault();
            if(abbr != null)
            {
                string abbrString = abbr.Abbrevation;
                abbrString = abbrString.ToUpper();
                abbrString = Generater(abbrString);
                while (abbrString.Contains("0")
                    || abbrString.Contains("D")
                    || abbrString.Contains("E")
                    || abbrString.Contains("F")
                    || abbrString.Contains("G")
                    || abbrString.Contains("H")
                    || abbrString.Contains("I")
                    || abbrString.Contains("M")
                    || abbrString.Contains("N")
                    || abbrString.Contains("O")
                    || abbrString.Contains("S"))
                {
                    abbrString = Generater(abbrString);
                }
                abbr.Abbrevation = abbrString;
                return abbr.Abbrevation;
            }
            else
            {
                abbr = new GlobalCategoryAbbrevation();
                abbr.Abbrevation = "11";
                abbr.Active = true;
                db.GlobalCategoryAbbrevations.Add(abbr);
                return abbr.Abbrevation;
            }
        }
    }
}
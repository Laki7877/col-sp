using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Colsp.Api.Helper
{
    public static class GlobalCatPIDCache
    {
        private static Dictionary<string, string> GlobalCatPID = new Dictionary<string, string>();

        public static void AddPID(string catCode, string pid)
        {
            if (!GlobalCatPID.ContainsKey(catCode)){
                GlobalCatPID.Add(catCode, pid);
            }
        }

        public static string GetPID(string CatCode)
        {
            if (GlobalCatPID.ContainsKey(CatCode))
            {
                return GlobalCatPID[CatCode];
            }
            else
            {
                return null;
            }
        }

        public static Boolean ContainCatCode(string catCode)
        {
            return GlobalCatPID.ContainsKey(catCode);
        }

        public static void UpdateKey(string catCode, string pid)
        {
            if (GlobalCatPID.ContainsKey(catCode))
            {
                GlobalCatPID[catCode] = pid;
            }
        }
    }
}
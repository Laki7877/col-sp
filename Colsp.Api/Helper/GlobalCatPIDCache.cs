using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Colsp.Api.Helper
{
    public static class GlobalCatPIDCache
    {
        private static Dictionary<string, string> GlobalCatPID = new Dictionary<string, string>();

        public static void AddPID(string CatCode, string PID)
        {
            if (!GlobalCatPID.ContainsKey(CatCode)){
                GlobalCatPID.Add(CatCode, PID);
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

        public static Boolean ContainCatCode(string CatCode)
        {
            return GlobalCatPID.ContainsKey(CatCode);
        }

        public static void UpdateKey(string CatCode, string PID)
        {
            if (GlobalCatPID.ContainsKey(CatCode))
            {
                GlobalCatPID[CatCode] = PID;
            }
        }
    }
}
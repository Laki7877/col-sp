﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Colsp.Api.Helpers
{
    public class Validation
    {
        public static DateTime? ValidateDateTime(string val,string fieldName,bool required,DateTime? defaultVal = null)
        {
            if (required && string.IsNullOrWhiteSpace(val))
            {
                if(defaultVal != null)
                {
                    return defaultVal;
                }
                throw new Exception(fieldName + " is a required field");
            }
            if(val == null)
            {
                return null;
            }
            val = val.Trim();
            if (string.IsNullOrEmpty(val))
            {
                return null;
            }
            try
            {
                return Convert.ToDateTime(val);
            }
            catch
            {
                throw new Exception(fieldName + " invalid date format");
            }
        }

        public static string ValidateString(string val,string fieldName, bool required, int maxLenght, bool isAlphanumeric, string defaultVal = null)
        {
            
            if(required && string.IsNullOrWhiteSpace(val))
            {
                if (string.IsNullOrEmpty(defaultVal))
                {
                    return defaultVal;
                }
                throw new Exception(fieldName + " is a required field");
            }
            if (string.IsNullOrWhiteSpace(val)) { return val; }
            val = val.Trim();
            if (isAlphanumeric)
            {
                Regex rg = new Regex(@"^[^<>]+$");
                //Regex rg = new Regex(@"^[ก-๙A-Za-z0-9\s]*$");
                if (!rg.IsMatch(val))
                {
                    throw new Exception(fieldName + " only letters and numbers allowed");
                }
            }
            if (val.Length > maxLenght)
            {
                throw new Exception(fieldName + " field must be no longer than " + maxLenght + " characters");
            }
            return val;
        }

        public static decimal? ValidateDecimal(decimal? val, string fieldName, bool required, int maxLenght, int decimalPlace,bool isPositive, decimal? defaultVal = null)
        {
            if(required && val == null)
            {
                if (defaultVal != null)
                {
                    return defaultVal;
                }
                throw new Exception(fieldName + " is a required field");
            }
            if(val == null)
            {

                return val;
            }
            if(isPositive && decimal.Compare(val.Value,0) < 0)
            {
                throw new Exception(fieldName + " cannot be less than 0");
            }
            val = decimal.Round(val.Value, decimalPlace, MidpointRounding.AwayFromZero);
            if(val.Value.ToString().Length > maxLenght)
            {
                throw new Exception(fieldName + " field must be no longer than " + maxLenght + " characters");
            }
            return val;
        }

        public static string ValidateTaging(string val, string fieldName, bool required,bool duplicateTag,int totalTagCount,int tagLength)
        {
            if (required && string.IsNullOrEmpty(val))
            {
                throw new Exception(fieldName + " is a required field");
            }
            if (val == null) { return val; }
            var split = val.Split(',').ToList();
            if (!duplicateTag)
            {
                var dupGroup = split.GroupBy(x => x).Where(group => group.Count() > 1).SelectMany(group=>group);
                var dup = dupGroup.ToList();
                if (dup.Count > 0)
                {
                    var result = String.Join(",", dup);
                    throw new Exception(result + " has already been used");
                }
            }
            if (split.Count > totalTagCount)
            {
                throw new Exception("Cannot exceed " + totalTagCount + " tags");
            }
            val = string.Empty;
            foreach (string tag in split)
            {
                if (string.IsNullOrWhiteSpace(tag)) { continue; }
                val = string.Concat(val, ",", Validation.ValidateString(tag, fieldName, false, tagLength, true));
            }
            return val;

        }

        public static int? ValidationInteger(int? val,string fieldName, bool required, int maxLenght, int? defaultVal)
        {
            if (required && val == null)
            {
                if(defaultVal != null)
                {
                    return defaultVal.Value;
                }
                throw new Exception(fieldName + " is a required field");
            }
            if(val > maxLenght)
            {
                throw new Exception(fieldName + " field must be no larger than " + maxLenght + "");
            }
            return val;
        }

        public static string ValidateCSVColumn(string val)
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                return string.Empty;
            }
            val = val.Trim();
            if (val.Contains(","))
            {
                val = string.Concat(@"""",val,@"""");
            }
            return string.Concat(val);
        }

        public static string ValidateCSVColumn(int? val)
        {
            if (val == null)
            {
                return string.Empty;
            }
            return string.Concat(val);
        }

        public static string ValidateCSVColumn(decimal? val)
        {
            if (val == null)
            {
                return string.Empty;
            }
            return string.Concat(val);
        }

        public static string ValidateCSVColumn(DateTime? val)
        {
            if (val == null)
            {
                return string.Empty;
            }
            return val.Value.ToString("MMMM dd, yyyy");
        }

        public static string ValidateCSVColumn(TimeSpan? val)
        {
            if (val == null)
            {
                return string.Empty;
            }
            return val.Value.ToString(@"hh\:mm");
        }

        public static string ValidateCSVStringColumn(Dictionary<string, int> dic, List<string> list, string key)
        {
            if (dic.ContainsKey(key))
            {
                string val = list[dic[key]];
                if(!string.IsNullOrWhiteSpace(val))
                {
                    val = val.Trim();
                    return val;
                }
            }
            return null;
        }

        public static DateTime? ValidateCSVDatetimeColumn(Dictionary<string, int> dic, List<string> list, string key)
        {
            if (dic.ContainsKey(key))
            {
                string val = list[dic[key]];
                if (!string.IsNullOrWhiteSpace(val))
                {
                    val = val.Trim();
                    return DateTime.Parse(val);
                }
            }
            return null;
        }

        public static TimeSpan? ValidateCSVTimeSpanColumn(Dictionary<string, int> dic, List<string> list, string key)
        {
            if (dic.ContainsKey(key))
            {
                string val = list[dic[key]];
                if (!string.IsNullOrWhiteSpace(val))
                {
                    val = val.Trim();
                    return TimeSpan.Parse(val);
                }
            }
            return null;
        }

    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Colsp.Api.Helpers
{
    public class Validation
    {
        public static string ValidateString(string val,string fieldName, bool required, int maxLenght, bool isAlphanumeric)
        {
            
            if(required && string.IsNullOrEmpty(val))
            {
                throw new Exception(fieldName + " is a required field");
            }
            if (val == null) { return val; }
            val = val.Trim();
            if (isAlphanumeric )
            {
                Regex rg = new Regex(@"^[ก-๙A-Za-z0-9\s]*$");
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

        public static decimal? ValidateDecimal(decimal? val, string fieldName, bool required, int maxLenght, int decimalPlace,bool isPositive)
        {
            if(required && val == null)
            {
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

    }
}
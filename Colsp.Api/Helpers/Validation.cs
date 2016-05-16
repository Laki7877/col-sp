using Colsp.Api.Constants;
using Colsp.Model.Requests;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text.RegularExpressions;

namespace Colsp.Api.Helpers
{
    public static class Validation
    {

        private static Regex regex = new Regex("^[a-z0-9_-]+$");

        public static string ValidateUniqueName(string val, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                throw new Exception(string.Concat(fieldName + " is required"));
            }
            val = val.ToLower().Trim();
            if (!regex.IsMatch(val))
            {
                throw new Exception(string.Concat(fieldName + " can only be a-z 0-9 _ -"));
            }
            return val;
        }






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

        public static string ValidateString(string val,string fieldName, bool required, int maxLenght, bool isAlphanumeric, string defaultVal = null,List<string> valueOnly = null)
        {
            
            if(required && string.IsNullOrWhiteSpace(val))
            {
                if (defaultVal != null)
                {
                    return defaultVal;
                }
                throw new Exception(string.Concat(fieldName , " is a required field"));
            }
            if (string.IsNullOrEmpty(val))
            {
                return defaultVal;
            }
            val = val.Trim();
            if (isAlphanumeric)
            {
                Regex rg = new Regex(@"^[^<>]+$");
                //Regex rg = new Regex(@"^[ก-๙A-Za-z0-9\s]*$");
                if (!rg.IsMatch(val))
                {
                    throw new Exception(string.Concat(fieldName , " only letters and numbers allowed"));
                }
            }
            if (val.Length > maxLenght)
            {
                throw new Exception(string.Concat(fieldName , " field must be no longer than " + maxLenght + " characters"));
            }
            if (valueOnly != null)
            {
                if (!valueOnly.Contains(val))
                {
                    throw new Exception(string.Concat(fieldName, " ", string.Join(",", valueOnly), " only"));
                }
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
                throw new Exception(string.Concat(fieldName , " is a required field"));
            }
            if(val == null)
            {
                return defaultVal;
            }
            if(isPositive && decimal.Compare(val.Value,0) < 0)
            {
                throw new Exception(string.Concat(fieldName , " cannot be less than 0"));
            }
            val = decimal.Round(val.Value, decimalPlace, MidpointRounding.AwayFromZero);
            if(val.Value.ToString().Length > maxLenght)
            {
                throw new Exception(string.Concat(fieldName , " field must be no longer than " , maxLenght , " characters"));
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

        public static string ValidateCSVStringColumn(Dictionary<string, int> dic, List<string> list, string key,List<ImportHeaderRequest> header,bool require,int maxLenght,HashSet<string> errormessage,int row, string defaultValue = null)
        {
            string headerName = header.Where(w => w.MapName.Equals(key)).Select(s => s.HeaderName).FirstOrDefault();
            if (string.IsNullOrEmpty(headerName))
            {
                headerName = key;
            }
            if (dic.ContainsKey(key))
            {
                string val = list[dic[key]];
                if(!string.IsNullOrWhiteSpace(val))
                {
                    val = val.Trim();
                    if(require && string.IsNullOrEmpty(val) && defaultValue == null)
                    {
                        
                        errormessage.Add(string.Concat(headerName , " is required at row " , row));
                        return null;
                    }
                    if(val.Length > maxLenght)
                    {
                        errormessage.Add(string.Concat(headerName , " field must be no longer than " , maxLenght , " characters at row " , row));
                        return null;
                    }
                    return val;
                }
            }
            if (require)
            {
                errormessage.Add(string.Concat(headerName , " is required at row " , row));
            }
            return defaultValue;
        }

        public static int ValidateCSVIntegerColumn(Dictionary<string, int> dic, List<string> list, string key, List<ImportHeaderRequest> header, bool require, int maxLenght, HashSet<string> errormessage, int row, int defaultValue = -1)
        {
            string headerName = header.Where(w => w.MapName.Equals(key)).Select(s => s.HeaderName).FirstOrDefault();
            if (string.IsNullOrEmpty(headerName))
            {
                headerName = key;
            }
            if (dic.ContainsKey(key))
            {
                string val = list[dic[key]];
                if (!string.IsNullOrWhiteSpace(val))
                {
                    val = val.Trim();
                    if (require && string.IsNullOrEmpty(val) && defaultValue == -1)
                    {
                        errormessage.Add(string.Concat(headerName , " is required at row " , row));
                        return -1;
                    }
                    if (string.IsNullOrEmpty(val))
                    {
                        return defaultValue;
                    }
                    try
                    {
                        var tmp = decimal.ToInt32(decimal.Parse(val));
                        if (tmp > maxLenght)
                        {
                            errormessage.Add(string.Concat(headerName , " field must be no longer than " , maxLenght , " at row " , row));
                            return -1;
                        }
                        return tmp;
                    }
                    catch(Exception)
                    {
                        errormessage.Add(string.Concat("Invalid " , headerName , " at row " , row));
                    }
                }
            }
            if (require)
            {
                errormessage.Add(string.Concat(headerName , " is required at row " , row));
            }
            return defaultValue;
        }

        public static DateTime? ValidateCSVDatetimeColumn(Dictionary<string, int> dic, List<string> list, string key, List<ImportHeaderRequest> header, HashSet<string> errormessage, int row)
        {
            string headerName = header.Where(w => w.MapName.Equals(key)).Select(s => s.HeaderName).FirstOrDefault();
            if (string.IsNullOrEmpty(headerName))
            {
                headerName = key;
            }
            try
            {
                if (dic.ContainsKey(key))
                {
                    string val = list[dic[key]];
                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        val = val.Trim();
                        return Convert.ToDateTime(val, Constant.DATETIME_FORMAT);
                    }
                }
            }
            catch(Exception)
            {
                errormessage.Add(string.Concat("Invalid " , headerName , " at row " , row));
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

        public static void ValidateImage(string filename, int minWidth, int minHeight, int maxWidth, int maxHeight,int maxSize,bool isSquare)
        {
            using (Image img = Image.FromFile(filename))
            {
                if (!ImageFormat.Jpeg.Equals(img.RawFormat)
                    && !ImageFormat.Png.Equals(img.RawFormat))
                {
                    throw new Exception(string.Concat("Wrong file format. Please upload only JPG or PNG file. The size should be between ",minWidth,"x",minHeight," px to ",maxWidth,"x",maxHeight, " px and not over ", maxSize, " mbs per image."));
                }
                if (img.Width < minWidth || img.Height < minHeight)
                {
                    if(minWidth == maxWidth && minHeight == maxHeight)
                    {
                        throw new Exception(string.Concat("The size should be ", minWidth, "x", minHeight, " px and not over ", maxSize, " mbs per image."));
                    }
                    throw new Exception(string.Concat("The size should be between ", minWidth, "x", minHeight, " px to ", maxWidth, "x", maxHeight, " px and not over ", maxSize, " mbs per image."));
                }
                if (img.Width > maxWidth || img.Height > maxHeight)
                {
                    if (minWidth == maxWidth && minHeight == maxHeight)
                    {
                        throw new Exception(string.Concat("The size should be ", minWidth, "x", minHeight, " px and not over ", maxSize, " mbs per image"));
                    }
                    throw new Exception(string.Concat("The size should be between ", minWidth, "x", minHeight, " px to ", maxWidth, "x", maxHeight, " px and not over ", maxSize, " mbs per image."));
                }
                if (isSquare && img.Height != img.Width)
                {
                    throw new Exception(string.Concat("The size should be between ", minWidth, "x", minHeight, " px to ", maxWidth, "x", maxHeight, " px and not over ", maxSize, " mbs per image."));
                }
            }
        }

        public static void ValidateImage(string filename,Constant.ImageRatio ratio)
        {

            using (Image img = Image.FromFile(filename))
            {
                if (!ImageFormat.Jpeg.Equals(img.RawFormat)
                    && !ImageFormat.Png.Equals(img.RawFormat))
                {
                    throw new Exception(string.Concat("Wrong file format. Please upload only JPG or PNG file."));
                }
                if (Constant.ImageRatio.IMAGE_RATIO_16_9.Equals(ratio))
                {
                    if((img.Width / img.Height) != Constant.IMAGE_RATIO_16_9)
                    {
                        throw new Exception("The size should be 16:6");
                    }
                }
            }

            
        }

    }
}
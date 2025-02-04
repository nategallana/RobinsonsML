using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RobinsonMallLinkingWBOX.Helpers
{
    public static class Strings
    {

        public static String ToSafeString(this object obj, string defaultValue = "")
        {
            string returnValue = obj.ToSafeString();
            if (string.IsNullOrEmpty(returnValue))
            {
                returnValue = defaultValue;
            }
            return returnValue;
        }
        public static String ToSafeString(this object obj)
        {
            if (obj != null)
            {
                return Convert.ToString(obj).Trim();
            }
            return String.Empty;
        }

        public static String ToStringType(this object value)
        {
            return value.ToSafeString().ToStringType();
        }
        public static String ToStringType(this String value)
        {
            return String.Format("'{0}'", value.EscapeQuote());
        }

        public static String EscapeQuote(this String value)
        {
            value = value.ToSafeString().Trim();
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }
            return value.Replace("'", "''");
        }

        public static string RemoveNonAlphaNumeric(this string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }
            Regex regex = new Regex("[^a-zA-Z0-9]");
            return regex.Replace(value, string.Empty);
        }

        public static string RemoveNonAlphaNumeric(this object value)
        {
            return value.ToSafeString().RemoveNonAlphaNumeric();
        }

        public static string TrimDashes(this string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }
            return Regex.Replace(value.Trim(), "-{2,}", "-");
        }

        public static string TrimUnderscore(this string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }
            return Regex.Replace(value.Trim(), "_{2,}", "_");
        }
        public static string ReplaceUnderscore(this string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }
            return Regex.Replace(value.TrimDashes(), "-", "_");
        }

        public static string ParseDollarSign(this string sheetName)
        {
            if (String.IsNullOrEmpty(sheetName))
            {
                return String.Empty;
            }
            string[] array = sheetName.Split(new char[] { '$' });
            return array[0];
        }

        public static string SubStringValue(this string value, int length)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            string subValue = string.Empty;
            try
            {
                subValue = value.Substring(0, Math.Min(length, value.Length));
            }
            catch
            {
                //Ignore
            }
            return subValue;
        }

        public static string TruncatePath(this string path)
        {
            if (Regex.IsMatch(path, "^(\\w+:|\\\\)(\\\\[^\\\\]+\\\\).*(\\\\[^\\\\]+\\\\[^\\\\]+)$"))
                return Regex.Replace(path, "^(\\w+:|\\\\)(\\\\[^\\\\]+\\\\).*(\\\\[^\\\\]+\\\\[^\\\\]+)$", "$1$2...$3");
            return path;
        }
    }
}

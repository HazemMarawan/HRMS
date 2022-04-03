using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace HRMS.Helpers
{
    public static class StringHelper
    {
        public static T ParseEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
        public static string GetTextPlain(string text)
        {
            return HttpUtility.HtmlDecode(Regex.Replace(text, "<(.|\n)*?>", ""));
        }
        public static string GetTextPlain(string text, int length)
        {
            var textPlain = GetTextPlain(text);
            return textPlain.Substring(0, textPlain.Length > length ? length : textPlain.Length) + "...";
        }
    }
}

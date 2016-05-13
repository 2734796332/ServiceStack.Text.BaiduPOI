using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BaiduBusFiddler
{
    public static class Helper
    {
        /// <summary>
        /// unicode转中文（符合js规则的）
        /// </summary>
        /// <returns></returns>
        public static string UnicodeToGb(string str)
        {
            string outStr = "";
            Regex reg = new Regex(@"(?i)\\u([0-9a-f]{4})");
            outStr = reg.Replace(str, delegate(Match m1)
            {
                return ((char)Convert.ToInt32(m1.Groups[1].Value, 16)).ToString();
            });
            return outStr;
        }
        /// <summary>
        /// 截取字符串中开始和结束字符串中间的字符串
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <param name="startStr">开始字符串</param>
        /// <param name="endStr">结束字符串</param>
        /// <returns>中间字符串</returns>
        public static string Substring(ref string source, string startStr, string endStr)
        {
            string result = "";
            Int32 index = source.IndexOf(startStr) + startStr.Length;
            Int32 endIndex = source.IndexOf(endStr, index);
            //string titleStr = source.Substring(index, endIndex + 1 - index);
            result = source.Substring(index, endIndex - index);
            source = source.Substring(endIndex + 1);
            return result;
        }
        public static string ReplaceSubstring(string source, string startStr, string endStr,string replaceStr)
        {
            string result = "";
            Int32 index = source.IndexOf(startStr) + startStr.Length;
            Int32 endIndex = source.IndexOf(endStr, index);
            //string titleStr = source.Substring(index, endIndex + 1 - index);
            result = source.Substring(index, endIndex - index);
            source.Replace(result,replaceStr);            
            return source;
        }
    }
}

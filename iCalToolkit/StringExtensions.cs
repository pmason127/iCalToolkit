using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace iCalToolKit
{
    public static class StringExtensions
    {
        private static readonly Regex quotedText = new Regex(@"\""\s*([^\""]*?)\s*\""");
        public static string Encode(this string data)
        {
            return HttpUtility.UrlEncode(data);
        }
        public static string Decode(this string data)
        {
            return HttpUtility.UrlDecode(data);
        }
        public static ParsedField ParseField(this string field)
        {
            return ParseField(field,true);
        }
        public static bool IsComponent(this string str)
        {
            return str.StartsWith("BEGIN:");
        }
        public static string GetComponentName(this string str)
        {
            if (!IsComponent(str)) return null;

            string[] componentParts = str.Split(new char[] {':'});
            if (componentParts.Length != 2) return null;
            return componentParts[1];
        }
        public static ParsedField ParseField(this string infield, bool removeQuotesOnProps)
        {
            string field = null;
            if (infield.IndexOf(@"""") >= 0)
            {
                field = quotedText.Replace(infield, m =>
                {
                    return m.Groups[1].Value.Encode();
                });
            }
            else
            {
                field = infield;
            }
            if (field.IndexOf(":") >= 0)
            {
                string[] fieldData = field.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (fieldData.Length != 2)
                    return null;

                ParsedField fd = new ParsedField();
                

                if (fieldData[0].IndexOf(";") >= 0)
                {
                    ParseSemiColonData(fieldData[0], fd, true);
                }
                else
                {
                    fd.FieldName = fieldData[0].Decode();
                }

                if (fieldData[1].IndexOf(";") >= 0)
                {
                    ParseSemiColonData(fieldData[1], fd, false);
                }
                else
                {
                    fd.FieldValue = fieldData[1].Decode();
                }
               

                return fd;
            }
            return null;
        }
        public static DateTimeInfo ParseICSDateTime(this string str,string TZID = null)
        {
            return null;
        }
        private static void ParseSemiColonData(string data, ParsedField field, bool NameAtZero)
        {
            var addInfo = data.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (NameAtZero) field.FieldName = addInfo[0];
            for (var i = NameAtZero ? 1 : 0; i < addInfo.Length; i++)
            {
                if (addInfo[i].IndexOf("=") >= 0)
                {
                    var addField = addInfo[i].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    field.AdditionalInformation.Add(addField[0].Decode(), addField[1].Decode().Replace("\"", ""));
                }
                else
                {
                    field.AdditionalInformation.Add(addInfo[i].Decode(), null);
                }
            }
        }
    }
}

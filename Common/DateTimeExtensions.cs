using System;
using System.Globalization;

namespace ED.Atlas.Svc.TC.Ice.FE.Common
{
    public static class DateTimeExtensions
    {
        public static DateTime TryParseIceFormat(string text)
        {
            var dt = DateTime.MinValue;
            try
            {
                dt = DateTime.ParseExact(text, "yyyyMMdd-HH:ss:mm.fff", CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                dt = DateTime.MinValue;
            }

            return dt;
        }
    }
}
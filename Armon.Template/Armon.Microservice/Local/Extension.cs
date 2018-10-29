using NHibernate.Hql.Ast.ANTLR.Tree;
using Armon.Microservice.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Armon.Microservice.Local
{
    /// <summary>
    /// 扩展静态类
    /// </summary>
    internal static partial class Extension
    {
        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }

        public static byte[] ToBuffer7_Ex(this DateTime time)
        {
            byte year = (byte)(time.Year % 100);
            byte month = (byte)(time.Month);
            byte day = (byte)(time.Day);
            byte hour = (byte)(time.Hour);
            byte week = (byte)(time.DayOfWeek);
            byte minute = (byte)(time.Minute);
            byte second = (byte)(time.Second);
            return new byte[] { year, month, day, week, hour, minute, second };
        }

        public static byte[] ToBuffer6_Ex(this DateTime time)
        {
            byte year = (byte)(time.Year % 100);
            byte month = (byte)(time.Month);
            byte day = (byte)(time.Day);
            byte hour = (byte)(time.Hour);
            byte minute = (byte)(time.Minute);
            byte second = (byte)(time.Second);
            return new byte[] { year, month, day, hour, minute, second };
        }

        public static DateTime ToDateTime6_Ex(this byte[] bytes)
        {
            if (bytes == null || bytes.Length < 6) return DateTime.MinValue;

            return DateTime.Parse(string.Format("20{0}-{1}-{2} {3}:{4}:{5}", bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5]));
        }

        public static DateTime ToDateTime7_Ex(this byte[] bytes)
        {
            if (bytes == null || bytes.Length < 7) return DateTime.MinValue;

            return DateTime.Parse(string.Format("20{0}-{1}-{2} {3}:{4}:{5}", bytes[0], bytes[1], bytes[2], bytes[4], bytes[5], bytes[6]));
        }
    }
}
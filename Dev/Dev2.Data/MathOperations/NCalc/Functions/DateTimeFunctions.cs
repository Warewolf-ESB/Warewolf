/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using NCalc;
using NCalc.Handlers;

namespace Dev2.MathOperations.NCalc.Functions
{
    /// <summary>
    /// Registers all date/time NCalc function handlers.
    /// Mirrors the Infragistics UltraCalcFunction date/time set for cross-platform parity.
    /// </summary>
    internal static class DateTimeFunctions
    {
        internal static void Register(IDictionary<string, Action<FunctionArgs>> handlers)
        {
            handlers["DATE"]        = Date;
            handlers["DATEVALUE"]   = DateValue;
            handlers["TIMEVALUE"]   = TimeValue;
            handlers["NOW"]         = Now;
            handlers["TODAY"]       = Today;
            handlers["DAY"]         = Day;
            handlers["MONTH"]       = Month;
            handlers["YEAR"]        = Year;
            handlers["HOUR"]        = Hour;
            handlers["MINUTE"]      = Minute;
            handlers["SECOND"]      = Second;
            handlers["TIME"]        = Time;
            handlers["DAYS360"]     = Days360;
            handlers["EDATE"]       = EDate;
            handlers["EOMONTH"]     = EoMonth;
            handlers["WEEKDAY"]     = WeekDay;
            handlers["WEEKNUM"]     = WeekNum;
            handlers["WORKDAY"]     = WorkDay;
            handlers["NETWORKDAYS"] = NetworkDays;
            handlers["DATEADD"]     = DateAdd;
            handlers["DATEDIFF"]    = DateDiff;
        }

        // ── Construction ─────────────────────────────────────────────────────────────────

        private static void Date(FunctionArgs args)
        {
            args.Parameters.RequireArgs(3, "DATE");
            args.Result = new DateTime(
                args.Parameters.I32(0),
                args.Parameters.I32(1),
                args.Parameters.I32(2));
        }

        private static void DateValue(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "DATEVALUE");
            args.Result = DateTime.Parse(args.Parameters.S(0), CultureInfo.InvariantCulture).Date;
        }

        private static void TimeValue(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "TIMEVALUE");
            var s = args.Parameters.S(0);
            // TimeSpan.Parse doesn't support AM/PM notation; use DateTime.Parse for those cases
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                args.Result = dt.TimeOfDay.TotalDays;
            }
            else
            {
                args.Result = TimeSpan.Parse(s, CultureInfo.InvariantCulture).TotalDays;
            }
        }

        private static void Now(FunctionArgs args)  => args.Result = DateTime.Now;
        private static void Today(FunctionArgs args) => args.Result = DateTime.Today;

        // ── Part extraction ──────────────────────────────────────────────────────────────

        private static void Day(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "DAY");
            args.Result = (double)FunctionArgHelper.ToDate(args.Parameters[0].Evaluate()).Day;
        }

        private static void Month(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "MONTH");
            args.Result = (double)FunctionArgHelper.ToDate(args.Parameters[0].Evaluate()).Month;
        }

        private static void Year(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "YEAR");
            args.Result = (double)FunctionArgHelper.ToDate(args.Parameters[0].Evaluate()).Year;
        }

        private static void Hour(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "HOUR");
            args.Result = (double)FunctionArgHelper.ToDate(args.Parameters[0].Evaluate()).Hour;
        }

        private static void Minute(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "MINUTE");
            args.Result = (double)FunctionArgHelper.ToDate(args.Parameters[0].Evaluate()).Minute;
        }

        private static void Second(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "SECOND");
            args.Result = (double)FunctionArgHelper.ToDate(args.Parameters[0].Evaluate()).Second;
        }

        // ── Time construction ────────────────────────────────────────────────────────────

        private static void Time(FunctionArgs args)
        {
            args.Parameters.RequireArgs(3, "TIME");
            var h = args.Parameters.I32(0);
            var m = args.Parameters.I32(1);
            var s = args.Parameters.I32(2);
            // Return fractional day (modulo 86400 to wrap hours ≥ 24)
            var totalSeconds = (h * 3600 + m * 60 + s) % 86400;
            args.Result = totalSeconds / 86400.0;
        }

        // ── Date arithmetic ──────────────────────────────────────────────────────────────

        private static void Days360(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "DAYS360");
            var start  = FunctionArgHelper.ToDate(args.Parameters[0].Evaluate());
            var end    = FunctionArgHelper.ToDate(args.Parameters[1].Evaluate());
            var method = args.Parameters.Length >= 3 && args.Parameters.Bool(2); // European method?

            int d1 = start.Day, m1 = start.Month, y1 = start.Year;
            int d2 = end.Day,   m2 = end.Month,   y2 = end.Year;

            if (!method) // US (NASD) method
            {
                if (d1 == 31) d1 = 30;
                if (d2 == 31 && d1 == 30) d2 = 30;
            }
            else // European method
            {
                if (d1 == 31) d1 = 30;
                if (d2 == 31) d2 = 30;
            }
            args.Result = (double)(360 * (y2 - y1) + 30 * (m2 - m1) + (d2 - d1));
        }

        private static void EDate(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "EDATE");
            var start  = FunctionArgHelper.ToDate(args.Parameters[0].Evaluate());
            var months = args.Parameters.I32(1);
            args.Result = start.AddMonths(months);
        }

        private static void EoMonth(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "EOMONTH");
            var start  = FunctionArgHelper.ToDate(args.Parameters[0].Evaluate());
            var months = args.Parameters.I32(1);
            var target = start.AddMonths(months);
            args.Result = new DateTime(target.Year, target.Month,
                DateTime.DaysInMonth(target.Year, target.Month));
        }

        private static void WeekDay(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "WEEKDAY");
            var dt         = FunctionArgHelper.ToDate(args.Parameters[0].Evaluate());
            var returnType = args.Parameters.Length >= 2 ? args.Parameters.I32(1) : 1;

            // Return types: 1=Sun=1..Sat=7  2=Mon=1..Sun=7  3=Mon=0..Sun=6
            var dow = (int)dt.DayOfWeek; // 0=Sun
            args.Result = (double)(returnType switch
            {
                2 => dow == 0 ? 7 : dow,
                3 => dow == 0 ? 6 : dow - 1,
                _ => dow + 1
            });
        }

        private static void WeekNum(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "WEEKNUM");
            var dt = FunctionArgHelper.ToDate(args.Parameters[0].Evaluate());
            args.Result = (double)CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                dt, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        private static void WorkDay(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "WORKDAY");
            var start = FunctionArgHelper.ToDate(args.Parameters[0].Evaluate());
            var days  = args.Parameters.I32(1);

            // Collect optional holiday dates (parameters 3+)
            var holidays = new System.Collections.Generic.HashSet<DateTime>();
            for (var h = 2; h < args.Parameters.Length; h++)
            {
                try { holidays.Add(FunctionArgHelper.ToDate(args.Parameters[h].Evaluate()).Date); }
                catch { /* ignore non-date values */ }
            }

            var sign = days >= 0 ? 1 : -1;
            var remaining = Math.Abs(days);
            var current = start;
            while (remaining > 0)
            {
                current = current.AddDays(sign);
                if (current.DayOfWeek != DayOfWeek.Saturday &&
                    current.DayOfWeek != DayOfWeek.Sunday &&
                    !holidays.Contains(current.Date))
                    remaining--;
            }
            args.Result = current;
        }

        private static void NetworkDays(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "NETWORKDAYS");
            var start = FunctionArgHelper.ToDate(args.Parameters[0].Evaluate());
            var end   = FunctionArgHelper.ToDate(args.Parameters[1].Evaluate());
            var count = 0;
            var sign  = end >= start ? 1 : -1;
            var current = start.Date;
            var endDate = end.Date;
            while (current != endDate)
            {
                if (current.DayOfWeek != DayOfWeek.Saturday &&
                    current.DayOfWeek != DayOfWeek.Sunday)
                    count++;
                current = current.AddDays(sign);
            }
            // Include end date
            if (endDate.DayOfWeek != DayOfWeek.Saturday &&
                endDate.DayOfWeek != DayOfWeek.Sunday)
                count++;
            args.Result = (double)(sign * count);
        }

        private static void DateAdd(FunctionArgs args)
        {
            // DATEADD(interval, number, date)
            args.Parameters.RequireArgs(3, "DATEADD");
            var interval = args.Parameters.S(0).ToLowerInvariant();
            var number   = args.Parameters.I32(1);
            var dt       = FunctionArgHelper.ToDate(args.Parameters[2].Evaluate());

            args.Result = interval switch
            {
                "yyyy" or "year"    => dt.AddYears(number),
                "q"    or "quarter" => dt.AddMonths(number * 3),
                "m"    or "month"   => dt.AddMonths(number),
                "d"    or "day"     => dt.AddDays(number),
                "ww"   or "week"    => dt.AddDays(number * 7),
                "h"    or "hour"    => dt.AddHours(number),
                "n"    or "minute"  => dt.AddMinutes(number),
                "s"    or "second"  => dt.AddSeconds(number),
                _ => throw new InvalidOperationException($"DATEADD: unknown interval '{interval}'.")
            };
        }

        private static void DateDiff(FunctionArgs args)
        {
            // DATEDIFF(interval, date1, date2)
            args.Parameters.RequireArgs(3, "DATEDIFF");
            var interval = args.Parameters.S(0).ToLowerInvariant();
            var dt1      = FunctionArgHelper.ToDate(args.Parameters[1].Evaluate());
            var dt2      = FunctionArgHelper.ToDate(args.Parameters[2].Evaluate());
            var span     = dt2 - dt1;

            args.Result = (double)(interval switch
            {
                "yyyy" or "year"   => dt2.Year - dt1.Year,
                "q"   or "quarter" => (dt2.Year - dt1.Year) * 4 + (dt2.Month - dt1.Month) / 3,
                "m"   or "month"   => (dt2.Year - dt1.Year) * 12 + dt2.Month - dt1.Month,
                "d"   or "day"     => (long)span.TotalDays,
                "ww"  or "week"    => (long)(span.TotalDays / 7),
                "h"   or "hour"    => (long)span.TotalHours,
                "n"   or "minute"  => (long)span.TotalMinutes,
                "s"   or "second"  => (long)span.TotalSeconds,
                _ => throw new InvalidOperationException($"DATEDIFF: unknown interval '{interval}'.")
            });
        }
    }
}
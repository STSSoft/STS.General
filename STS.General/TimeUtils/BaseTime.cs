using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STS.General.TimeUtils
{
    public enum PeriodType : byte
    {
        Millisecond,
        Second,
        Minute,
        Hour,
        Day,
        Week,
        Month,
        Year
    }

    public class BaseTime
    {
        private long periodInTicks;

        public const long TICKS_PER_MILLISECOND = 10000;
        public const long TICKS_PER_SECOND = 1000 * TICKS_PER_MILLISECOND;
        public const long TICKS_PER_MINUTE = 60 * TICKS_PER_SECOND;
        public const long TICKS_PER_HOUR = 60 * TICKS_PER_MINUTE;
        public const long TICKS_PER_DAY = 24 * TICKS_PER_HOUR;

        public PeriodType PeriodType { get; private set; }
        public int Duration { get; private set; }

        public BaseTime(PeriodType periodType, int duration)
        {
            if (duration <= 0)
                throw new ArgumentException("periodValue");

            PeriodType = periodType;
            Duration = duration;

            switch (periodType)
            {
                case PeriodType.Millisecond: periodInTicks = duration * TICKS_PER_MILLISECOND; break;
                case PeriodType.Second: periodInTicks = duration * TICKS_PER_SECOND; break;
                case PeriodType.Minute: periodInTicks = duration * TICKS_PER_MINUTE; break;
                case PeriodType.Hour: periodInTicks = duration * TICKS_PER_HOUR; break;
                case PeriodType.Day: periodInTicks = duration * TICKS_PER_DAY; break;
                case PeriodType.Week: periodInTicks = duration * 7 * TICKS_PER_DAY; break;
            }
        }

        public DateTime Base(DateTime timestamp)
        {
            switch (PeriodType)
            {
                case PeriodType.Millisecond:
                case PeriodType.Second:
                case PeriodType.Minute:
                case PeriodType.Hour:
                case PeriodType.Day:
                case PeriodType.Week:
                    {
                        return new DateTime(timestamp.Ticks - timestamp.Ticks % periodInTicks);
                    }

                case PeriodType.Month:
                    {
                        int months = 12 * (timestamp.Year - 1) + timestamp.Month - 1;
                        months = months - months % Duration;
                        int year = 1 + months / 12;
                        int month = 1 + months % 12;

                        return new DateTime(year, month, 1);
                    }

                case PeriodType.Year:
                    {
                        int year = timestamp.Year;

                        return new DateTime(year - year % Duration, 1, 1);
                    }

                default:
                    throw new NotSupportedException(PeriodType.ToString());
            }
        }

        public override string ToString()
        {
            return String.Format("{0} {1}", Duration, PeriodType);
        }
    }
}

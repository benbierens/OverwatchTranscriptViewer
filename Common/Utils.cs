using System;

namespace OverwatchTranscriptViewer.Common
{
    public static class Utils
    {
        public static string FormatDuration(TimeSpan d)
        {
            var result = "";
            if (d.Days > 0) result += $"{d.Days} days, ";
            if (d.Hours > 0) result += $"{d.Hours} hours, ";
            if (d.Minutes > 0) result += $"{d.Minutes} mins, ";
            result += $"{d.Seconds} secs";
            return result;
        }

        public static string FormateDateTime(DateTime dt)
        {
            return $"{dt.Hour}:{dt.Minute}:{dt.Second}.{dt.Millisecond}";
        }
    }
}

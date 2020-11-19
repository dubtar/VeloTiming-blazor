using Google.Protobuf.WellKnownTypes;

namespace VeloTiming.Client
{
    public static class Utils
    {
        public static string FormatDate(Timestamp time)
        {
            if (time == null) return string.Empty;
            return time.ToDateTime().ToShortDateString();
        }
    }
}
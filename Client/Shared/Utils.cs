using Google.Protobuf.WellKnownTypes;

namespace VeloTiming.Client
{
	internal static class Utils
	{
		internal static string FormatDate(Timestamp time)
		{
			if (time == null) return string.Empty;
			return time.ToDateTime().ToShortDateString();
		}

		internal static string FormatTime(Timestamp time)
		{
			if (time == null) return string.Empty;
			return time.ToDateTime().ToLocalTime().ToShortTimeString();
		}
		internal static string FormatSex(Proto.Sex sex, bool anyAsEmpty = false)
		{
			return sex == Proto.Sex.Any ? (anyAsEmpty ? "" : "Ћюбой") :
				sex == Proto.Sex.Male ? "ћуж" :
				sex == Proto.Sex.Female ? "∆ен" :
				sex.ToString();
		}
	}
}
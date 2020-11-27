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
		internal static string DisplaySex(Proto.Sex sex)
		{
			return sex == Proto.Sex.Any ? "Ћюбой" :
				sex == Proto.Sex.Male ? "ћуж" :
				sex == Proto.Sex.Female ? "∆ен" :
				sex.ToString();
		}
	}
}
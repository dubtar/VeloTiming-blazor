using Google.Protobuf.WellKnownTypes;
using System;

namespace VeloTiming.Client
{
	internal static class Utils
	{
		internal static string FormatDate(Timestamp? time)
		{
			if (time == null) return string.Empty;
			return time.ToDateTime().ToShortDateString();
		}

		internal static string FormatTime(Timestamp? time)
		{
			if (time == null) return string.Empty;
			return time.ToDateTime().ToLocalTime().ToShortTimeString();
		}

		internal static string FormatTime(DateTime? endTime, DateTime? startTime)
		{
			if (endTime == null || startTime == null) return "--:--";
			var diff = endTime.Value - startTime.Value;
			return $"{Math.Floor(diff.TotalHours)}:{diff.Minutes:00}:{diff.Seconds:00}";
		}

		internal static string FormatSex(Proto.Sex sex, bool anyAsEmpty = false)
		{
			return sex == Proto.Sex.Any ? (anyAsEmpty ? "" : "Ћюбой") :
				sex == Proto.Sex.Male ? "ћуж" :
				sex == Proto.Sex.Female ? "∆ен" :
				sex.ToString();
		}
		internal static Proto.Sex ToProto(this Pages.Races.Sex sex)
		{
			return (Proto.Sex)sex;
		}

		internal static Pages.Races.Sex ToEdit(this Proto.Sex sex)
		{
			return (Pages.Races.Sex)sex;
		}
	}
}
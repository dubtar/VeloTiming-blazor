using System.Text.Json.Serialization;

namespace VeloTiming.Server.Data
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum Sex
	{
		Male = 'M', Female = 'F'
	}

	public static class SexConverter
	{
		public static Sex? FromProto(this Proto.Sex sex)
		{
			return sex == Proto.Sex.Any ? null :
				sex == Proto.Sex.Male ? Sex.Male :
				sex == Proto.Sex.Female ? Sex.Female :
				throw new System.NotImplementedException($"Unknown sex '{sex}'");
		}

		public static Proto.Sex ToProto(this Sex? sex)
		{
			return sex == null ? Proto.Sex.Any :
				sex == Sex.Male ? Proto.Sex.Male :
				sex == Sex.Female ? Proto.Sex.Female :
				throw new System.NotImplementedException($"Unknown sex '{sex}'");
		}
	}
}

using System.Text.Json.Serialization;

namespace VeloTiming.Server.Data
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum Sex
	{
		Male = 'M', Female = 'F'
	}
}

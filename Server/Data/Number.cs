using System.ComponentModel.DataAnnotations;

namespace VeloTiming.Server.Data
{
	public class Number
	{
		public Number(string id, string numberRfids)
		{
			Id = id;
			NumberRfids = numberRfids;
		}

		[Key, MaxLength(50)]
		public string Id { get; set; }

		// space-separated list of rfids (список rfid кодов через пробел)
		public string NumberRfids { get; set; }
	}
}
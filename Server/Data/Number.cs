using System.ComponentModel.DataAnnotations;

namespace VeloTiming.Server.Data
{
	public class Number
	{
		[Key, MaxLength(50)]
		public string Id { get; set; }

		// space-separated list of rfids (список rfid кодов через пробел)
		public string NumberRfids { get; set; }
	}
}
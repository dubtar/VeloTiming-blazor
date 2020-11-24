using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeloTiming.Server.Data
{
	public class RaceCategory
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Code { get; set; }
		public Sex? Sex { get; set; }
		public int? MinYearOfBirth { get; set; }
		public int? MaxYearOfBirth { get; set; }

		[ForeignKey("Race")]
		public int RaceId { get; set; }
		public virtual Race Race { get; set; }

		public virtual ICollection<StartCategory> Starts { get; set; }
	}
}

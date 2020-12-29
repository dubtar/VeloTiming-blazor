using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeloTiming.Server.Data
{
	public class RaceCategory
	{
		public RaceCategory(string name, string code)
		{
			Name = name;
			Code = code;
		}
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }

		[Required]
		public string Code { get; set; }
		public Sex? Sex { get; set; }
		public int? MinYearOfBirth { get; set; }
		public int? MaxYearOfBirth { get; set; }

		[ForeignKey("Race")]
		public int RaceId { get; set; }
		public virtual Race Race { get; set; } = null!;

		public virtual ICollection<StartCategory> Starts { get; set; } = null!;
	}
}

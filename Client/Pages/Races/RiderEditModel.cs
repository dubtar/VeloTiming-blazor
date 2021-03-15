using System;
using System.ComponentModel.DataAnnotations;
using VeloTiming.Client.Shared;

namespace VeloTiming.Client.Pages.Races
{
	public class RiderEditModel
	{
		public int Id { get; set; }

		public string Number { get; set; } = "";

		[Required(ErrorMessage = "Имя обязательно")]
		public string FirstName { get; set; } = "";

		[Required(ErrorMessage = "Фамилия обязательна")]
		public string LastName { get; set; } = "";

		public Sex Sex { get; set; }

		public event EventHandler? OnYearChange;
		private int? yearOfBirth;
		[YearRange(1900, ErrorMessage = "Год рождения некорректный")]
		public int? YearOfBirth
		{
			get
			{
				return yearOfBirth;
			}
			set { yearOfBirth = value; OnYearChange?.Invoke(this, new EventArgs()); }
		}

		public string Category { get; set; } = "";
		public string City { get; set; } = "";
		public string Team { get; set; } = "";
	}

	public enum Sex
	{
		Any = 0,
		Male = 1,
		Female = 2,
	}

	public class RiderEditCategory
	{
		public RiderEditCategory(int id, string code, string name)
		{
			Id = id;
			Code = code;
			Name = name;
		}
		internal RiderEditCategory(Proto.RaceCategory category)
		{
			Id = category.Id;
			Code = category.Code;
			Name = category.Name;
			Sex = category.Sex.ToEdit();
			MinYearOfBirth = category.MinYearOfBirth;
			MaxYearOfBirth = category.MaxYearOfBirth;
		}

		public int Id { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public Sex Sex { get; set; }
		public int? MinYearOfBirth { get; set; }
		public int? MaxYearOfBirth { get; private set; }
	}
}

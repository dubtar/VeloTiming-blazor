using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VeloTiming.Client.Shared
{
	public class YearRangeAttribute: RangeAttribute
	{
		public YearRangeAttribute(int startYear) : base(startYear, DateTime.Now.Year)
		{
		}
	}
}

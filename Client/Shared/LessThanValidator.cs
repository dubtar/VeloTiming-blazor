using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VeloTiming.Client.Shared
{
	public class LessThanAttribute: ValidationAttribute
	{
		public LessThanAttribute(string fieldToCompare)
		{
			OtherFieldName = fieldToCompare;
			ErrorMessage = $"Value should be less than {OtherFieldName}";
		}

		public string OtherFieldName { get; set; }
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value == null)
				return ValidationResult.Success;

			var otherProp = validationContext.ObjectType.GetProperty(OtherFieldName);
			if (otherProp == null)
				throw new InvalidOperationException($"{OtherFieldName} property is not found on {validationContext.ObjectType.Name}");
			var val = otherProp.GetValue(validationContext.ObjectInstance);
			if (val == null)
				return ValidationResult.Success; //other property is null

			if (Convert.ToDouble(value) > Convert.ToDouble(val))
				return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });

			return ValidationResult.Success;
		}
	}
}

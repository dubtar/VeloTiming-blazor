using System;
using System.ComponentModel.DataAnnotations;

namespace VeloTiming.Client.Shared
{
    public class GreaterThanAttribute: ValidationAttribute
	{
		public GreaterThanAttribute(string fieldToCompare)
		{
			OtherFieldName = fieldToCompare;
			ErrorMessage = $"Value should be greater than {OtherFieldName}";
		}

		public string OtherFieldName { get; set; }
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if (value == null)
				return ValidationResult.Success;

			var otherProp = validationContext.ObjectType.GetProperty(OtherFieldName);
			if (otherProp == null)
				throw new InvalidOperationException($"{OtherFieldName} property is not found on {validationContext.ObjectType.Name}");
			var val = otherProp.GetValue(validationContext.ObjectInstance);
			if (val == null)
				return ValidationResult.Success; //other property is null

			if (Convert.ToDouble(value) < Convert.ToDouble(val))
				return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName });

			return ValidationResult.Success;
		}
	}
}

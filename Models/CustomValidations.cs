using System.ComponentModel.DataAnnotations;
using System;

namespace auctions.Models.Validations
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if((DateTime)value < DateTime.Now)
                return new ValidationResult("Date must be a future date");
            return ValidationResult.Success;
        }
    }
}
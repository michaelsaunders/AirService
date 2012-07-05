using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace AirService.Web.Infrastructure.Filters
{
    /// <summary>
    /// Validation attribute that demands that a boolean value must be true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class MustBeTrueAttribute : RegularExpressionAttribute, IClientValidatable
    {
        public MustBeTrueAttribute() : base("true|True")
        {
        }

        #region IClientValidatable Members

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata,
                                                                               ControllerContext context)
        {
            var rule = new ModelClientValidationRegexRule(this.ErrorMessage, this.Pattern);
            return new[] {rule};
        }

        #endregion

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null && value is bool && (bool) value)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(this.ErrorMessage);
            }
        }
    }
}
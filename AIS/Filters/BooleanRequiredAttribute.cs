using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Reckomend.Web.Framework.MVC.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class BooleanRequiredAttribute : ValidationAttribute, IClientValidatable
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if ((bool)value)
                return ValidationResult.Success;
            return new ValidationResult(String.Format(ErrorMessageString, validationContext.DisplayName));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = FormatErrorMessage(metadata.GetDisplayName()),
                ValidationType = "checkboxtrue"
            };

            yield return rule;
        }
    }
}

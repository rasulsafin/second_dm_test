using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace Brio.Docs.Api.Validators
{
    /// <summary>
    /// Base class for custom validation attributes with error message localization support.
    /// Localized message is supplied by <see cref="IStringLocalizer{SharedLocalization}"/>.
    /// Default error message key is built from attribute name like: MyAttribute -> My_DefaultErrorMessage.
    /// </summary>
    internal abstract class ALocalizableValidationAttribute : ValidationAttribute
    {
        private string defaultErrorMessage = null;

        protected virtual string DefaultErrorMessage
        {
            get
            {
                if (defaultErrorMessage != null)
                    return defaultErrorMessage;

                var thisTypeName = GetType().Name;
                var splitIndex = thisTypeName.LastIndexOf("Attribute", StringComparison.Ordinal);
                var name = thisTypeName.Substring(0, splitIndex);
                defaultErrorMessage = $"{name}_DefaultErrorMessage";
                return defaultErrorMessage;
            }
        }

        protected ValidationResult GetLocalizedErrorResult(ValidationContext context)
        {
            var localizer = (IStringLocalizer)context.GetService(typeof(IStringLocalizer<SharedLocalization>));
            if (localizer == null)
                return new ValidationResult(DefaultErrorMessage);

            bool resourceNameSet = !string.IsNullOrEmpty(ErrorMessageResourceName);
            bool errorMessageSet = !string.IsNullOrEmpty(ErrorMessage);
            bool resourceTypeSet = ErrorMessageResourceType != null;

            string localizedFormat;
            if (!resourceNameSet && !errorMessageSet && !resourceTypeSet)
                localizedFormat = localizer.GetString(DefaultErrorMessage);
            else
                localizedFormat = localizer.GetString(ErrorMessageString);

            var message = string.Format(localizedFormat, context.DisplayName);
            return new ValidationResult(message);
        }
    }
}

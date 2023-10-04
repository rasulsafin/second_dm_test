using System;
using System.ComponentModel.DataAnnotations;
using Brio.Docs.Client;

namespace Brio.Docs.Api.Validators
{
    /// <summary>
    /// Validation attribute intended to check if supplied ID values are valid. Does NOT check if entity with supplied ID actually exists.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = true, AllowMultiple = false)]
    internal sealed class CheckValidIDAttribute : ALocalizableValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var isValid = value switch
            {
                ID<object> id => id.IsValid,
                int intID => intID > 0,
                _ => throw new InvalidOperationException(
                    $"{nameof(CheckValidIDAttribute)} can validate only int or ID<T> type")
            };

            return isValid ? ValidationResult.Success : GetLocalizedErrorResult(validationContext);
        }
    }
}

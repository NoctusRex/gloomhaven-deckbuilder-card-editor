using System.Globalization;
using System.Windows.Controls;

namespace GloomhavenDeckbuilder.CardEditor.ValidationRules
{
    public class NotEmptyValidationRule : ValidationRule
    {
        public NotEmptyValidationRule() { }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty((string)value)) return new ValidationResult(false, $"The input can not be empty.");

            return ValidationResult.ValidResult;
        }
    }
}

using System.Globalization;
using System.Windows.Controls;

namespace GloomhavenDeckbuilder.CardEditor.ValidationRules
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/dotnet/desktop/wpf/data/how-to-implement-binding-validation?view=netframeworkdesktop-4.8
    /// </summary>
    public class MinMaxValidationRule : ValidationRule
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public bool AllowX { get; set; }

        public MinMaxValidationRule() { }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (AllowX && ((string)value).ToUpper() == "X") return ValidationResult.ValidResult;

            if (!int.TryParse((string)value, out int parsedValue)) return new ValidationResult(false, $"The input is not a number.");
            if ((parsedValue < Min) || (parsedValue > Max)) return new ValidationResult(false, $"The value needs to be between {Min} and {Max}.");

            return ValidationResult.ValidResult;
        }
    }
}

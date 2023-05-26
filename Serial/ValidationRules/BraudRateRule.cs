using System.Globalization;
using System.Windows.Controls;

namespace Serial.ValidationRules
{
    class BraudRateRule : RuleBase<string>
    {
        public override ValidationResult Validate(string value, CultureInfo cultureInfo)
        {
            int braudRate;
            if (!int.TryParse(value, out braudRate))
            {
                return new ValidationResult(false, "必须是整数");
            }
            if (braudRate > 0)
            {
                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, "波特率必须大于零");
            }
        }
    }
}

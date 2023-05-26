using System.Globalization;
using System.Windows.Controls;

namespace Serial.ValidationRules
{
    class PortNameRule : RuleBase<string>
    {
        public override ValidationResult Validate(string value, CultureInfo cultureInfo)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, "串口名不能为空");
            }
        }
    }
}
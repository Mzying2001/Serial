using System.Globalization;
using System.Windows.Controls;

namespace Serial.WPF.ValidationRules
{
    public abstract class RuleBase<T> : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is T)
            {
                return Validate((T)value, cultureInfo);
            }
            else
            {
                return new ValidationResult(false, "数据类型错误");
            }
        }

        public abstract ValidationResult Validate(T value, CultureInfo cultureInfo);
    }
}

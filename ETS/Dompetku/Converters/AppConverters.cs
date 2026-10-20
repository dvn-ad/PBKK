using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Dompetku.Converters
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal dec)
            {
                return $"Rp {dec:N0}";
            }
            if (value is double dbl)
            {
                return $"Rp {dbl:N0}";
            }
            return "Rp 0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TransactionTypeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = value?.ToString();
            if (type == "Income")
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16A34A")); // Green
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC2626")); // Red
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hex && !string.IsNullOrWhiteSpace(hex))
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
                }
                catch
                {
                    // Fallback
                }
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2563EB"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BudgetStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double pct)
            {
                if (pct >= 100.0) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC2626"));
                if (pct >= 80.0) return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16A34A"));
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16A34A"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

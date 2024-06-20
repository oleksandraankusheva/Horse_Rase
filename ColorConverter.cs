using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;

namespace _1
{
    // Конвертер кольорів для використання в прив'язках даних
    class ColorConverter : IValueConverter
    {
        // Метод перетворення значення з моделі в прив'язане значення для відображення
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                // Перетворюємо колір у SolidColorBrush
                return new SolidColorBrush(color);
            }
            // Повертаємо незадане значення залежності, якщо вхідне значення не є кольором
            return DependencyProperty.UnsetValue;
        }
        // Зворотне перетворення не реалізоване, оскільки не потрібне
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}


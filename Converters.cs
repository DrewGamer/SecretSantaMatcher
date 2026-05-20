using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SecretSantaMatcher.Models;

namespace SecretSantaMatcher
{
    public class Converters
    {
        // Global register of participants for dynamic name resolution in the UI list
        public static Dictionary<string, string> ParticipantNameRegister { get; set; } = new();

        public static readonly IValueConverter VisibilityConverter = new StringToVisibilityConverter();
        public static readonly IValueConverter SoNameConverter = new SignificantOtherNameConverter();
    }

    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SignificantOtherNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string id && !string.IsNullOrEmpty(id))
            {
                if (Converters.ParticipantNameRegister.TryGetValue(id, out string? name) && name != null)
                {
                    return name;
                }
                return "Unknown";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

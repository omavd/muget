﻿using System;
using System.Globalization;
using Xamarin.Forms;

namespace MuGet.Forms.Converters
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
        
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

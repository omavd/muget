﻿using System;
using System.Globalization;
using Xamarin.Forms;

namespace MuGet.Forms.Converters
{
    public class StringToInverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
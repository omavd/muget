﻿using System;
using System.Collections;
using System.Globalization;
using Xamarin.Forms;

namespace MuGet.Forms.Converters
{
    public class ListToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is IList list && list.Count > 0;
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

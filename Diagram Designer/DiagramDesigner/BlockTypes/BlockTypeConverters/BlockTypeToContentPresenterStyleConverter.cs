using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DiagramDesigner.BlockTypes.BlockTypeConverters
{
    public class BlockTypeToContentPresenterStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ElementType elementType)
            {
                object FoundResource = Application.Current.Resources[elementType.ToString() + "ContentPresenter"];

                if (FoundResource is Style style && style.TargetType == typeof(ContentPresenter))
                    return style;
                else
                    return Application.Current.Resources["ElementContentPresenter"];
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
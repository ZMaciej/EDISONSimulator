using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DiagramDesigner.Converters
{
    public class OrientationToLabelVerticalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ConnectorOrientation orientation)
            {
                switch (orientation)
                {
                    case ConnectorOrientation.Bottom:
                        return VerticalAlignment.Top;
                    case ConnectorOrientation.Top:
                        return VerticalAlignment.Bottom;
                    default:
                        return VerticalAlignment.Center;
                }
            }

            return VerticalAlignment.Center;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
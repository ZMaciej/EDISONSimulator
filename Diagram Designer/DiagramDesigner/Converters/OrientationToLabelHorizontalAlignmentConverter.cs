using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DiagramDesigner.Converters
{
    public class OrientationToLabelHorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ConnectorOrientation orientation)
            {
                switch (orientation)
                {
                    case ConnectorOrientation.Left:
                        return HorizontalAlignment.Right;
                    case ConnectorOrientation.Right:
                        return HorizontalAlignment.Left;
                    default:
                        return HorizontalAlignment.Center;
                }
            }

            return HorizontalAlignment.Center;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
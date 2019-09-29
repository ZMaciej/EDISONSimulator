using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DiagramDesigner.Converters
{
    public class PortNumberToLabelMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType.FullName == "System.Windows.Thickness")
            {
                if (value.Length <= 1) return new Thickness(-8);
                if (value[1] is ConnectorOrientation connectorOrientation &&
                    (connectorOrientation == ConnectorOrientation.Top ||
                     connectorOrientation == ConnectorOrientation.Bottom))
                {
                    return new Thickness(-13);
                }

                if (!(value[0] is int intValue)) return new Thickness(-8);
                var valueLength = intValue.ToString(CultureInfo.InvariantCulture).Length;
                switch (valueLength)
                {
                    case 1:
                        return new Thickness(-8);
                    case 2:
                        return new Thickness(-12);
                    case 3:
                        return new Thickness(-18);
                    default:
                        return new Thickness(-8);
                }
            }

            throw new Exception("PortNumberToLabelMarginConverter target Type should be System.Windows.Thickness");
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
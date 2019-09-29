using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Shapes;

namespace DiagramDesigner.BlockTypes.BlockTypeConverters
{
    public class BlockTypeToPathStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ElementType elementType)
            {
                object FoundResource = Application.Current.Resources[elementType.ToString() + "Path"];

                if (FoundResource is Style style && style.TargetType == typeof(Path))
                    return style;
                else
                    return Application.Current.Resources["ElementPath"];
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
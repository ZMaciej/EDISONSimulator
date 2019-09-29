using DiagramDesigner.BlockTypes;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner
{
    // Represents a selectable item in the Toolbox/>.
    public class ToolboxItem : ContentControl
    {
        static ToolboxItem()
        {
            // set the key to reference the style for this control
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ToolboxItem), new FrameworkPropertyMetadata(typeof(ToolboxItem)));
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragObject dataObject = new DragObject();
                
                dataObject.DesiredSize = new Size(60,50);
                dataObject.Type = Type;

                DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);

                e.Handled = true;
            }
        }

        #region Type Property

        public ElementType Type { get; set; }

        #endregion
    }

    // Wraps info of the dragged object into a class
    public class DragObject
    {
        public Size DesiredSize { get; set; }
        public ElementType Type { get; set; }
    }

    public class DragString
    {
        public string DraggedString { get; set; }
        public Point DragPointStart { get; set; }
    }
}

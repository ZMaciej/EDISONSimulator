using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using DiagramDesigner.CommandManagement.Commands;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner.Controls
{
    public class ResizeThumb : Thumb
    {
        public ResizeThumb()
        {
            base.DragDelta += new DragDeltaEventHandler(ResizeThumb_DragDelta);
        }

        void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            DesignerItem designerItem = this.DataContext as DesignerItem;
            DesignerCanvas designer = VisualTreeHelper.GetParent(designerItem) as DesignerCanvas;

            if (designerItem != null && designer != null && designerItem.IsSelected)
            {
                double minLeft, minTop, minDeltaHorizontal, minDeltaVertical;
                double dragDeltaVertical, dragDeltaHorizontal, scale;

                IEnumerable<DesignerItem> selectedDesignerItems = designer.SelectionService.CurrentSelection.OfType<DesignerItem>();

                CalculateDragLimits(selectedDesignerItems, out minLeft, out minTop,
                                    out minDeltaHorizontal, out minDeltaVertical);

                foreach (DesignerItem item in selectedDesignerItems)
                {
                    if (item != null && item.ParentID == Guid.Empty && item.DataContext is ElementVM elementVm)
                    {

                        switch (base.VerticalAlignment)
                        {
                            case VerticalAlignment.Bottom:
                                dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                                scale = (elementVm.Height - dragDeltaVertical) / elementVm.Height;
                                DragBottom(scale, elementVm, designer.SelectionService);
                                break;
                            case VerticalAlignment.Top:
                                double top = Canvas.GetTop(item);
                                dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                                scale = (elementVm.Height - dragDeltaVertical) / elementVm.Height;
                                DragTop(scale, elementVm, designer.SelectionService);
                                break;
                            default:
                                break;
                        }

                        switch (base.HorizontalAlignment)
                        {
                            case HorizontalAlignment.Left:
                                double left = Canvas.GetLeft(item);
                                dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                                scale = (elementVm.Width - dragDeltaHorizontal) / elementVm.Width;
                                DragLeft(scale, elementVm, designer.SelectionService);
                                break;
                            case HorizontalAlignment.Right:
                                dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
                                scale = (elementVm.Width - dragDeltaHorizontal) / elementVm.Width;
                                DragRight(scale, elementVm, designer.SelectionService);
                                break;
                            default:
                                break;
                        }
                    }
                }
                e.Handled = true;
            }
        }

        #region Helper methods

        private void DragLeft(double scale, ElementVM elementVm, SelectionService selectionService)
        {
            double delta = elementVm.Width * (scale - 1);
            elementVm.Left = elementVm.Left - delta;
            elementVm.Width = elementVm.Width * scale;
        }

        private void DragTop(double scale, ElementVM elementVm, SelectionService selectionService)
        {
            double delta = elementVm.Height * (scale - 1);
            elementVm.Top = elementVm.Top - delta;
            elementVm.Height = elementVm.Height * scale;
        }

        private void DragRight(double scale, ElementVM elementVm, SelectionService selectionService)
        {
            elementVm.Width = elementVm.Width * scale;
        }

        private void DragBottom(double scale, ElementVM elementVm, SelectionService selectionService)
        {
            elementVm.Height = elementVm.Height * scale;
        }

        private void CalculateDragLimits(IEnumerable<DesignerItem> selectedItems, out double minLeft, out double minTop, out double minDeltaHorizontal, out double minDeltaVertical)
        {
            minLeft = double.MaxValue;
            minTop = double.MaxValue;
            minDeltaHorizontal = double.MaxValue;
            minDeltaVertical = double.MaxValue;

            // drag limits are set by these parameters: canvas top, canvas left, minHeight, minWidth
            // calculate min value for each parameter for each item
            foreach (DesignerItem item in selectedItems)
            {
                double left = Canvas.GetLeft(item);
                double top = Canvas.GetTop(item);

                minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);

                minDeltaVertical = Math.Min(minDeltaVertical, item.ActualHeight - item.MinHeight);
                minDeltaHorizontal = Math.Min(minDeltaHorizontal, item.ActualWidth - item.MinWidth);
            }
        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (this.DataContext is DesignerItem designerItem && designerItem.DataContext is ElementVM elementVM)
            {
                _oldPosition = elementVM.Position;
                _oldSize = elementVM.Size;
            }
            else
            {
                if (DataContext is DesignerItem)
                    throw new Exception("DesignerItem DataContext is not a ElementVM");
                throw new Exception("DragThumb DataContext is not a DesignerItem");
            }
        }

        private Point _oldPosition;
        private Size _oldSize;
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);
            if (this.DataContext is DesignerItem designerItem && designerItem.DataContext is ElementVM elementVM)
            {
                Element element = elementVM.Element ?? throw new Exception("Element of ElementVM could not be null when resizing occurs");
                if (element.MainModelCommandManager != null
                ) //if MainVMCommandManager is null it means that ElementVM is not added to list yet
                {
                    if (_oldPosition != element.Position)
                    {
                        PropertyChangedCommand propertyChangedCommand =
                            new PropertyChangedCommand(_oldPosition, element.Position, nameof(element.Position),
                                element, false);
                        element.MainModelCommandManager.AddToList(propertyChangedCommand);
                    }

                    if (_oldSize != element.Size)
                    {
                        PropertyChangedCommand propertyChangedCommand =
                            new PropertyChangedCommand(_oldSize, element.Size, nameof(element.Size), element,
                                false);
                        element.MainModelCommandManager.AddToList(propertyChangedCommand);
                    }
                }
            }
            else
            {
                if (DataContext is DesignerItem)
                    throw new Exception("DesignerItem DataContext is not a ElementVM");
                throw new Exception("DragThumb DataContext is not a DesignerItem");
            }
        }
        #endregion
    }
}

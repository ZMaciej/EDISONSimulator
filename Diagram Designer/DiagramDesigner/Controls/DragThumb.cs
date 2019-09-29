using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DiagramDesigner.CommandManagement.Commands;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner.Controls
{
    public class DragThumb : Thumb
    {
        public DragThumb()
        {
            base.DragDelta += new DragDeltaEventHandler(DragThumb_DragDelta);
        }

        void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            DesignerItem designerItem = this.DataContext as DesignerItem;
            DesignerCanvas designer = designerItem.Parent as DesignerCanvas;
            if (designerItem != null && designer != null && designerItem.IsSelected)
            {
                double minLeft = double.MaxValue;
                double minTop = double.MaxValue;

                // we only move DesignerItems
                var designerItems = designer.SelectionService.CurrentSelection.OfType<DesignerItem>();

                foreach (DesignerItem item in designerItems)
                {
                    double left = Canvas.GetLeft(item);
                    double top = Canvas.GetTop(item);
                    if (item.DataContext is ElementVM elementVM)
                    {
                        minLeft = double.IsNaN(elementVM.Left) ? 0 : Math.Min(elementVM.Left, minLeft);
                        minTop = double.IsNaN(elementVM.Top) ? 0 : Math.Min(elementVM.Top, minTop);
                    }
                }

                double deltaHorizontal = Math.Max(-minLeft, e.HorizontalChange);
                double deltaVertical = Math.Max(-minTop, e.VerticalChange);

                foreach (DesignerItem item in designerItems)
                {
                    if (item.DataContext is ElementVM elementVM)
                    {
                        if (double.IsNaN(elementVM.Left)) elementVM.Left = 0;
                        if (double.IsNaN(elementVM.Top)) elementVM.Top = 0;

                        elementVM.Left += deltaHorizontal;
                        elementVM.Top += deltaVertical;
                    }
                }

                designer.InvalidateMeasure();
                e.Handled = true;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            _propertyChangeCommands = new List<PropertyChangedCommand>();
            if (this.DataContext is DesignerItem dataContextDesignerItem &&
                dataContextDesignerItem.Parent is DesignerCanvas designer)
            {
                var designerItems = designer.SelectionService.CurrentSelection.OfType<DesignerItem>();
                foreach (DesignerItem item in designerItems)
                {
                    if (item.DataContext is ElementVM elementvm)
                    {
                        Element element = elementvm.Element;
                        PropertyChangedCommand newPropertyChangedCommand = new PropertyChangedCommand(element.Position, element.Position, nameof(element.Position), element, false);
                        _propertyChangeCommands.Add(newPropertyChangedCommand);
                    }
                    else throw new Exception("DataContext of DesignerItem should be type of ElementVM");
                }
            }
            else throw new Exception("DesignerCanvas is not a Parent of DragThumb DataContext");
        }

        private List<PropertyChangedCommand> _propertyChangeCommands;
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            if (_propertyChangeCommands.Count>0)
            {
                foreach (PropertyChangedCommand propertyChangedCommand in _propertyChangeCommands)
                {
                    if (propertyChangedCommand.Source is Element element)
                        propertyChangedCommand.PropertyNewValue = element.Position;
                    else throw new Exception("Source of propertyChangedCommand should be type of Element");
                }

                {
                    if (_propertyChangeCommands[0].Source is Element element)
                    {
                        GroupPropertyChangeCommand newGroupPropertyChangeCommand = new GroupPropertyChangeCommand(_propertyChangeCommands);
                        element.MainModelCommandManager.AddToList(newGroupPropertyChangeCommand);
                    }
                    else throw new Exception("Source of propertyChangedCommand should be type of Element");
                }
            }
        }
    }
}

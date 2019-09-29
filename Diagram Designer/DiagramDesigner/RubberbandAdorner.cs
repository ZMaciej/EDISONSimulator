using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner
{
    public class RubberbandAdorner : Adorner
    {
        private Point? startPoint;
        private Point? endPoint;
        private Pen SelectWhenTouchRubberbandPen;
        private Pen SelectWhenContainsRubberbandPen;

        private DesignerCanvas designerCanvas;

        public RubberbandAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
            : base(designerCanvas)
        {
            this.designerCanvas = designerCanvas;
            this.startPoint = dragStartPoint;

            SolidColorBrush SelectWhenTouchBrush = new SolidColorBrush(Colors.ForestGreen);
            SelectWhenTouchBrush.Opacity = 0.7;

            SolidColorBrush SelectWhenContainsBrush = new SolidColorBrush(Colors.DodgerBlue);
            SelectWhenContainsBrush.Opacity = 0.7;

            SelectWhenTouchRubberbandPen = new Pen(SelectWhenTouchBrush, 1.5);
            SelectWhenContainsRubberbandPen = new Pen(SelectWhenContainsBrush, 1.5);

            //rubberbandPen.DashStyle = new DashStyle(new double[] { 2 }, 1);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                endPoint = e.GetPosition(this);
                UpdateSelection();
                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }

            e.Handled = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                // release mouse capture
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();

                // remove this adorner from adorner layer
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this.designerCanvas);
                if (adornerLayer != null)
                    adornerLayer.Remove(this);
            }
            e.Handled = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // without a background the OnMouseMove event would not be fired!
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            SolidColorBrush SelectWhenTouchFillBrush = new SolidColorBrush(Colors.ForestGreen);
            SelectWhenTouchFillBrush.Opacity = 0.1;
            SolidColorBrush SelectWhenContainsFillBrush = new SolidColorBrush(Colors.DodgerBlue);
            SelectWhenContainsFillBrush.Opacity = 0.1;

            if (this.startPoint.HasValue && this.endPoint.HasValue)
            {
                if (this.startPoint.Value.X > this.endPoint.Value.X)
                    dc.DrawRectangle(SelectWhenTouchFillBrush, SelectWhenTouchRubberbandPen, new Rect(this.startPoint.Value, this.endPoint.Value));
                else
                    dc.DrawRectangle(SelectWhenContainsFillBrush, SelectWhenContainsRubberbandPen, new Rect(this.startPoint.Value, this.endPoint.Value));
            }
        }

        private void UpdateSelection()
        {
            bool DeleteUnSelected = (Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) == ModifierKeys.None; //deletes unselected when ctrl or shift are not pressed

            Rect rubberBand = new Rect(this.startPoint.Value, this.endPoint.Value);
            foreach (Control item in designerCanvas.Children)
            {
                Rect itemRect;
                Rect itemBounds;
                if (item is DesignerItem designerItem && designerItem.DataContext is ElementVM elementVM)
                {
                    BoundingRectangle boundingRectangle = elementVM.BoundingRectangle;
                    itemRect = new Rect(boundingRectangle.TopLeftCorner, boundingRectangle.Size);
                    itemBounds = itemRect;
                }
                else
                {
                    itemRect = VisualTreeHelper.GetDescendantBounds(item);
                    itemBounds = item.TransformToAncestor(designerCanvas).TransformBounds(itemRect);
                }
                

                bool selectWhenTouch = this.startPoint.Value.X > this.endPoint.Value.X;
                bool selectWhenContains = this.startPoint.Value.X <= this.endPoint.Value.X;

                if ((selectWhenContains && rubberBand.Contains(itemBounds)) || (selectWhenTouch && rubberBand.IntersectsWith(itemBounds)))
                {
                    if (!(item as ISelectable).IsSelected)
                        if (item is Connection)
                            designerCanvas.SelectionService.AddToSelection(item as ISelectable);
                        else
                        {
                            DesignerItem di = item as DesignerItem;
                            if (di.ParentID == Guid.Empty)
                                designerCanvas.SelectionService.AddToSelection(di);
                        }
                }
                else if (DeleteUnSelected)
                {
                    if (item is Connection)
                        designerCanvas.SelectionService.RemoveFromSelection(item as ISelectable);
                    else
                    {
                        DesignerItem di = item as DesignerItem;
                        if (di.ParentID == Guid.Empty)
                            designerCanvas.SelectionService.RemoveFromSelection(di);
                    }
                }
            }
        }
    }
}
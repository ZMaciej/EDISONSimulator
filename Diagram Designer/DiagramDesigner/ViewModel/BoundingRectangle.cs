using System.Windows;

namespace DiagramDesigner.ViewModel
{
    public class BoundingRectangle
    {
        public BoundingRectangle(Point position, Size size)
        {
            TopLeftCorner = position;
            Size = size;
        }
        public BoundingRectangle(Point topLeftCorner, Point bottomRightCorner)
        {
            TopLeftCorner = topLeftCorner;
            BottomRightCorner = bottomRightCorner;
        }

        public BoundingRectangle()
        {
        }

        public Point TopLeftCorner { get; set; }

        private Point _bottomRightCorner;
        public Point BottomRightCorner
        {
            get => _bottomRightCorner;
            set
            {
                _bottomRightCorner = value;
                _size = new Size(value.X - TopLeftCorner.X, value.Y - TopLeftCorner.Y);
            }
        }

        private Size _size;
        public Size Size
        {
            get => _size;
            set
            {
                _size = value;
                _bottomRightCorner = new Point(TopLeftCorner.X + value.Width, TopLeftCorner.Y + value.Height);
            }
        }

        public Point Center
        {
            get
            {
                return new Point(TopLeftCorner.X + (Size.Width / 2), TopLeftCorner.Y + (Size.Height) / 2);
            }
        }
    }
}
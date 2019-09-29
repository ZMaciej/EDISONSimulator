using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DiagramDesigner.Annotations;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner.CommandManagement.Commands
{
    public class GroupRotationCommand : Command
    {
        private List<Element> _elementsToRotate;
        private BoundingRectangle _boundingRectangle;

        public GroupRotationCommand([NotNull]List<Element> elementsToRotate)
        {
            _elementsToRotate = elementsToRotate ?? throw new ArgumentNullException(nameof(elementsToRotate));
            _boundingRectangle = GetBoundingRectangle(_elementsToRotate);
        }

        public void Execute()
        {
            if (_elementsToRotate.Any())
            {
                foreach (Element element in _elementsToRotate)
                {
                    //rotate element around itself
                    element.Rotation++;
                    //rotate clockwise element around center of group
                    Point CenterOfElement =
                        new Point(element.Position.X + (element.Size.Width / 2),
                                  element.Position.Y + (element.Size.Height / 2));
                    Vector Delta = Point.Subtract(CenterOfElement, _boundingRectangle.Center);
                    Double Top = element.Position.Y + (Delta.X - Delta.Y);
                    Double Left = element.Position.X - (Delta.X + Delta.Y);
                    element.Position = new Point(Left, Top);
                }
            }
        }

        public void Undo()
        {
            if (_elementsToRotate.Any())
            {
                foreach (Element element in _elementsToRotate)
                {
                    //rotate element around itself
                    element.Rotation += 3;
                    //rotate counter-clockwise element around center of group
                    Point CenterOfElement =
                        new Point(element.Position.X + (element.Size.Width / 2),
                                  element.Position.Y + (element.Size.Height / 2));
                    Vector Delta = Point.Subtract(CenterOfElement, _boundingRectangle.Center);
                    Double Top = element.Position.Y - (Delta.Y + Delta.X);
                    Double Left = element.Position.X + (Delta.Y - Delta.X);
                    element.Position = new Point(Left,Top);
                }
            }
        }

        public BoundingRectangle GetBoundingRectangle(List<Element> Elements)
        {
            Point topLeftCorner = new Point(Double.MaxValue, Double.MaxValue);
            Point bottomRightCorner = new Point(Double.MinValue, Double.MinValue);
            foreach (Element element in Elements)
            {
                if (element.Position.X < topLeftCorner.X)
                    topLeftCorner.X = element.Position.X;
                if (element.Position.Y < topLeftCorner.Y)
                    topLeftCorner.Y = element.Position.Y;

                if ((element.Position.X + element.Size.Width) > bottomRightCorner.X)
                    bottomRightCorner.X = element.Position.X + element.Size.Width;
                if ((element.Position.Y + element.Size.Height) > bottomRightCorner.Y)
                    bottomRightCorner.Y = element.Position.Y + element.Size.Height;
            }
            return new BoundingRectangle(topLeftCorner, bottomRightCorner);
        }
    }
}
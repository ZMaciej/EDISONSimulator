using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using DiagramDesigner.ViewModel;
using DiagramDesigner.BlockTypes;
using DiagramDesigner.BlockTypes.ImportedComponent;
using DiagramDesigner.Model;
using DiagramDesigner.sNpFile;
using DiagramDesigner.ViewModel.ViewModelBases;

namespace DiagramDesigner
{
    public partial class DesignerCanvas : Canvas
    {
        private Point? _rubberbandSelectionStartPoint = null;
        private Point? _canvasDraggingStartPoint = null;
        ScrollViewer parent;

        private SelectionService _selectionService;
        private ScaleTransform ScaleCanvas = new ScaleTransform();

        internal SelectionService SelectionService
        {
            get
            {
                if (_selectionService == null)
                    _selectionService = new SelectionService(this);

                return _selectionService;
            }
        }

        #region Elements Dependency Property

        //Adding and Removing DesignerItem On Collection Changed
        private void OnElementsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ElementVM newItem in e.NewItems)
                {
                    DesignerItem NewDesignerItem = new DesignerItem();
                    NewDesignerItem.DataContext = newItem;
                    this.Children.Add(NewDesignerItem);
                    NewDesignerItem.ActivateConnectors();
                    NewDesignerItem.Focus();
                    SelectionService.SelectItem(NewDesignerItem);
                    Canvas.SetZIndex(NewDesignerItem, this.Children.Count);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (ElementVM removedItem in e.OldItems)
                {
                    DesignerItem ToRemove = GetDesignerItemByGuid(removedItem.ID);
                    Control cd = ToRemove.Template.FindName("PART_ConnectorDecorator", ToRemove) as Control;
                    List<Connector> connectors = new List<Connector>();
                    GetConnectors(cd, connectors);

                    foreach (Connector connector in connectors)
                    {
                        foreach (Connection con in connector.Connections)
                        {
                            SelectionService.RemoveFromSelection(con);
                            this.Children.Remove(con);
                        }
                    }
                    SelectionService.RemoveFromSelection(ToRemove);
                    this.Focus();
                    this.Children.Remove(ToRemove);
                }
            }
        }

        private static readonly DependencyProperty ElementsProperty = DependencyProperty.Register(nameof(Elements),
            typeof(INotifyCollectionChanged), typeof(DesignerCanvas), new UIPropertyMetadata(null, OnElementsPropertyChanged));

        private static void OnElementsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var coll = (INotifyCollectionChanged)e.NewValue;
            ((DesignerCanvas)d).Elements = coll;
        }

        private INotifyCollectionChanged _elements = new ObservableCollection<ElementVM>();

        public INotifyCollectionChanged Elements
        {
            get
            {
                return (INotifyCollectionChanged)GetValue(ElementsProperty);
            }
            set
            {
                if (_elements != null)
                    _elements.CollectionChanged -= OnElementsCollectionChanged;
                _elements = value;
                _elements.CollectionChanged += OnElementsCollectionChanged;
                SetValue(ElementsProperty, value);
            }
        }
        #endregion

        #region Connections Dependency Property

        private void OnConnectionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ConnectionVM newConnection in e.NewItems)
                {
                    Connector Source = GetConnectorByGuid(newConnection.SourceVM.ID);
                    Connector Sink = GetConnectorByGuid(newConnection.SinkVM.ID);
                    Connection NewConnection = new Connection(Source, Sink);
                    NewConnection.DataContext = newConnection;
                    this.Children.Add(NewConnection);
                    Canvas.SetZIndex(NewConnection, this.Children.Count);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (ConnectionVM removedConnection in e.OldItems)
                {
                    Connection ToRemove = GetConnectionByGuid(removedConnection.ID);
                    ToRemove.ConnectionUnload();
                    this.Children.Remove(ToRemove);
                }
            }
        }

        private static readonly DependencyProperty ConnectionsProperty = DependencyProperty.Register(nameof(Connections),
            typeof(INotifyCollectionChanged), typeof(DesignerCanvas), new UIPropertyMetadata(null, OnConnectionsPropertyChanged));

        private static void OnConnectionsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var coll = (INotifyCollectionChanged)e.NewValue;
            ((DesignerCanvas)d).Connections = coll;
        }

        private INotifyCollectionChanged _connections;
        public INotifyCollectionChanged Connections
        {
            get { return (INotifyCollectionChanged)GetValue(ConnectionsProperty); }
            set
            {
                if (_connections != null)
                    _connections.CollectionChanged -= OnConnectionsCollectionChanged;
                _connections = value;
                _connections.CollectionChanged += OnConnectionsCollectionChanged;
                SetValue(ConnectionsProperty, value);
            }
        }
        #endregion

        public Connector GetConnectorByGuid(Guid guid)
        {
            //for now it is just a prototype
            foreach (var Item in Children)
            {
                if (Item is DesignerItem designerItem)
                {
                    foreach (Connector connector in designerItem.ConnectorsRelativePositionPanel.Children)
                    {
                        if (connector.ID.Equals(guid))
                            return connector;
                    }
                }
            }
            return null;
        }
        public DesignerItem GetDesignerItemByGuid(Guid guid)
        {
            foreach (var Item in Children)
            {
                if (Item is DesignerItem designerItem && designerItem.ID.Equals(guid))
                {
                    return designerItem;
                }
            }
            return null;
        }
        public Connection GetConnectionByGuid(Guid guid)
        {
            foreach (var Item in Children)
            {
                if (Item is Connection connection && connection.ID.Equals(guid))
                {
                    return connection;
                }
            }
            return null;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Space && this.Children.Count > 0)
            {
                Rect boundingRect = GetBoundingRectangle(this.Children.OfType<DesignerItem>());

                if (parent == null)
                    parent = this.Parent as ScrollViewer;

                double desiredScale = Math.Min((parent.ViewportWidth - 20) / (boundingRect.Width + 20),
                    (parent.ViewportHeight - 20) / (boundingRect.Height + 20));
                if (desiredScale > 1) desiredScale = 1;
                ScaleCanvas.ScaleY = ScaleCanvas.ScaleX = desiredScale;
                //calculation of desired position of scrolbars
                parent.ScrollToHorizontalOffset(boundingRect.Left * desiredScale);
                parent.ScrollToVerticalOffset(boundingRect.Top * desiredScale);
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Source == this)
            {
                // in case that this click is the start of a 
                // drag operation we cache the start point
                this._rubberbandSelectionStartPoint = new Point?(e.GetPosition(this));

                if (parent == null)
                    parent = this.Parent as ScrollViewer;
                this._canvasDraggingStartPoint = new Point?(e.GetPosition(parent));

                // if you click directly on the canvas, and you are not holding Ctrl or Shift
                // all selected items are 'de-selected'
                if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) == ModifierKeys.None)
                {
                    SelectionService.ClearSelection();
                }

                Focus();
                e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // if mouse button is not pressed we have no drag operation, ...
            if (e.LeftButton != MouseButtonState.Pressed)
                this._rubberbandSelectionStartPoint = null;

            if (this._rubberbandSelectionStartPoint.HasValue)
            {
                // create rubberband adorner
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    RubberbandAdorner adorner = new RubberbandAdorner(this, _rubberbandSelectionStartPoint);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
                }
            }

            if (e.MiddleButton != MouseButtonState.Pressed)
                this._canvasDraggingStartPoint = null;

            if (this._canvasDraggingStartPoint.HasValue)
            {
                Point mousePosition = Mouse.GetPosition(parent);

                double newVerticalOffset =
                    parent.VerticalOffset + (_canvasDraggingStartPoint.Value.Y - mousePosition.Y);
                double newHorizontalOffset =
                    parent.HorizontalOffset + (_canvasDraggingStartPoint.Value.X - mousePosition.X);

                _canvasDraggingStartPoint = Mouse.GetPosition(parent);

                parent.ScrollToHorizontalOffset(newHorizontalOffset);
                parent.ScrollToVerticalOffset(newVerticalOffset);
            }

            e.Handled = true;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            //base.OnDrop(e);

            #region FileImport

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                List<string> validSNPFiles = new List<string>();
                foreach (string file in files)
                {
                    if (TouchstoneFile.ExtensionIsValid(file))
                    {
                        validSNPFiles.Add(file);
                    }
                }

                if (validSNPFiles.Count > 0)
                {
                    Point position = e.GetPosition(this);
                    Vector positionOffset = new Vector(0,0);
                    foreach (string validFile in validSNPFiles)
                    {
                        TouchstoneFile touchstoneFile = new TouchstoneFile();
                        TouchstoneResult touchstoneResult;
                        try
                        {
                            touchstoneResult = touchstoneFile.Read(validFile);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                            throw;
                        }

                        if (touchstoneResult != null)
                        {
                            ImportedComponentModel importedComponentModel = new ImportedComponentModel();
                            importedComponentModel.Size = new Size(60,50);
                            importedComponentModel.Position = Point.Add(position,positionOffset);
                            positionOffset = Vector.Add(positionOffset, new Vector(10, 10));
                            importedComponentModel.LoadFromTouchstoneResult(touchstoneResult);
                            importedComponentModel.FileName = validFile;
                            importedComponentModel.Name = Path.GetFileNameWithoutExtension(validFile);

                            if (DataContext is MainVM mainVM)
                            {
                                if (mainVM.AddElement is RelayCommand relayCommand)
                                    relayCommand.Run(importedComponentModel);
                                else
                                    throw new Exception("MainVM AddElement Command should be type of RelayCommand");
                            }
                            else
                                throw new Exception("DataContext of DesignerCanvas should be MainVM type");
                        }
                    }
                }
            }

            #endregion

            #region Drag from toolbox / Copy by drag

            DragObject dragObject = e.Data.GetData(typeof(DragObject)) as DragObject;
            DragString dragString = e.Data.GetData(typeof(DragString)) as DragString;
            if (dragObject != null)
            {
                Point position = e.GetPosition(this);

                double width = dragObject.DesiredSize.Width;
                double height = dragObject.DesiredSize.Height;

                double left = Math.Max(0, position.X - width / 2);
                double top = Math.Max(0, position.Y - height / 2);

                Element NewElement = Creator.CreateElementByType(dragObject.Type, width, height, top, left);

                if (DataContext is MainVM mainVM)
                {
                    if (mainVM.AddElement is RelayCommand relayCommand)
                        relayCommand.Run(NewElement);
                    else
                        throw new Exception("MainVM AddElement Command should be type of RelayCommand");
                }
                else
                    throw new Exception("DataContext of DesignerCanvas should be MainVM type");

                e.Handled = true;
            }
            else if (dragString != null)
            {
                if (DataContext is MainVM mainVM)
                {
                    Vector elementsShift = Point.Subtract(e.GetPosition(this), dragString.DragPointStart);
                    mainVM.ReadAndShiftElementsAndConnectionsFromString(dragString.DraggedString, elementsShift);
                }
            }

            #endregion
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size size = new Size();

            foreach (UIElement element in this.InternalChildren)
            {
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);
                left = double.IsNaN(left) ? 0 : left;
                top = double.IsNaN(top) ? 0 : top;

                //measure desired size for each child
                element.Measure(constraint);

                Size desiredSize = element.DesiredSize;
                if (!double.IsNaN(desiredSize.Width) && !double.IsNaN(desiredSize.Height))
                {
                    size.Width = Math.Max(size.Width, left + desiredSize.Width);
                    size.Height = Math.Max(size.Height, top + desiredSize.Height);
                }
            }

            // add margin 
            size.Width += 20;
            size.Height += 20;
            return size;
        }

        private void SetConnectorDecoratorTemplate(DesignerItem item)
        {
            if (item.ApplyTemplate() && item.Content is UIElement itemContent)
            {
                ControlTemplate template = DesignerItem.GetConnectorDecoratorTemplate(itemContent);
                Control decorator = item.Template.FindName("PART_ConnectorDecorator", item) as Control;
                if (decorator != null && template != null)
                    decorator.Template = template;
            }
        }

    }
}

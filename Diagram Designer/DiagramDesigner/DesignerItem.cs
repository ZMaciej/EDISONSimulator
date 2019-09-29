using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DiagramDesigner.Controls;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner
{
    //These attributes identify the types of the named parts that are used for templating
    [TemplatePart(Name = "PART_DragThumb", Type = typeof(DragThumb))]
    [TemplatePart(Name = "PART_ResizeDecorator", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ConnectorDecorator", Type = typeof(RelativePositionPanel))]
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_SelectionMarker", Type = typeof(Path))]
    public class DesignerItem : ContentControl, ISelectable
    {

        #region ID
        public static readonly DependencyProperty IDProperty = DependencyProperty.Register(
            nameof(ID), typeof(Guid), typeof(DesignerItem), new PropertyMetadata(default(Guid)));

        public Guid ID
        {
            get { return (Guid)GetValue(IDProperty); }
            set { SetValue(IDProperty, value); }
        }
        #endregion

        #region ParentID
        public Guid ParentID
        {
            get { return (Guid)GetValue(ParentIDProperty); }
            set { SetValue(ParentIDProperty, value); }
        }
        public static readonly DependencyProperty ParentIDProperty = DependencyProperty.Register("ParentID", typeof(Guid), typeof(DesignerItem));
        #endregion

        #region IsSelected Property

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty =
          DependencyProperty.Register("IsSelected",
                                       typeof(bool),
                                       typeof(DesignerItem),
                                       new FrameworkPropertyMetadata(false));

        #endregion

        #region DragThumbTemplate Property

        // can be used to replace the default template for the DragThumb
        public static readonly DependencyProperty DragThumbTemplateProperty =
            DependencyProperty.RegisterAttached("DragThumbTemplate", typeof(ControlTemplate), typeof(DesignerItem));

        public static ControlTemplate GetDragThumbTemplate(UIElement element)
        {
            return (ControlTemplate)element.GetValue(DragThumbTemplateProperty);
        }

        public static void SetDragThumbTemplate(UIElement element, ControlTemplate value)
        {
            element.SetValue(DragThumbTemplateProperty, value);
        }

        #endregion

        #region ConnectorDecoratorTemplate Property

        // can be used to replace the default template for the ConnectorDecorator
        public static readonly DependencyProperty ConnectorDecoratorTemplateProperty =
            DependencyProperty.RegisterAttached("ConnectorDecoratorTemplate", typeof(ControlTemplate), typeof(DesignerItem));

        public static ControlTemplate GetConnectorDecoratorTemplate(UIElement element)
        {
            if (element != null)
            {
                return (ControlTemplate)element.GetValue(ConnectorDecoratorTemplateProperty);
            }
            else
            {
                return null;
            }
        }

        public static void SetConnectorDecoratorTemplate(UIElement element, ControlTemplate value)
        {
            if (element != null)
            {
                element.SetValue(ConnectorDecoratorTemplateProperty, value);
            }
        }

        #endregion

        #region Connectors Dependency Property

        public static readonly DependencyProperty ConnectorsProperty = DependencyProperty.Register(
            nameof(Connectors), typeof(ObservableCollection<ConnectorVM>), typeof(DesignerItem), new UIPropertyMetadata(null, OnConnectorsPropertyChanged));

        private static void OnConnectorsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var coll = (ObservableCollection<ConnectorVM>)e.NewValue;
            ((DesignerItem)d).Connectors = coll;
        }

        private ObservableCollection<ConnectorVM> _connectors;
        public ObservableCollection<ConnectorVM> Connectors
        {
            get { return (ObservableCollection<ConnectorVM>)GetValue(ConnectorsProperty); }
            set
            {
                if (_connectors != null)
                    _connectors.CollectionChanged -= OnConnectorsCollectionChanged;
                _connectors = value;
                _connectors.CollectionChanged += OnConnectorsCollectionChanged;
                SetValue(ConnectorsProperty, value);
            }
        }

        public void ActivateConnectors()
        {
            if (_connectors.Count > 0)
                OnConnectorsCollectionChanged(_connectors, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _connectors));
        }

        private void OnConnectorsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //create connector, connect it with viewmodel and paste to relativePositionPanel
                foreach (ConnectorVM newConnectorVM in e.NewItems)
                {
                    Connector New = new Connector();
                    New.ViewModel = newConnectorVM;
                    ConnectorsRelativePositionPanel.Children.Add(New);
                }
            }
        }

        #endregion

        #region CanvasWidth Dependency Property

        public static readonly DependencyProperty CanvasWidthProperty =
            DependencyProperty.Register("CanvasWidth", typeof(double), typeof(DesignerItem), new PropertyMetadata());

        public double CanvasWidth
        {
            get
            {
                return (double)GetValue(CanvasWidthProperty);
            }
            set { SetValue(CanvasWidthProperty, value); }
        }

        #endregion

        private RelativePositionPanel _connectorsRelativePositionPanel;
        public RelativePositionPanel ConnectorsRelativePositionPanel
        {
            get
            {
                if (_connectorsRelativePositionPanel != null)
                    return _connectorsRelativePositionPanel;
                else
                {
                    this.ApplyTemplate();
                    Control connectorDecorator = this.Template.FindName("PART_ConnectorDecorator", this) as Control;
                    connectorDecorator.ApplyTemplate();
                    _connectorsRelativePositionPanel = VisualTreeHelper.GetChild(connectorDecorator, 0) as RelativePositionPanel;
                    return _connectorsRelativePositionPanel;
                }
            }
        }

        #region IsDragConnectionOver

        // while drag connection procedure is ongoing and the mouse moves over 
        // this item this value is true; if true the ConnectorDecorator is triggered
        // to be visible, see template
        public bool IsDragConnectionOver
        {
            get { return (bool)GetValue(IsDragConnectionOverProperty); }
            set { SetValue(IsDragConnectionOverProperty, value); }
        }
        public static readonly DependencyProperty IsDragConnectionOverProperty =
            DependencyProperty.Register("IsDragConnectionOver",
                                         typeof(bool),
                                         typeof(DesignerItem),
                                         new FrameworkPropertyMetadata(false));

        #endregion

        #region Constructors

        static DesignerItem()
        {
            // set the key to reference the style for this control
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerItem), new FrameworkPropertyMetadata(typeof(DesignerItem)));
        }
        #endregion

        private Point _copyOperationMousePosition;

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            DesignerCanvas designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;

            // update selection
            if (designer != null)
            {
                if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                {
                    _copyOperationMousePosition = Mouse.GetPosition(this);
                    if (this.IsSelected)
                    {
                        designer.SelectionService.RemoveFromSelection(this);
                    }
                    else
                    {
                        designer.SelectionService.AddToSelection(this);
                    }
                }
                else if (!this.IsSelected)
                {
                    designer.SelectionService.SelectItem(this);
                }
                Focus();
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                _copyOperationMousePosition = Mouse.GetPosition(this);
            }

            e.Handled = false; //"= false" to be able to drag an object later
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Point _actualMousePosition = Mouse.GetPosition(this);
                if (Point.Subtract(_copyOperationMousePosition, _actualMousePosition).Length > 10)
                {
                    //copying should occur when dragged
                    DesignerCanvas designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;

                    var selectedItems = from item in designer.SelectionService.CurrentSelection.OfType<DesignerItem>()
                                        select item;
                    List<ElementVM> selectedElementsVM = new List<ElementVM>();
                    foreach (DesignerItem item in selectedItems)
                    {
                        if (item.DataContext is ElementVM elementVM)
                            selectedElementsVM.Add(elementVM);
                    }

                    var selectedConnections = from connection in designer.SelectionService.CurrentSelection.OfType<Connection>() select connection;
                    List<ConnectionVM> selectedConnectionsVM = new List<ConnectionVM>();
                    foreach (Connection connection in selectedConnections)
                    {
                        if (connection.DataContext is ConnectionVM connectionVM)
                            selectedConnectionsVM.Add(connectionVM);
                    }

                    string returnedString = (designer.DataContext as MainVM).WriteElementsAndConnectionsToString(selectedElementsVM, selectedConnectionsVM);
                    DragString dragString = new DragString();
                    dragString.DraggedString = returnedString;
                    dragString.DragPointStart = Mouse.GetPosition(designer);
                    DragDrop.DoDragDrop(this, dragString, DragDropEffects.Copy);
                }
            }
        }

        public override void OnApplyTemplate()
        {
            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CanvasWidth = ActualWidth;
        }
    }
}

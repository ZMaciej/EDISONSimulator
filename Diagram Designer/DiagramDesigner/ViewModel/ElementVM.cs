using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using DiagramDesigner.BlockTypes;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel.ViewModelBases;

namespace DiagramDesigner.ViewModel
{
    public class ElementVM : NotifyPropertyChangedBase
    {
        public ElementVM() : this(new Element())
        {
        }
        public ElementVM(Element element)
        {
            Element = element;
            _actualRotation = element.Rotation;
        }

        public void Detach()
        {
            _element.PropertyChanged -= Element_PropertyChanged;
            _element.Connectors.CollectionChanged -= OnConnectorModelsCollectionChanged;
        }

        [Browsable(false)]
        public ObservableCollection<ConnectorModel> ConnectorModels
        {
            get { return Element.Connectors; }
        }

        private void OnConnectorModelsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ConnectorModel newConnectorModel in e.NewItems)
                {
                    ConnectorVM NewConnnector = new ConnectorVM(this, newConnectorModel);
                    if (AddConnector.CanExecute(NewConnnector)) AddConnector.Execute(NewConnnector);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                throw new NotImplementedException("Removing Connector from model layer is not implemented");
            }

            UpdateConnectorsGlobalPosition();
        }

        [Browsable(false)]
        public ObservableCollection<ConnectorVM> ConnectorsVM { get; } = new ObservableCollection<ConnectorVM>();

        public delegate void MyDelegate(ElementVM elementVM);

        public MyDelegate OnMove;

        #region Element Property

        private Element _element;
        [Browsable(false)]
        public Element Element
        {
            get => _element;
            set
            {
                if (_element != null)
                {
                    _element.PropertyChanged -= Element_PropertyChanged;
                    _element.Connectors.CollectionChanged -= OnConnectorModelsCollectionChanged;
                    ConnectorsVM.Clear();
                }
                _element = value ?? throw new Exception("Model of ElementVM can't be null");
                _element.PropertyChanged += Element_PropertyChanged;
                ConnectorModels.CollectionChanged += OnConnectorModelsCollectionChanged;
                ActivateConnectors();
                //Element_PropertyChanged(Element, new PropertyChangedEventArgs(nameof(Rotation))); // causes error in pathFinder when rotation == 2
            }
        }
        public void ActivateConnectors()
        {
            if (ConnectorModels.Count > 0)
                OnConnectorModelsCollectionChanged(ConnectorModels, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ConnectorModels));
        }
        public void Element_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Position))
            {
                OnPropertyChanged(nameof(Top));
                OnPropertyChanged(nameof(Left));
                OnPropertyChanged(nameof(LeftOnCanvas));
                OnMoveRotateResize();
                return;
            }

            if (e.PropertyName == nameof(Size))
            {
                OnPropertyChanged(nameof(Width));
                OnPropertyChanged(nameof(LeftOnCanvas)); //when width changed Left position should also be updated
                OnPropertyChanged(nameof(Height));
                OnMoveRotateResize();
                return;
            }

            if (e.PropertyName == nameof(Rotation))
            {
                for (int i = 0; i < (4 + (Element.Rotation - _actualRotation)) % 4; i++)
                {
                    RotateOnce();
                    OnPropertyChanged(nameof(Rotation));
                }

                _actualRotation = Element.Rotation;
                OnMoveRotateResize();
                return;
            }

            //if property with the same name as changed property in Model layer exist in VM
            if(this.GetType().GetProperty(e.PropertyName) != null)
                //invoke propertyChanged on this property in VM
                OnPropertyChanged(e.PropertyName);
        }

        void OnMoveRotateResize()
        {
            OnMove.Invoke(this);
            UpdateConnectorsGlobalPosition();
        }

        void UpdateConnectorsGlobalPosition()
        {
            foreach (ConnectorVM connectorVm in ConnectorsVM)
            {
                double xConnectorPosition = Position.X + Size.Width * connectorVm.Position.X; //connector position is position relative to element
                double yConnectorPosition = Position.Y + Size.Height * connectorVm.Position.Y;
                connectorVm.GlobalPosition = new Point(xConnectorPosition, yConnectorPosition);
            }
        }

        #endregion

        #region ID Property

        [Browsable(false)]
        public Guid ID
        {
            get => Element.ID;
            set => Element.ID = value;
        }

        #endregion

        #region Name Property
        public string Name
        {
            get => Element.Name;
            set => Element.Name = value;
        }
        #endregion

        #region Position Property
        [Browsable(false)]
        public Point Position
        {
            get => Element.Position;
            set => Element.Position = value;
        }
        #endregion

        #region Size Property
        //it is created to have a one object in propertyChangedCommand

        [Browsable(false)]
        public Size Size
        {
            get => Element.Size;
            set => Element.Size = value;
        }

        #endregion

        #region Type Property

        [Browsable(false)]
        public ElementType Type
        {
            get => Element.Type;
            set => Element.Type = value;
        }
        #endregion

        #region Top Property

        [Browsable(false)]
        public double Top
        {
            get => Position.Y;
            set => Position = new Point(Position.X, value);
        }
        #endregion

        #region Left Property
        // _left stores value of Left position of a Block

        [Browsable(false)]
        public double Left
        {
            get => Position.X;
            set => Position = new Point(value, Position.Y);
        }
        #endregion

        #region LeftOnCanvas Property

        [Browsable(false)]
        public double LeftOnCanvas
        {
            get => Position.X - (_widthOnCanvas - Width) / 2;
            set => Position = new Point(value + (_widthOnCanvas - Width) / 2,Position.Y);
        }

        #endregion

        #region Height Property

        [Browsable(false)]
        public double Height
        {
            get => Size.Height;
            set => Size = new Size(Size.Width, value > 0 ? value : 2);
        }
        #endregion

        #region Width Property

        [Browsable(false)]
        public double Width
        {
            get => Size.Width;
            set => Size = new Size(value > 0 ? value : 2, Size.Height);
        }
        #endregion

        #region WidthOnCanvas Property
        //this property is only a feedback of how wide DesignerItem is
        private double _widthOnCanvas;

        [Browsable(false)]
        public double WidthOnCanvas
        {
            set
            {
                _widthOnCanvas = value;
                OnPropertyChanged(nameof(WidthOnCanvas));
                OnPropertyChanged(nameof(LeftOnCanvas)); //when width changed Left position should also be updated
            }
        }

        #endregion

        #region Rotation Property
        //rotation have 0,1,2,3 values, and it represents clockwise rotation * 90deg
        private int _actualRotation;
        public int Rotation
        {
            get => Element.Rotation;
            set => Element.Rotation = value;
        }
        #endregion

        #region BoundingRectangle Property

        private BoundingRectangle _boundingRectangle;
        [Browsable(false)]
        public BoundingRectangle BoundingRectangle
        {
            get
            {
                if (_boundingRectangle == null)
                    _boundingRectangle = new BoundingRectangle();
                _boundingRectangle.TopLeftCorner = new Point(Left, Top);
                _boundingRectangle.Size = new Size(Width, Height);
                return _boundingRectangle;
            }
        }

        #endregion

        #region AddConnector Command
        private ICommand _addConnector;
        [Browsable(false)]
        public ICommand AddConnector => _addConnector ?? (_addConnector = new RelayCommand(AddConnectorExecute, AddConnectorCanExecute));


        bool AddConnectorCanExecute(object newConnector)
        {
            if (newConnector is ConnectorVM connector)
                if (ConnectorsVM.Contains(connector))
                    return false;
                else
                    return true;
            else
                return false;
        }
        void AddConnectorExecute(object newConnector)
        {
            ConnectorsVM.Add(newConnector as ConnectorVM);
        }
        #endregion

        protected virtual void RotateOnce()
        {
            //rotate by clockwise 90deg
            Top = Top - (Width - Height) / 2;
            Left = Left + (Width - Height) / 2;

            double Temp = Width;
            Width = Height;
            Height = Temp;

            foreach (ConnectorVM connectorVm in ConnectorsVM)
            {
                if (connectorVm.Orientation != ConnectorOrientation.None)
                    if (connectorVm.Orientation > (ConnectorOrientation)3)
                        connectorVm.Orientation = connectorVm.Orientation - 3;
                    else
                        connectorVm.Orientation++;

                Point newPosition = new Point();
                newPosition.X = 1 - connectorVm.Position.Y; //rotation of relative position
                newPosition.Y = connectorVm.Position.X;
                connectorVm.Position = newPosition;
            }
        }

        private ICommand _rotate;
        [Browsable(false)]
        public ICommand Rotate => _rotate ?? (_rotate = new RelayCommand(RotateExecute));

        private void RotateExecute(object obj)
        {
            this.Rotation++;
        }
    }
}
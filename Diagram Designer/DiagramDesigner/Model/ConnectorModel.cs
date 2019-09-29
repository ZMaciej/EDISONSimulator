using System;
using System.Collections.ObjectModel;
using System.Windows;
using DiagramDesigner.Annotations;
using DiagramDesigner.ViewModel.ViewModelBases;

namespace DiagramDesigner.Model
{
    public class ConnectorModel : NotifyPropertyChangedBase
    {
        public ConnectorModel([NotNull] Element parent)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            ID = Guid.NewGuid();
        }
        public ObservableCollection<ConnectionModel> ConnectionModels { get; } = new ObservableCollection<ConnectionModel>();

        #region ID Property

        private Guid _id;
        public Guid ID
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        #endregion

        #region Connected Property

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set
            {
                _connected = value;
                OnPropertyChanged(nameof(Connected));
            }
        }

        #endregion

        #region Port Number Property

        private int _number;

        public int Number
        {
            get => _number;
            set
            {
                _number = value;
                OnPropertyChanged(nameof(Number));
            }
        }

        #endregion

        #region Position Property

        private Point _position;
        public Point Position
        {
            get => _position;
            set
            {
                if (value == null)
                    throw new Exception("Position of ConnectorModel can't be null");
                if (value == _position) return;

                _position = value;
                OnPropertyChanged(nameof(Position));
            }
        }

        #endregion

        #region Orientation Property

        private ConnectorOrientation _orientation;
        public ConnectorOrientation Orientation
        {
            get => _orientation;
            set
            {
                if (value != _orientation)
                {
                    _orientation = value;
                    OnPropertyChanged(nameof(Orientation));
                }
            }
        }
        
        #endregion

        #region Parent Property

        private Element _parent;
        public Element Parent
        {
            get => _parent;
            set => _parent = value ?? throw new Exception("Parent of Connector can't be null");
        }

        #endregion
    }
}
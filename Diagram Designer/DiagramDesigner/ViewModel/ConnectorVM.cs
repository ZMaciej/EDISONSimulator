using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using DiagramDesigner.Annotations;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel.ViewModelBases;

namespace DiagramDesigner.ViewModel
{
    public class ConnectorVM : NotifyPropertyChangedBase
    {
        #region Constructors

        public ConnectorVM([NotNull] ElementVM parent,[NotNull] ConnectorModel connectorModel)
        {
            this.Parent = parent;
            ConnectorModel = connectorModel;
        }

        #endregion

        public ObservableCollection<ConnectionVM> ConnectionsVM { get; } = new ObservableCollection<ConnectionVM>();

        public ElementVM Parent { get; }

        public void Detach()
        {
            _connectorModel.PropertyChanged -= ConnectorModel_PropertyChanged;
        }

        #region ConnectorModel Property

        private ConnectorModel _connectorModel;
        public ConnectorModel ConnectorModel
        {
            get => _connectorModel;
            set
            {
                if (_connectorModel != null)
                    _connectorModel.PropertyChanged -= ConnectorModel_PropertyChanged;
                _connectorModel = value ?? throw new Exception("Model of ConnectionVM can't be null");
                _connectorModel.PropertyChanged += ConnectorModel_PropertyChanged;
            }
        }

        private void ConnectorModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if property with the same name as changed property in Model layer exist in VM
            if (this.GetType().GetProperty(e.PropertyName) != null)
                //invoke propertyChanged on this property in VM
                OnPropertyChanged(e.PropertyName);

            OnPropertyChanged(nameof(Number));
        }

        #endregion

        #region ID Property

        public Guid ID
        {
            get => ConnectorModel.ID;
            set => ConnectorModel.ID = value;
        }

        #endregion

        #region Connected Property

        public bool Connected
        {
            get => ConnectorModel.Connected;
            set => ConnectorModel.Connected = value;
        }

        #endregion

        #region Number Property

        public int Number => (ConnectorModel.Number + 1);

        #endregion

        #region Position Property

        public Point Position //relative position
        {
            get => ConnectorModel.Position;
            set => ConnectorModel.Position = value;
        }

        public Point GlobalPosition { get; set; } //useful for autoConnect

        #endregion

        #region Orientation Property

        public ConnectorOrientation Orientation
        {
            get => ConnectorModel.Orientation;
            set => ConnectorModel.Orientation = value;
        }

        #endregion
    }
}
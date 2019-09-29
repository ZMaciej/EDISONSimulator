using System;
using System.ComponentModel;
using DiagramDesigner.Annotations;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel.ViewModelBases;

namespace DiagramDesigner.ViewModel
{
    public class ConnectionVM : NotifyPropertyChangedBase
    {
        private ConnectionModel _connectionModel;
        public ConnectionModel ConnectionModel
        {
            get => _connectionModel;
            set
            {
                if (_connectionModel != null)
                    _connectionModel.PropertyChanged -= ConnectionModel_PropertyChanged;
                _connectionModel = value ?? throw new Exception("Model of ConnectionVM can't be null");
                _connectionModel.PropertyChanged += ConnectionModel_PropertyChanged;
            }
        }
        public void Detach()
        {
            _connectionModel.PropertyChanged -= ConnectionModel_PropertyChanged;
        }

        private void ConnectionModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if property with the same name as changed property in Model layer exist in VM
            if (this.GetType().GetProperty(e.PropertyName) != null)
            {
                if (e.PropertyName == nameof(ConnectionModel.SinkConnector))
                {
                    SinkVM = MainVm.GetConnectorVMByGuid(ConnectionModel.SinkConnector.ID);
                    return;
                }

                if (e.PropertyName == nameof(ConnectionModel.SourceConnector))
                {
                    SourceVM = MainVm.GetConnectorVMByGuid(ConnectionModel.SourceConnector.ID);
                    return;
                }
                //invoke propertyChanged on this property in VM
                OnPropertyChanged(e.PropertyName);
            }
        }

        public MainVM MainVm { get; set; } //it is necessary to find a connector by ID when it is changed

        #region ID Property

        public Guid ID
        {
            get => ConnectionModel.ID;
            set => ConnectionModel.ID = value;
        }

        #endregion

        #region SinkVM Property
        private ConnectorVM _sinkVM;
        public ConnectorVM SinkVM
        {
            get => _sinkVM;
            set
            {
                _sinkVM = value ?? throw new ArgumentNullException(nameof(SinkVM));
                OnPropertyChanged(nameof(SinkVM));
            }
        }
        #endregion

        #region SourceVM Property
        private ConnectorVM _sourceVM;
        public ConnectorVM SourceVM
        {
            get => _sourceVM;
            set
            {
                _sourceVM = value ?? throw new ArgumentNullException(nameof(SourceVM));
                OnPropertyChanged(nameof(SourceVM));
            }
        }
        #endregion

        #region Constructor

        public ConnectionVM([NotNull] ConnectorVM sinkVM, [NotNull] ConnectorVM sourceVM, [NotNull] MainVM mainVM)
        {
            MainVm = mainVM ?? throw new ArgumentNullException(nameof(mainVM));
            SinkVM = sinkVM ?? throw new ArgumentNullException(nameof(sinkVM));
            SourceVM = sourceVM ?? throw new ArgumentNullException(nameof(sourceVM));
        }

        #endregion

        public bool Contains(ConnectorVM connectorVM)
        {
            if (SinkVM == connectorVM || SourceVM == connectorVM)
            {
                return true;
            }
            return false;
        }
    }
}
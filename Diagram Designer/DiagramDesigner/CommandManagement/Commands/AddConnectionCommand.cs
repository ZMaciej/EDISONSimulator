using System;
using DiagramDesigner.Annotations;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;
using DiagramDesigner.ViewModel.ViewModelBases;

namespace DiagramDesigner.CommandManagement.Commands
{
    public class AddConnectionCommand : Command
    {
        private readonly MainModel _mainModel;
        private readonly ConnectionModel _connectionToAdd;

        public AddConnectionCommand([NotNull] ConnectionModel connectionToAdd, [NotNull] MainModel mainModel)
        {
            _connectionToAdd = connectionToAdd ?? throw new ArgumentNullException(nameof(connectionToAdd));
            _mainModel = mainModel ?? throw new ArgumentNullException(nameof(mainModel));
        }

        public void Execute()
        {
            _mainModel.ConnectionModels.Add(_connectionToAdd);
            if (!_connectionToAdd.SinkConnector.ConnectionModels.Contains(_connectionToAdd))
                _connectionToAdd.SinkConnector.ConnectionModels.Add(_connectionToAdd);
            if (!_connectionToAdd.SourceConnector.ConnectionModels.Contains(_connectionToAdd))
                _connectionToAdd.SourceConnector.ConnectionModels.Add(_connectionToAdd);
            _connectionToAdd.SinkConnector.Connected = true;
            _connectionToAdd.SourceConnector.Connected = true;
        }

        public void Undo()
        {
            RemoveConnectionCommand removeConnectionCommand = new RemoveConnectionCommand(_connectionToAdd, _mainModel);
            removeConnectionCommand.Execute();
        }
    }
}
using System;
using System.Linq;
using DiagramDesigner.Annotations;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;
using DiagramDesigner.ViewModel.ViewModelBases;

namespace DiagramDesigner.CommandManagement.Commands
{
    public class RemoveConnectionCommand : Command
    {
        private readonly MainModel _mainModel;
        private readonly ConnectionModel _connectionToRemove;

        public RemoveConnectionCommand([NotNull] ConnectionModel ConnectionToRemove,[NotNull] MainModel mainModel)
        {
            _connectionToRemove = ConnectionToRemove ?? throw new ArgumentNullException(nameof(ConnectionToRemove));
            _mainModel = mainModel ?? throw new ArgumentNullException(nameof(mainModel));
        }

        public void Execute()
        {
            if (_mainModel.ConnectionModels.Contains(_connectionToRemove))
            {
                _mainModel.ConnectionModels.Remove(_connectionToRemove);
                _connectionToRemove.SinkConnector.ConnectionModels.Remove(_connectionToRemove);
                _connectionToRemove.SourceConnector.ConnectionModels.Remove(_connectionToRemove);
                _connectionToRemove.SinkConnector.Connected = _connectionToRemove.SinkConnector.ConnectionModels.Any();
                _connectionToRemove.SourceConnector.Connected = _connectionToRemove.SourceConnector.ConnectionModels.Any();
            }
        }

        public void Undo()
        {
            AddConnectionCommand addConnectionCommand = new AddConnectionCommand(_connectionToRemove, _mainModel);
            addConnectionCommand.Execute();
        }
    }
}
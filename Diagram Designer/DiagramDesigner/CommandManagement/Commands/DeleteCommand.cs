using System.Collections.Generic;
using System.Linq;
using DiagramDesigner.Annotations;
using DiagramDesigner.Model;

namespace DiagramDesigner.CommandManagement.Commands
{
    public class DeleteCommand : Command
    {
        private IList<Command> _elementsAndConnectionsRemoveCommands = new List<Command>();
        private readonly MainModel _mainModel;

        public DeleteCommand([NotNull] IEnumerable<Element> elementsToDelete, [NotNull] IEnumerable<ConnectionModel> connectionsToDelete, [NotNull] MainModel mainModel)
        {
            _mainModel = mainModel;
            foreach (Element element in elementsToDelete)
            {
                foreach (ConnectorModel connectorModel in element.Connectors)
                {
                    foreach (ConnectionModel connectionModel in connectorModel.ConnectionModels)
                    {
                        if(!connectionsToDelete.Contains(connectionModel))
                            _elementsAndConnectionsRemoveCommands.Add(new RemoveConnectionCommand(connectionModel, _mainModel));
                    }
                }
            }
            foreach (ConnectionModel connectionModel in connectionsToDelete)
            {
                _elementsAndConnectionsRemoveCommands.Add(new RemoveConnectionCommand(connectionModel, _mainModel));
            }
            foreach (Element element in elementsToDelete)
            {
                _elementsAndConnectionsRemoveCommands.Add(new RemoveElementCommand(element, _mainModel));
            }
        }
        public void Execute()
        {
            foreach (Command removeCommand in _elementsAndConnectionsRemoveCommands)
            {
                removeCommand.Execute();
            }
        }

        public void Undo()
        {
            foreach (Command removeCommand in _elementsAndConnectionsRemoveCommands.Reverse())
            {
                removeCommand.Undo();
            }
        }
    }
}
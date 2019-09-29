using System.Collections.Generic;
using System.Linq;
using DiagramDesigner.Annotations;
using DiagramDesigner.Model;

namespace DiagramDesigner.CommandManagement.Commands
{
    public class PasteCommand : Command
    {
        private IList<Command> _elementsAndConnectionsAddCommands = new List<Command>();
        private readonly MainModel _mainModel;

        public PasteCommand([NotNull] IEnumerable<Element> elementsToPaste, [NotNull] IEnumerable<ConnectionModel> connectionsToPaste, [NotNull] MainModel mainModel)
        {
            _mainModel = mainModel;
            foreach (Element element in elementsToPaste)
            {
                _elementsAndConnectionsAddCommands.Add(new AddElementCommand(element, _mainModel));
            }
            foreach (ConnectionModel connectionModel in connectionsToPaste)
            {
                _elementsAndConnectionsAddCommands.Add(new AddConnectionCommand(connectionModel, _mainModel));
            }
        }
        public void Execute()
        {
            foreach (Command addCommand in _elementsAndConnectionsAddCommands)
            {
                addCommand.Execute();
            }
        }

        public void Undo()
        {
            foreach (Command addCommand in _elementsAndConnectionsAddCommands.Reverse())
            {
                addCommand.Undo();
            }
        }
    }
}
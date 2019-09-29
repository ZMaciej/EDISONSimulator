using System;
using DiagramDesigner.Annotations;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner.CommandManagement.Commands
{
    public class RemoveElementCommand : Command
    {
        private readonly MainModel _mainModel;
        private readonly Element _elementToRemove;

        public RemoveElementCommand([NotNull] Element elementToRemove,[NotNull] MainModel mainModel)
        {
            _elementToRemove = elementToRemove ?? throw new ArgumentNullException(nameof(elementToRemove));
            _mainModel = mainModel ?? throw new ArgumentNullException(nameof(mainModel));
        }
        public void Execute()
        {
            if(_mainModel.Elements.Contains(_elementToRemove))
            {
                //removing connections of a Element
                foreach (ConnectorModel connectorModel in _elementToRemove.Connectors)
                    for (int i = connectorModel.ConnectionModels.Count - 1; i >= 0; i--)
                        if (connectorModel.ConnectionModels[i] is ConnectionModel connectionModel)
                        {
                            RemoveConnectionCommand removeConnectionCommand = 
                                new RemoveConnectionCommand(connectionModel, _mainModel);
                            removeConnectionCommand.Execute();
                            _mainModel.MyCommandManager.AddToList(removeConnectionCommand);
                        }

                _elementToRemove.MainModelCommandManager = null;
                _mainModel.Elements.Remove(_elementToRemove);
            }
        }

        public void Undo()
        {
            AddElementCommand addElementCommand = new AddElementCommand(_elementToRemove, _mainModel);
            addElementCommand.Execute();
        }
    }
}
using System;
using DiagramDesigner.Annotations;
using DiagramDesigner.Model;

namespace DiagramDesigner.CommandManagement.Commands
{
    public class AddElementCommand : Command
    {
        private readonly MainModel _mainModel;
        private readonly Element _elementToAdd;

        public AddElementCommand([NotNull] Element elementToAdd, [NotNull] MainModel mainModel)
        {
            _elementToAdd = elementToAdd ?? throw new ArgumentNullException(nameof(elementToAdd));
            _mainModel = mainModel ?? throw new ArgumentNullException(nameof(mainModel));
        }
        public void Execute()
        {
            _mainModel.Elements.Add(_elementToAdd);
            _elementToAdd.MainModelCommandManager = _mainModel.MyCommandManager;
        }

        public void Undo()
        {
            RemoveElementCommand removeElementCommand = new RemoveElementCommand(_elementToAdd, _mainModel);
            removeElementCommand.Execute();
        }
    }
}
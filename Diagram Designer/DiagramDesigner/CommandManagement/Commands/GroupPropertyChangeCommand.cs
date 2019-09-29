using System;
using System.Collections.Generic;
using System.Windows;
using DiagramDesigner.Annotations;
using DiagramDesigner.Model;

namespace DiagramDesigner.CommandManagement.Commands
{
    public class GroupPropertyChangeCommand : Command
    {
        private readonly List<PropertyChangedCommand> _propertyChangeCommands;
        public GroupPropertyChangeCommand([NotNull]List<PropertyChangedCommand> propertyChangeCommands)
        {
            if (propertyChangeCommands != null)
                _propertyChangeCommands = propertyChangeCommands;
            else
                throw new NullReferenceException(nameof(propertyChangeCommands));

            //deleting elements where property change command is created but property doesn't changed
            for (int i = propertyChangeCommands.Count - 1; i >= 0; i--)
            {
                if (propertyChangeCommands[i].PropertyNewValue.Equals(propertyChangeCommands[i].PropertyOldValue))
                {
                    propertyChangeCommands.Remove(propertyChangeCommands[i]);
                }
            }
        }

        public int NumberOfChangedProperties => _propertyChangeCommands.Count;

        public void Execute()
        {
            foreach (PropertyChangedCommand propertyChangedCommand in _propertyChangeCommands)
            {
                propertyChangedCommand.Execute();
            }
        }

        public void Undo()
        {
            foreach (PropertyChangedCommand propertyChangedCommand in _propertyChangeCommands)
            {
                propertyChangedCommand.Undo();
            }
        }
    }
}
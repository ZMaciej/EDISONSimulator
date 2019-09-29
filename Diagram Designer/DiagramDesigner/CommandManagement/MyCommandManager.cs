using System;
using System.Collections.Generic;
using DiagramDesigner.Annotations;
using DiagramDesigner.CommandManagement.Commands;

namespace DiagramDesigner.CommandManagement
{
    public class MyCommandManager
    {
        private List<Command> CommandHistory = new List<Command>();
        private bool ICausedPropertyChange;
        private int _counter;

        public MyCommandManager()
        {
            _counter = 0;
        }

        public void AddToList([NotNull] Command commandToAdd)
        {
            if(commandToAdd == null)
                throw new ArgumentNullException(nameof(commandToAdd));

            if (commandToAdd is GroupPropertyChangeCommand groupPropertyChangeCommand)
            {
                if(groupPropertyChangeCommand.NumberOfChangedProperties < 1)
                    return;
            }

            if (commandToAdd is PropertyChangedCommand propertyChangedCommand)
            {
                //avoid adding element when MyCommandManager is the reason why propertyChangedCommand is created
                if (ICausedPropertyChange)
                {
                    ICausedPropertyChange = false;
                    return;
                }

                //avoid adding element when property change command is created but property doesn't changed
                if (propertyChangedCommand.PropertyNewValue.Equals(propertyChangedCommand.PropertyOldValue))
                {
                    return;
                }

                //stack propertyChanges with previous changes in property
                if (_counter > 0 && CommandHistory[_counter - 1] is PropertyChangedCommand lastChangedCommand && propertyChangedCommand.IsTheSameAs(lastChangedCommand))
                {
                    if (lastChangedCommand.PropertyOldValue.Equals(propertyChangedCommand.PropertyNewValue))
                    {
                        _counter--;
                        CommandHistory.RemoveAt(_counter);
                        return;
                    }
                    else
                    {
                        lastChangedCommand.PropertyNewValue = propertyChangedCommand.PropertyNewValue;
                        return;
                    }
                }
            }

            //adding element to CommandHistory list
            if (_counter < CommandHistory.Count)
                ClearAllAbove();
            CommandHistory.Add(commandToAdd);
            _counter++;
        }

        private void ClearAllAbove()
        {
            CommandHistory.RemoveRange(_counter, CommandHistory.Count - _counter);
        }

        public Boolean CanUndo => (_counter > 0);

        public void Undo()
        {
            if (_counter > 0)
            {
                _counter--;
                if (CommandHistory[_counter] is PropertyChangedCommand propertyChangedCommand)
                {
                    //it prevents from adding a new PropertyChangedCommand to the list: when property changed by Undo it adds propertyChangedCommand to list,
                    //but when ICausedPropertyChange is true MyCommandManager do not react to Command add
                    ICausedPropertyChange = propertyChangedCommand.CanCausePropertyChange;
                }
                CommandHistory[_counter].Undo();
            }
        }

        public Boolean CanRedo => (_counter < CommandHistory.Count);

        public void Redo()
        {
            if (_counter < CommandHistory.Count)
            {
                if (CommandHistory[_counter] is PropertyChangedCommand propertyChangedCommand)
                {
                    //same activity as in Undo()
                    ICausedPropertyChange = propertyChangedCommand.CanCausePropertyChange;
                }
                CommandHistory[_counter].Execute();
                _counter++;
            }
        }
    }

    public interface Command
    {
        void Execute();
        void Undo();
    }
}
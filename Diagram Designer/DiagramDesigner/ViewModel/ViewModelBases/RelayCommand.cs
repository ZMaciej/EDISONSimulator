using System;
using System.CodeDom;
using System.Windows.Input;

namespace DiagramDesigner.ViewModel.ViewModelBases
{
    public class RelayCommand : ICommand
    {
        private Action<object> _execute;
        private Predicate<object> _canExecute;

        public RelayCommand(Action<object> Execute, Predicate<object> CanExecute)
        {
            _execute = Execute ?? throw new ArgumentNullException(nameof(Execute));
            _canExecute = CanExecute;
        }

        public RelayCommand(Action<object> Execute)
            : this(Execute, null)
        {
        }

        public void Run(object parameter)
        {
            if(this.CanExecute(parameter))
                this.Execute(parameter);
        }

        #region ICommand Members
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        #endregion
    }
}
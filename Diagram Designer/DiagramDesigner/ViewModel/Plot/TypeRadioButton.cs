using System.Windows.Input;
using DiagramDesigner.ViewModel.ViewModelBases;

namespace DiagramDesigner.ViewModel
{
    public class TypeRadioButton
    {
        public string Name { get; set; }
        public DataPointListType Type { get; set; }
        public bool IsChecked { get; set; }

        public delegate void Update(TypeRadioButton typeRadioButton);
        private Update update;

        public TypeRadioButton(DataPointListType type, string name, Update update, bool isChecked)
        {
            Type = type;
            Name = name;
            this.update = update;
            IsChecked = isChecked;
        }
        private ICommand _changed;
        public ICommand Changed => _changed ?? (_changed = new RelayCommand(RadioButtonChanged));

        private void RadioButtonChanged(object parameter)
        {
            if (parameter is bool isSelected)
            {
                if (isSelected)
                    update.Invoke(this);
            }
        }
    }
}
using System;
using System.Reflection;
using DiagramDesigner.Annotations;

namespace DiagramDesigner.CommandManagement.Commands
{
    public class PropertyChangedCommand : Command
    {
        public object PropertyOldValue { get; set; }
        public object PropertyNewValue { get; set; }

        //when we add PropertyChangedCommand like this Property{set{Add(new PropertyChangedCommand)}} undoing changes can cause creation of new PropertyChangedCommand
        //but when CanCausePropertyChange is true the undo method prevents adding a new PropertyChangedCommand to the list
        public bool CanCausePropertyChange { get; }
        private readonly string _propertyName;
        private readonly object _source;
        public object Source => _source;

        public PropertyChangedCommand(object propertyOldValue, object propertyNewValue, [NotNull] string propertyName, [NotNull] object source, bool canCausePropertyChange)
        {
            if (propertyName == String.Empty)
                throw new ArgumentNullException(nameof(propertyName));
            _propertyName = propertyName;

            _source = source ?? throw new ArgumentNullException(nameof(source));

            PropertyOldValue = propertyOldValue;
            PropertyNewValue = propertyNewValue;
            CanCausePropertyChange = canCausePropertyChange;
        }

        public PropertyChangedCommand(object propertyOldValue, object propertyNewValue, [NotNull] string propertyName, [NotNull] object source) 
            :this(propertyOldValue, propertyNewValue, propertyName, source, true) //by default property change can cause infinite loop
        {
        }

        public void Execute()
        {
            if (PropertyOldValue != PropertyNewValue)
            {
                SetPropValue(_source, _propertyName,PropertyNewValue);
            }
        }

        public void Undo()
        {
            if (PropertyOldValue != PropertyNewValue)
            {
                SetPropValue(_source, _propertyName, PropertyOldValue);
            }
        }

        private static void SetPropValue(object src, string propName, object value)
        {
            PropertyInfo prop = src.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                if (prop.PropertyType == value.GetType())
                    prop.SetValue(src, value, null);
            }
        }

        public bool IsTheSameAs([NotNull] PropertyChangedCommand propertyChangedCommand)
        {
            if(propertyChangedCommand == null)
                throw new ArgumentNullException(nameof(propertyChangedCommand));
            return _source == propertyChangedCommand._source && _propertyName == propertyChangedCommand._propertyName;
        }
    }
}
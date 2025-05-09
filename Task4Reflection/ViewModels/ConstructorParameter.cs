using System;
using System.ComponentModel;

namespace Task4Reflection.ViewModels
{
    public class ConstructorParameter : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public Type TargetType { get; set; }
        public string Value { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(object value) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        public object TryGetValue()
        {
            if (TargetType == typeof(string))
            {
                return Value;
            }
            else if (TargetType == typeof(double))
            {
                return Double.Parse(Value);
            }
            return null;
        }
    }
}
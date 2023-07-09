using System.ComponentModel;
using Xamarin.Forms;

namespace MuscleAnatomyAndMotion
{
    public abstract class ViewModelBase : BindableObject, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static MuscleAnatomyAndMotion.App;

namespace MuscleAnatomyAndMotion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MuscleInfoRedirect : ContentView, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;
        private string _Description;
        private MuscleID _Link;
        public string Description
        {
            get => _Description;
            set
            {
                _Description = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
            }
        }
        public MuscleID Link
        {
            get => _Link;
            set
            {
                _Link = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsButtonEnabled)));
            }
        }
        public bool IsButtonEnabled { get => Link != null; }
        public MuscleInfoRedirect()
        {
            BindingContext = this;
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MuscleInfoRedirectPage(Link));
        }
    }
}
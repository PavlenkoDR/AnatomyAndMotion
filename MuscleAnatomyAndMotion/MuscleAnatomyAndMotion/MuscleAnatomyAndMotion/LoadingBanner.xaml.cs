using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingBanner : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;
        private string _Progress;
        public string Progress 
        { 
            get => _Progress; 
            set
            {
                _Progress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
            } 
        }
        public LoadingBanner()
        {
            InitializeComponent();
            BindingContext = this;
        }
        protected override bool OnBackButtonPressed() => true;
    }
}
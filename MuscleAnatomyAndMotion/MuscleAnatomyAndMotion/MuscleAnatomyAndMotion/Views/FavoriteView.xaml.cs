using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FavoriteView : ContentView, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;
        public ImageSource CurrentImageSource { get; set; }
        private ImageSource unselectedImageSource;
        private ImageSource selectedImageSource;

        public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
            nameof(IsSelected),
            typeof(bool),
            typeof(FavoriteView),
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: (bindableObject, oldValue, newValue) =>
            {
                (bindableObject as FavoriteView).updateImage();
            }
        );

        public bool IsSelected
        {
            get
            {
                return (bool)GetValue(IsSelectedProperty);
            }
            set
            {
                SetValue(IsSelectedProperty, value);
                updateImage();
            }
        }

        private void updateImage()
        {
            Device.BeginInvokeOnMainThread(() => {
                CurrentImageSource = IsSelected ? selectedImageSource : unselectedImageSource;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentImageSource"));
            });
        }

        public FavoriteView()
        {
            unselectedImageSource = ImageSource.FromResource("MuscleAnatomyAndMotion.Assets.heart.png");
            selectedImageSource = ImageSource.FromResource("MuscleAnatomyAndMotion.Assets.heartFull.png");
            InitializeComponent();
            Content.BindingContext = this;
            IsSelected = false;
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            IsSelected = !IsSelected;
        }
    }
}
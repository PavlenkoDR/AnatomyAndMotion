using MuscleAnatomyAndMotion.Controllers;
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
    public class ExerciseViewModel : Exercise
    {
        public ExerciseID id;
        public ExerciseViewModel SetID(ExerciseID id)
        {
            this.id = id;
            return this;
        }
        private bool _isFiltered = true;
        public bool isFiltered
        {
            get => _isFiltered;
            set
            {
                _isFiltered = value;
                RaisePropertyChanged("isSelected");
            }
        }
        private bool _isSearched = true;
        public bool isSearched
        {
            get => _isSearched;
            set
            {
                _isSearched = value;
                RaisePropertyChanged("isSelected");
            }
        }
        public bool isSelected { get => isFiltered && isSearched; }
        public bool isFavorite
        {
            get => LocalFilesController.favoriteData.exerciseIDs.Contains(id);
            set
            {
                if (value)
                {
                    if (!LocalFilesController.favoriteData.exerciseIDs.Contains(id))
                    {
                        LocalFilesController.favoriteData.exerciseIDs.Add(id);
                    }
                }
                else
                {
                    LocalFilesController.favoriteData.exerciseIDs.Remove(id);
                }
                RaisePropertyChanged("isFavorite");
            }
        }
        public string shareText { get => $"{name}\n\n{target_muscle}"; set { } }
        public List<string> shareUrls { get => new List<string>() { thumbnail_image_url }; set { } }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExerciseView : ContentView, INotifyPropertyChanged
    {
        public ExerciseView()
        {
            InitializeComponent();
        }
        
        public new event PropertyChangedEventHandler PropertyChanged;
        public static readonly BindableProperty ModelProperty = BindableProperty.Create(
            nameof(Model),
            typeof(ExerciseViewModel),
            typeof(ExerciseView),
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: (bindableObject, oldValue, newValue) =>
            {
                (bindableObject as ExerciseView).updateModel();
            }
        );

        public ExerciseViewModel Model
        {
            get
            {
                return (ExerciseViewModel)GetValue(ModelProperty);
            }
            set
            {
                SetValue(ModelProperty, value);
                updateModel();
            }
        }
        private void updateModel()
        {
            Content.BindingContext = Model;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            Model = BindingContext as ExerciseViewModel ?? Model;
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            ((Application.Current.MainPage as MainFlyoutPage).Detail as NavigationPage).PushAsync(new ExercisePage(Model.id));
        }
    }
}
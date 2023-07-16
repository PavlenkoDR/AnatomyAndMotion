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
            get => FavoriteController.favoriteData.exerciseIDs.Contains(id);
            set
            {
                Task.Run(() => {
                    Device.BeginInvokeOnMainThread(() => {
                        if (value)
                        {
                            if (!FavoriteController.favoriteData.exerciseIDs.Contains(id))
                            {
                                FavoriteController.favoriteData.exerciseIDs.Add(id);
                            }
                        }
                        else
                        {
                            FavoriteController.favoriteData.exerciseIDs.Remove(id);
                        }
                        RaisePropertyChanged("isFavorite");
                    });
                    Task.Yield();
                });
            }
        }
        public bool isDownloaded
        {
            get => WebResourceController.downloadedData.exerciseIDs.Contains(id);
            set
            {
                Task.Run(()=> {
                    Device.BeginInvokeOnMainThread(async () => {
                        if (ResourceController.IsOffline)
                        {
                            return;
                        }
                        if (value)
                        {
                            if (!WebResourceController.downloadedData.exerciseIDs.Contains(id))
                            {
                                var loadingBanner = new LoadingBanner();
                                loadingBanner.Progress = $"Загрузка и сохранение";
                                await Application.Current.MainPage.Navigation.PushModalAsync(loadingBanner);
                                WebResourceController.downloadedData.exerciseIDs.Add(id);
                                await Application.Current.MainPage.Navigation.PopModalAsync();
                            }
                        }
                        else
                        {
                            var loadingBanner = new LoadingBanner();
                            loadingBanner.Progress = $"Удаление из сохраненных";
                            await Application.Current.MainPage.Navigation.PushModalAsync(loadingBanner);
                            WebResourceController.downloadedData.exerciseIDs.Remove(id);
                            await Application.Current.MainPage.Navigation.PopModalAsync();
                        }
                        RaisePropertyChanged("isDownloaded");
                    });
                    Task.Yield();
                });
            }
        }
        public string shareText { get => $"{name}\n\n{target_muscle}"; set { } }
        public List<string> shareUrls { get => new List<string>() { thumbnail_image_url }; set { } }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExerciseView : ContentView
    {
        public ExerciseView()
        {
            InitializeComponent();
        }
        
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
                Task.Run(() =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        SetValue(ModelProperty, value);
                        updateModel();
                    });
                    Task.Yield();
                });
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
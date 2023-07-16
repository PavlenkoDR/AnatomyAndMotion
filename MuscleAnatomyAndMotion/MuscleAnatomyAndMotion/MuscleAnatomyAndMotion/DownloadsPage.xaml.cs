using MuscleAnatomyAndMotion.Controllers;
using MuscleAnatomyAndMotion.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DownloadsPage : ContentPage
    {
        public class Variant
        {
            public ExerciseViewModel exerciseViewModel { get; set; }
            public MuscleViewModel muscleViewModel { get; set; }
            public bool IsExercise { get => exerciseViewModel != null; }
            public bool IsMuscle { get => muscleViewModel != null; }
        }
        public DownloadsPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            Device.BeginInvokeOnMainThread(()=> {

                var models = GetExerciseViewModels();
                listView.ItemsSource = models;
            });
            WebResourceController.downloadedData.exerciseIDs.CollectionChanged += UpdateListView;
            WebResourceController.downloadedData.muscleIDs.CollectionChanged += UpdateListView;
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            WebResourceController.downloadedData.exerciseIDs.CollectionChanged -= UpdateListView;
            WebResourceController.downloadedData.muscleIDs.CollectionChanged -= UpdateListView;
            base.OnDisappearing();
        }

        private void UpdateListView(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(()=> {
                listView.ItemsSource = GetExerciseViewModels();
            });
        }

        private List<Utils.Grouping<string, Variant>> GetExerciseViewModels()
        {
            List<Utils.Grouping<string, Variant>> favorites = new List<Utils.Grouping<string, Variant>>();
            {
                var models = WebResourceController.downloadedData.exerciseIDs.Select(y => Utils.As<Exercise, ExerciseViewModel>(MuscleDictionary.exercises[y]).SetID(y)).ToList();
                models.ForEach(x => x.isDownloaded = true);
                if (models.Count() > 0)
                {
                    favorites.Add(new Utils.Grouping<string, Variant>("Упражнения", models.Select(x => new Variant() { exerciseViewModel = x, muscleViewModel = null })));
                }
            }
            {
                var models = WebResourceController.downloadedData.muscleIDs.Select(y => Utils.As<MuscleExtended, MuscleViewModel>(MuscleDictionary.musclesExtended[y])).ToList();
                models.ForEach(x => x.isDownloaded = true);
                if (models.Count() > 0)
                {
                    favorites.Add(new Utils.Grouping<string, Variant>("Мышцы", models.Select(x => new Variant() { exerciseViewModel = null, muscleViewModel = x })));
                }
            }
            return favorites;
        }
    }
}
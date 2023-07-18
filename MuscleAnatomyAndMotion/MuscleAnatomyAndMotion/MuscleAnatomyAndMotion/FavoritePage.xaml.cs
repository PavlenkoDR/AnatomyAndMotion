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
    public partial class FavoritePage : ContentPage
    {
        public class Variant
        {
            public ExerciseViewModel exerciseViewModel { get; set; }
            public MuscleViewModel muscleViewModel { get; set; }
            public bool IsExercise { get => exerciseViewModel != null; }
            public bool IsMuscle { get => muscleViewModel != null; }
        }
        public FavoritePage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            Device.BeginInvokeOnMainThread(()=> {

                var models = GetExerciseViewModels();
                listView.ItemsSource = models;
            });
            FavoriteController.favoriteData.exerciseIDs.CollectionChanged += UpdateListView;
            FavoriteController.favoriteData.subMuscleIDs.CollectionChanged += UpdateListView;
            FavoriteController.favoriteData.muscleIDs.CollectionChanged += UpdateListView;
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            FavoriteController.favoriteData.exerciseIDs.CollectionChanged -= UpdateListView;
            FavoriteController.favoriteData.subMuscleIDs.CollectionChanged -= UpdateListView;
            FavoriteController.favoriteData.muscleIDs.CollectionChanged -= UpdateListView;
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
                var models = FavoriteController.favoriteData.exerciseIDs.Select(y => Utils.As<Exercise, ExerciseViewModel>(MuscleDictionary.GetCurrent().exercises[y]).SetID(y)).ToList();
                models.ForEach(x => x.isFavorite = true);
                if (models.Count() > 0)
                {
                    favorites.Add(new Utils.Grouping<string, Variant>("Упражнения", models.Select(x => new Variant() { exerciseViewModel = x, muscleViewModel = null })));
                }
            }
            {
                var models = FavoriteController.favoriteData.muscleIDs.Select(y => Utils.As<MuscleExtended, MuscleViewModel>(MuscleDictionary.GetCurrent().musclesExtended[y])).ToList();
                models.ForEach(x => x.isFavorite = true);
                if (models.Count() > 0)
                {
                    favorites.Add(new Utils.Grouping<string, Variant>("Мышцы", models.Select(x => new Variant() { exerciseViewModel = null, muscleViewModel = x })));
                }
            }
            {
                var models = FavoriteController.favoriteData.subMuscleIDs.Select(y => Utils.As<MuscleExtended, MuscleViewModel>(MuscleDictionary.GetCurrent().musclesExtended[y.baseID]).SetSubID(y)).ToList();
                models.ForEach(x => x.isFavorite = true);
                if (models.Count() > 0)
                {
                    favorites.Add(new Utils.Grouping<string, Variant>("Подкатегории мышц", models.Select(x => new Variant() { exerciseViewModel = null, muscleViewModel = x })));
                }
            }
            return favorites;
        }
    }
}
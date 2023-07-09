using MuscleAnatomyAndMotion.Controllers;
using MuscleAnatomyAndMotion.Views;
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
    public partial class ExercisePage : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;
        public List<Utils.Grouping<string, MuscleViewModel>> muscleGroups { get; set; }
        public string exerciseName { get; set; }
        public string exerciseImage { get; set; }
        public bool isFavorite
        {
            get => LocalFilesController.favoriteData.exerciseIDs.Contains(exercise.id);
            set
            {
                if (value)
                {
                    if (!LocalFilesController.favoriteData.exerciseIDs.Contains(exercise.id))
                    {
                        LocalFilesController.favoriteData.exerciseIDs.Add(exercise.id);
                    }
                }
                else
                {
                    LocalFilesController.favoriteData.exerciseIDs.Remove(exercise.id);
                }
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("isFavorite"));
            }
        }
        public string shareText { get => $"{exercise.name}"; }
        public List<string> shareUrls { get => new List<string>() { exercise.thumbnail_image_url }; }
        private ExerciseExtended exercise;
        public ExercisePage(ExerciseID exerciseID)
        {
            exercise = App.exercisesExtended[exerciseID];
            exerciseName = exercise.name;
            exerciseImage = exercise.thumbnail_image_url;
            muscleGroups = new List<Utils.Grouping<string, MuscleViewModel>>();
            foreach (var info in new[] {
                new
                {
                    description = "Целевые мышцы",
                    muscles = exercise.muscle_ids.target
                },
                new
                {
                    description = "Синергисты",
                    muscles = exercise.muscle_ids.synergist
                },
                new
                {
                    description = "Стабилизаторы",
                    muscles = exercise.muscle_ids.stabilizer
                },
                new
                {
                    description = "Растяжение",
                    muscles = exercise.muscle_ids.lengthening
                }
            })
            {
                if (info.muscles.Count() > 0)
                {
                    var muscles = info.muscles.Select(y => Utils.As<MuscleExtended, MuscleViewModel>(App.musclesExtended[y.baseID]).SetSubID(y));
                    muscleGroups.Add(new Utils.Grouping<string, MuscleViewModel>(info.description, muscles));
                }
            }
            InitializeComponent();
            BindingContext = this;
        }

        private void videosButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new FullVideo(exercise.correct_videos.Select(x => new FullVideo.VideoInfo { videoUrl = x.asset_url, description = x.description, title = exercise.name }).ToList()) { Title = "Упражнение" });
        }

        private void problemsButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new FullVideo(exercise.mistake_videos.Select(x => new FullVideo.VideoInfo { videoUrl = x.asset_url, description = x.description, title = exercise.name }).ToList()) { Title = "Упражнение" });
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var muscleID = (MuscleID)button.ClassId;
            Navigation.PushAsync(new MuscleInfoPage(muscleID.baseID, muscleID.subID));
        }
    }
}
using MuscleAnatomyAndMotion.Controllers;
using MuscleAnatomyAndMotion.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MuscleInfoRedirectPage : ContentPage
    {
        public class ExerciseInfoData : ViewModelBase
        {
            public ExerciseViewModel exerciseViewModel { get; set; }
            private bool _isFiltered;
            public bool isFiltered
            {
                get => _isFiltered;
                set
                {
                    _isFiltered = value;
                    RaisePropertyChanged("isSelected");
                }
            }
            private bool _isSearched;
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
        }
        public MuscleViewModel muscleViewModel { get; set; }
        public string muscleImage { get; set; }
        public string muscleOverlay { get; set; }
        public List<Utils.Grouping<string, ExerciseInfoData>> exerciseGroups { get; set; }
        public List<Utils.Grouping<string, ExerciseInfoData>> exerciseGroupsFiltered { get; set; }
        private MuscleID muscleID;
        public MuscleInfoRedirectPage(MuscleID muscleID)
        {
            this.muscleID = muscleID;
            var muscle = MuscleDictionary.GetCurrent().musclesExtended[muscleID.baseID];
            var musclePart = muscle.parts.Where(x => x.id.CompareTo(muscleID) == 0).First();
            muscleViewModel = Utils.As<MuscleExtended, MuscleViewModel>(muscle).SetSubID(muscleID);

            exerciseGroups = new List<Utils.Grouping<string, ExerciseInfoData>>();
            foreach (var info in new[] {
                new
                {
                    description = "Целевые мышцы",
                    exercises = musclePart.exerciseIDs.target
                },
                new
                {
                    description = "Синергисты",
                    exercises = musclePart.exerciseIDs.synergist
                },
                new
                {
                    description = "Стабилизаторы",
                    exercises = musclePart.exerciseIDs.stabilizer
                },
                new
                {
                    description = "Растяжение",
                    exercises = musclePart.exerciseIDs.lengthening
                }
            })
            {
                if (info.exercises.Count() > 0)
                {
                    var models = info.exercises.Select(y => Utils.As<Exercise, ExerciseViewModel>(MuscleDictionary.GetCurrent().exercises[y]).SetID(y)).Select(x =>
                            new ExerciseInfoData()
                            {
                                exerciseViewModel = x,
                                isFiltered = true,
                                isSearched = true
                            }).ToList();
                    exerciseGroups.Add(new Utils.Grouping<string, ExerciseInfoData>(info.description, models));
                }
            }
            exerciseGroupsFiltered = exerciseGroups;

            InitializeComponent();

            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            listView.ItemsSource = exerciseGroupsFiltered;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MuscleInfoPage(muscleID.baseID, muscleID.subID));
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            var button = sender as Button;
            var exerciseID = (ExerciseID)button.ClassId;
            Navigation.PushAsync(new ExercisePage(exerciseID));
        }

        FiltersView filtersView = null;
        bool filterUsing = false;

        private void Button_Clicked_2(object sender, EventArgs e)
        {
            if (!filterUsing)
            {
                filtersView = filtersView ?? new FiltersView();
                filtersView.OnCheckedChanged += (_, __) =>
                {
                    exerciseGroups.ForEach(x => x.ForEach(z => z.isFiltered = true));
                    exerciseGroups.ForEach(
                        x => x.Where(
                            y => filtersView.selectedFilters.Where(
                                z => y.exerciseViewModel.filter_ids.Intersect(z).Count() == 0
                            ).Count() > 0
                        ).ForEach(
                            y => y.isFiltered = false
                        )
                    );
                    exerciseGroupsFiltered = exerciseGroups.Select(x => new Utils.Grouping<string, ExerciseInfoData>(x.Name, x.Where(y => y.isFiltered && y.isSearched))).ToList();
                    exerciseGroupsFiltered = exerciseGroupsFiltered.Where(x => x.Count > 0).ToList();
                    listView.ItemsSource = exerciseGroupsFiltered;
                    Device.BeginInvokeOnMainThread(() => {
                        filtersHolder.Children.Remove(filtersView);
                        filtersHolder.Children.Add(filtersView);
                    });
                };
                filtersHolder.Children.Add(filtersView);
            }
            else
            {
                filtersHolder.Children.Remove(filtersView);
            }
            filterUsing = !filterUsing;
        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            exerciseGroups.ForEach(x => x.ForEach(z => z.isSearched = false));
            var searchValues = e.NewTextValue.ToLower().Split(' ').Where(x => x != "").ToList();
            exerciseGroups.ForEach(
                x => x.Where(
                    y => searchValues.Where(
                        z => y.exerciseViewModel.name.ToLower().Split(' ').Where(
                            xx => xx.StartsWith(z)
                        ).Count() == 1
                    ).Count() == searchValues.Count
                ).ForEach(
                    y => y.isSearched = true
                )
            );
            exerciseGroupsFiltered = exerciseGroups.Select(x => x.Where(y => y.isFiltered && y.isSearched) as Utils.Grouping<string, ExerciseInfoData>).ToList();
            exerciseGroupsFiltered = exerciseGroupsFiltered.Where(x => x.Count > 0).ToList();
            listView.ItemsSource = exerciseGroupsFiltered;
        }
    }
}
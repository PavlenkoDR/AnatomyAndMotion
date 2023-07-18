using MuscleAnatomyAndMotion.Controllers;
using MuscleAnatomyAndMotion.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExerciseListPage : ContentPage
    {
        public List<ExerciseViewModel> ExerciseList { get; set; }
        public List<ExerciseViewModel> ExerciseListFiltered { get; set; }
        public ExerciseListPage()
        {
            ExerciseList = MuscleDictionary.GetCurrent().exercises.Select(x => Utils.As<Exercise, ExerciseViewModel>(x.Value).SetID(x.Key)).ToList();
            ExerciseListFiltered = ExerciseList;
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            Task.Run(()=> {
                Device.BeginInvokeOnMainThread(() =>
                {
                    listView.ItemsSource = null;
                    listView.ItemsSource = ExerciseListFiltered;
                });
            });
            base.OnAppearing();
        }

        FiltersView filtersView = null;
        bool filterUsing = false;

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (!filterUsing)
            {
                filtersView = filtersView ?? new FiltersView();
                filtersView.OnCheckedChanged += (_, __) =>
                {
                    ExerciseList.ForEach(x => x.isFiltered = true);
                    ExerciseList.Where(
                        y => filtersView.selectedFilters.Where(
                            z => y.filter_ids.Intersect(z).Count() == 0
                        ).Count() > 0
                    ).ForEach(
                        y => y.isFiltered = false
                    );
                    Device.BeginInvokeOnMainThread(() => {
                        ExerciseListFiltered = ExerciseList.Where(x => x.isSelected).ToList();
                        listView.ItemsSource = ExerciseListFiltered;
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
            ExerciseList.ForEach(x => x.isSearched = false);
            var searchValues = e.NewTextValue.ToLower().Split(' ').Where(x => x != "").ToList();
            ExerciseList.Where(
                y => searchValues.Where(
                    z => y.name.ToLower().Split(' ').Where(
                        xx => xx.StartsWith(z)
                    ).Count() == 1
                ).Count() == searchValues.Count
            ).ForEach(
                y => y.isSearched = true
            );
            ExerciseListFiltered = ExerciseList.Where(x => x.isSelected).ToList();
            listView.ItemsSource = ExerciseListFiltered;
        }
    }
}
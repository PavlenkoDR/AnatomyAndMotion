using MuscleAnatomyAndMotion.Controllers;
using MuscleAnatomyAndMotion.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MuscleInfoListPage : ContentPage
    {

        public ObservableCollection<MuscleViewModel> MuscleList { get; set; }
        public MuscleInfoListPage()
        {
            MuscleList = new ObservableCollection<MuscleViewModel>(MuscleDictionary.musclesExtended.Values.Select(x => Utils.As<MuscleExtended, MuscleViewModel>(x)));
            InitializeComponent();
            BindingContext = this;
            mainStack.ItemsSource = MuscleList;
        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            MuscleList.ForEach(x => x.isSelected = false);
            var searchValues = e.NewTextValue.ToLower().Split(' ').Where(x => x != "").ToList();
            MuscleList.Where(
                y => searchValues.Where(
                    z => y.name.ToLower().Split(' ').Where(
                        xx => xx.StartsWith(z)
                    ).Count() == 1
                ).Count() == searchValues.Count
            ).ForEach(
                y => y.isSelected = true
            );
            mainStack.ItemsSource = MuscleList.Select(x => x.isSelected).ToList();
        }
    }
}
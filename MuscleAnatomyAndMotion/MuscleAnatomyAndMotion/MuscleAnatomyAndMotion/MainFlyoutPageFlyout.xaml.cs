using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainFlyoutPageFlyout : ContentPage
    {
        public ListView ListView;

        public MainFlyoutPageFlyout()
        {
            InitializeComponent();

            BindingContext = new MainFlyoutPageFlyoutViewModel();
            ListView = MenuItemsListView;
        }

        public class MainFlyoutPageFlyoutViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<IMainFlyoutPageFlyoutMenuItem> MenuItems { get; set; }
            
            public MainFlyoutPageFlyoutViewModel()
            {
                MenuItems = new ObservableCollection<IMainFlyoutPageFlyoutMenuItem>(new IMainFlyoutPageFlyoutMenuItem[]
                {
                    new MainFlyoutPageFlyoutMenuItemMuscleInfo { Id = 0, Title = "Список" },
                    new MainFlyoutPageFlyoutMenuItemAnatomy { Id = 1, ContentScale = 2, Title = "Tors",  muscleAreaName = "tors", xOffset = -2,  maxLayer = 6 },
                    new MainFlyoutPageFlyoutMenuItemAnatomy { Id = 2, ContentScale = 3, Title = "Hand Top",  muscleAreaName = "hand_top",  maxLayer = 3 },
                    new MainFlyoutPageFlyoutMenuItemAnatomy { Id = 3, ContentScale = 3, Title = "Hand Bottom",  muscleAreaName = "hand_bottom",  maxLayer = 5 },
                    new MainFlyoutPageFlyoutMenuItemAnatomy { Id = 4, ContentScale = 3, Title = "Leg Top",  muscleAreaName = "leg_top", xOffset = -2,  maxLayer = 6 },
                    new MainFlyoutPageFlyoutMenuItemAnatomy { Id = 5, ContentScale = 3, Title = "Leg Bottom",  muscleAreaName = "leg_bottom", maxLayer = 4 },
                    new MainFlyoutPageFlyoutMenuItemAnatomy { Id = 6, ContentScale = 2, Title = "Head",  muscleAreaName = "head", xOffset = -2,  maxLayer = 9 }
                });
            }
            
            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}
﻿using System;
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
                var muscleAssetValuess = App.muscleAssets.ToList();
                if (muscleAssetValuess.Count == 0)
                {
                    return;
                }
                MenuItems = new ObservableCollection<IMainFlyoutPageFlyoutMenuItem>(new IMainFlyoutPageFlyoutMenuItem[]
                {
                    new MainFlyoutPageFlyoutMenuItem { Id = 0, Title = "Упражнения" },
                    new MainFlyoutPageFlyoutMenuItem { Id = 1, Title = "Мышцы" },
                    new MainFlyoutPageFlyoutMenuItem { Id = 2, Title = "Избранное" },
                    new MainFlyoutPageFlyoutMenuItemAnatomy { Id = 10, ContentScale = 2, Title = "Torso",  bodyPartID = muscleAssetValuess.Where(x => x.Value.body_part == "torso").First().Key, xOffset = -2,  maxLayer = 6 },
                    new MainFlyoutPageFlyoutMenuItemAnatomy { Id = 11, ContentScale = 3, Title = "Hand Top",  bodyPartID =  muscleAssetValuess.Where(x => x.Value.body_part == "upperArm").First().Key,  maxLayer = 3 },
                    new MainFlyoutPageFlyoutMenuItemAnatomy { Id = 12, ContentScale = 3, Title = "Hand Bottom",  bodyPartID = muscleAssetValuess.Where(x => x.Value.body_part == "lowerArm").First().Key,  maxLayer = 5 },
                    new MainFlyoutPageFlyoutMenuItemAnatomy { Id = 13, ContentScale = 2.8f, Title = "Leg Top",  bodyPartID = muscleAssetValuess.Where(x => x.Value.body_part == "upperLegPelvis").First().Key, xOffset = -2,  maxLayer = 6 },
                    new MainFlyoutPageFlyoutMenuItemAnatomy { Id = 14, ContentScale = 2.6f, Title = "Leg Bottom",  bodyPartID = muscleAssetValuess.Where(x => x.Value.body_part == "lowerLegFoot").First().Key, maxLayer = 4 },
                    new MainFlyoutPageFlyoutMenuItemAnatomy { Id = 15, ContentScale = 1.8f, Title = "Head",  bodyPartID =  muscleAssetValuess.Where(x => x.Value.body_part == "headNeck").First().Key, xOffset = -2,  maxLayer = 9 }
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
using MuscleAnatomyAndMotion.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainFlyoutPage : FlyoutPage
    {
        public MainFlyoutPage()
        {
            InitializeComponent();
            FlyoutPage.ListView.ItemSelected += ListView_ItemSelected;
            var menuItems = ((Flyout as MainFlyoutPageFlyout).BindingContext as MainFlyoutPageFlyout.MainFlyoutPageFlyoutViewModel).MenuItems;
            OpenPage(menuItems?.Count > 0 ? menuItems[0] : null);
        }
        
        SortedDictionary<string, List<int>> rotateTimingFrames = new SortedDictionary<string, List<int>>()
        {
            {"torso", new List<int>{2, 21, 31, 61} },
            {"upperArm", new List<int>{2, 21, 31, 41, 61} },
            {"lowerArm", new List<int>{2, 17, 27, 37, 60} },
            {"upperLegPelvis", new List<int>{2, 21, 31, 41, 61} },
            {"lowerLegFoot", new List<int>{2, 21, 31, 41, 61} },
            {"headNeck", new List<int>{0, 19, 29, 60} }
        };

        private void OpenPage(IMainFlyoutPageFlyoutMenuItem item)
        {
            Page page = null;
            if (item?.GetType() == typeof(MainFlyoutPageFlyoutMenuItemAnatomy))
            {
                var itemm = item as MainFlyoutPageFlyoutMenuItemAnatomy;
                page = new AnatomyPage(itemm.xOffset, itemm.ContentScale, itemm.bodyPartID, rotateTimingFrames[MuscleDictionary.muscleAssets[itemm.bodyPartID].body_part]);
                page.Title = itemm.Title;
            }
            else if (item?.GetType() == typeof(MainFlyoutPageFlyoutMenuItem))
            {
                var itemm = item as MainFlyoutPageFlyoutMenuItem;
                if (itemm.Id == 0)
                {
                    page = new ExerciseListPage();
                    page.Title = itemm.Title;
                }
                else if (itemm.Id == 1)
                {
                    page = new MuscleInfoListPage();
                    page.Title = itemm.Title;
                }
                else if (itemm.Id == 2)
                {
                    page = new FavoritePage();
                    page.Title = itemm.Title;
                }
                else if (itemm.Id == 3)
                {
                    page = new DownloadsPage();
                    page.Title = itemm.Title;
                }
            }

            var navigationPage = new NavigationPage(page) { BarBackgroundColor = Color.Orange, BarTextColor = Color.Black, BackgroundColor = Color.Black };
            if (!ResourceController.IsOffline)
            {
                navigationPage.ToolbarItems.Add(new ToolbarItem("Очистить кэш", null, () => {
                    Task.Run(() => {
                        Device.BeginInvokeOnMainThread(async () => {
                            await Navigation.PushModalAsync(new LoadingBanner() { Progress = "Очистка кэша" });
                            WebResourceController.ClearCache();
                            await Navigation.PopModalAsync();
                        });
                    });
                }));
            }
            Detail = navigationPage;
            Application.Current.SendResume();
            IsPresented = false;

            FlyoutPage.ListView.SelectedItem = null;
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as IMainFlyoutPageFlyoutMenuItem;
            if (item == null)
            {
                return;
            }
            OpenPage(item);
        }
    }
}
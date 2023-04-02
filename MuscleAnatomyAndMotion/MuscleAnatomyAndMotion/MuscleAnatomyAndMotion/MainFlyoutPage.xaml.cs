using System.Collections.Generic;
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
            var menuItem = ((Flyout as MainFlyoutPageFlyout).BindingContext as MainFlyoutPageFlyout.MainFlyoutPageFlyoutViewModel).MenuItems[0];
            OpenPage(menuItem);
        }
        
        SortedDictionary<string, List<int>> rotateTimingFrames = new SortedDictionary<string, List<int>>()
        {
            {"tors", new List<int>{2, 21, 31, 61} },
            {"hand_top", new List<int>{2, 21, 31, 41, 61} },
            {"hand_bottom", new List<int>{2, 17, 27, 37, 60} },
            {"leg_top", new List<int>{2, 21, 31, 41, 61} },
            {"leg_bottom", new List<int>{2, 21, 31, 41, 61} },
            {"head", new List<int>{0, 19, 29, 60} }
        };

        private void OpenPage(IMainFlyoutPageFlyoutMenuItem item)
        {
            Page page = null;
            if (item.GetType() == typeof(MainFlyoutPageFlyoutMenuItemAnatomy))
            {
                var itemm = item as MainFlyoutPageFlyoutMenuItemAnatomy;
                page = new AnatomyPage(itemm.xOffset, itemm.ContentScale, itemm.muscleAreaName, itemm.maxLayer, rotateTimingFrames[itemm.muscleAreaName]);
                page.Title = itemm.Title;
            }
            else if (item.GetType() == typeof(MainFlyoutPageFlyoutMenuItemMuscleInfo))
            {
                var itemm = item as MainFlyoutPageFlyoutMenuItemMuscleInfo;
                page = new MuscleInfoListPage();
                page.Title = itemm.Title;
            }

            Detail = new NavigationPage(page) { BarBackgroundColor = Color.Orange, BarTextColor = Color.Black };
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
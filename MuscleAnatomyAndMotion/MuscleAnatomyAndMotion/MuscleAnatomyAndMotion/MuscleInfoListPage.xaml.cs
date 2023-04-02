using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MuscleInfoListPage : ContentPage
    {
        public class MuscleCellInfoInner
        {
            public string MuscleName { get; set; }
            public ImageSource MuscleImage { get; set; }
            public ImageSource MuscleImageCover { get; set; }
        }
        public class MuscleCellInfo
        {
            public List<MuscleCellInfoInner> MuscleCell { get; set; }
            public string MuscleIndex { get; set; }
        }
        public ObservableCollection<MuscleCellInfo> MuscleCellsBase { get; set; }
        public ObservableCollection<MuscleCellInfo> MuscleCells { get; set; }
        public MuscleInfoListPage()
        {
            BindingContext = this;
            App.CollectionChanged += (sender, e) =>
            {
                FillMuscleCells();
            };
            FillMuscleCells();
            InitializeComponent();
        }

        private ImageSource getImageSourceFromMovementInfo(string info)
        {
            if (info == null)
            {
                return null;
            }
            var path = $"MuscleAnatomyAndMotion.Assets._{info.Replace("/", ".")}";
            var assembly = Application.Current.GetType().Assembly;
            var fileNames = assembly.GetManifestResourceNames().ToList();
            if (!fileNames.Contains(path))
            {
                return null;
            }
            var source = ImageSource.FromResource(path);
            return source;
        }

        private void FillMuscleCells()
        {
            MuscleCellsBase = new ObservableCollection<MuscleCellInfo>();
            foreach (var info in App.muscleFullInfos)
            {
                MuscleCellsBase.Add(new MuscleCellInfo()
                {
                    MuscleIndex = info.Key.ToString(),
                    MuscleCell = info.Value.mainVideos.Select(x =>
                    {
                        return new MuscleCellInfoInner()
                        {
                            MuscleName = x.muscle,
                            MuscleImage = x.MuscleImage,
                            MuscleImageCover = x.MuscleImageCover
                        };
                    }).ToList()
                });
            }
            MuscleCells = new ObservableCollection<MuscleCellInfo>(MuscleCellsBase);
            BindingContext = null;
            BindingContext = this;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var index = int.Parse(button.ClassId);
            Navigation.PushAsync(new MuscleInfoPage(App.muscleFullInfos[index], 0));
        }

        void RunSearch(string text)
        {
            search = Task.Run(() => {
                foreach (var cell in MuscleCellsBase)
                {
                    if (string.Join("\n", cell.MuscleCell.Select(x => x.MuscleName).ToList()).ToLower().Contains(text))
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            MuscleCells.Add(cell);
                        });
                    }
                }
            });
        }

        Task search = null;
        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            MuscleCells.Clear();
            if (search != null && !search.IsCompleted)
            {
                search.ContinueWith(x =>
                {
                    RunSearch(e.NewTextValue);
                });
            }
            else
            {
                RunSearch(e.NewTextValue);
            }
        }

        private void scrollLeftButton_Clicked(object sender, EventArgs e)
        {
            var view = ((sender as View).Parent.Parent as Grid).Children.Where(x => x.GetType() == typeof(CarouselView)).First() as CarouselView;
            view.ScrollTo(Math.Max(view.Position - 1, 0));
        }

        private void scrollRightButton_Clicked(object sender, EventArgs e)
        {
            var view = ((sender as View).Parent.Parent as Grid).Children.Where(x => x.GetType() == typeof(CarouselView)).First() as CarouselView;
            view.ScrollTo(Math.Min(view.Position + 1, (view.ItemsSource as List<MuscleCellInfoInner>).Count));
        }
    }
}
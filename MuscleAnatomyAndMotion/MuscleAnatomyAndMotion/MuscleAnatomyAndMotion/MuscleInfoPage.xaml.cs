using MuscleAnatomyAndMotion.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static MuscleAnatomyAndMotion.App;

namespace MuscleAnatomyAndMotion
{
    public class MovementInfo
    {
        public string movementName { get; set; }
        public Func<Page> videoUrls { get; set; }
        public List<string> imageUrls { get; set; }
        public string imageDescription { get; set; }
    }
    public class MuscleInfo : ViewModelBase
    {
        public MuscleID id { get; set; }
        public MusclePart musclePart { get; set; }
        public string muscle { get; set; }
        public ImageSource MuscleImage { get; set; }
        public ImageSource MuscleImageCover { get; set; }
        public bool IsImageEnabled { get => MuscleImage != null; }
        public string imageDescriptionOrigin { get; set; }
        public string imageDescriptionInsertion { get; set; }
        public List<MovementInfo> movements { get; set; }
        public bool isFavorite
        {
            get => LocalFilesController.favoriteData.subMuscleIDs.Contains(id);
            set
            {
                if (value && id != null)
                {
                    if (!LocalFilesController.favoriteData.subMuscleIDs.Contains(id))
                    {
                        LocalFilesController.favoriteData.subMuscleIDs.Add(id);
                    }
                }
                else
                {
                    LocalFilesController.favoriteData.subMuscleIDs.Remove(id);
                }
                RaisePropertyChanged("isFavorite");
            }
        }
        public bool isHaveFavorite { get => id != null; }
        public string shareText { get => $"{muscle}\n\n{imageDescriptionOrigin}\n\n{imageDescriptionInsertion}"; }
        public List<string> shareUrls { get => new List<string>() { musclePart?.image_url }; }
    }
    public class MuscleFullInfo
    {
        public List<MuscleInfo> mainVideos { get; set; }
        public string moreInfoText { get; set; }
        public string strengthVideoUrl { get; set; }
        public string stretchVideoUrl { get; set; }
        public string originAndInsertionVideoUrl { get; set; }
    }

    public static class ListExtention
    {
        public static List<T> AddSequence<T>(this List<T> list, T info)
        {
            list.Add(info);
            return list;
        }
        public static List<T> AddRangeSequence<T>(this List<T> list, IEnumerable<T> info)
        {
            list.AddRange(info);
            return list;
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MuscleInfoPage : ContentPage
    {
        MuscleFullInfo info;
        public int InfoIndex { get; set; }
        public MuscleInfoPage(BaseMuscleID muscleID, SubMuscleID InfoIndex)
        {
            MuscleExtended info = App.musclesExtended[muscleID];
            this.InfoIndex = int.Parse(InfoIndex.ToString());
            this.Title = info.name;
            this.info = new MuscleFullInfo()
            {
                mainVideos = new List<MuscleInfo>(info.parts.Select(x => new MuscleInfo() {
                    id = x.id,
                    musclePart = x,
                    imageDescriptionOrigin = $"Origin:\n{x.origin}",
                    imageDescriptionInsertion = $"Insertion:\n{x.insertion}",
                    movements = new List<MovementInfo>() { new MovementInfo()
                    {
                        movementName = "Упражнения",
                        videoUrls = () =>
                        {
                            return new MuscleInfoRedirectPage(x.id);
                        }
                    }}.AddRangeSequence( x.actions?.Select(y => new MovementInfo()
                        {
                            movementName = y.name,
                            videoUrls = y.videos != null ? CreateVideoOpenAction(y.videos?.Select(z => z.video_url).ToList(), y.name) : null
                        })
                    ),
                    muscle = x.name,
                    MuscleImage = ExternalResourceController.GetImageSource("ru", $"{x.image_url}").Result,
                    MuscleImageCover = ExternalResourceController.GetImageSource("ru", $"{x.overlay_image_url}").Result
                })),
                originAndInsertionVideoUrl = info.origin?.video_url,
                moreInfoText = info.description.actions,
                strengthVideoUrl = info.strength_video_url,
                stretchVideoUrl = info.stretch_video_url
            };
            if (this.info.moreInfoText != null && this.info.moreInfoText != ""
                || this.info.originAndInsertionVideoUrl != null && this.info.originAndInsertionVideoUrl != ""
                || this.info.strengthVideoUrl != null && this.info.strengthVideoUrl != ""
                || this.info.stretchVideoUrl != null && this.info.stretchVideoUrl != "")
            this.info.mainVideos.Add(new MuscleInfo() {
                muscle = "Подробнее",
                imageDescriptionOrigin = info.description.actions,
                movements = new List<MovementInfo>()
                {
                    new MovementInfo()
                    {
                        movementName = "originAndInsertionVideoUrl",
                        videoUrls = info.origin != null ? CreateVideoOpenAction(new List<string>(){ info.origin?.video_url }, "originAndInsertionVideoUrl") : null
                    },
                    new MovementInfo()
                    {
                        movementName = "strengthVideoUrl",
                        videoUrls = info.strength_video_url != null ? CreateVideoOpenAction(new List<string>(){ info.strength_video_url }, "strengthVideoUrl") : null
                    },
                    new MovementInfo()
                    {
                        movementName = "stretchVideoUrl",
                        videoUrls = info.stretch_video_url != null ? CreateVideoOpenAction(new List<string>(){ info.stretch_video_url }, "stretchVideoUrl") : null
                    }
                }.Where(x => x.videoUrls != null).ToList()
            });
            BindingContext = this.info;
            InitializeComponent();
            Device.BeginInvokeOnMainThread(() => 
            { 
                MainCarousel.Position = this.InfoIndex;
            });
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private Func<Page> CreateVideoOpenAction(List<string> videoUrls, string movementName)
        {
            return () => new FullVideo(videoUrls.Select(x => new FullVideo.VideoInfo { videoUrl = x, title = movementName }).ToList()) { Title = "Мышцы" };
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var info = button.BindingContext as MuscleInfo;
        }

        private void Button_Clicked_2(object sender, EventArgs e)
        {
            var button = sender as Button;
            var info = button.BindingContext as MovementInfo;
            Navigation.PushAsync(info.videoUrls.Invoke());
        }

        private void CarouselView_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
        {
            var info = e.CurrentItem as MuscleInfo;

        }
        private void scrollLeftButton_Clicked(object sender, EventArgs e)
        {
            var view = ((sender as View).Parent.Parent as Grid).Children.Where(x => x.GetType() == typeof(CarouselView)).First() as CarouselView;
            view.ScrollTo(Math.Max(view.Position - 1, 0));
        }

        private void scrollRightButton_Clicked(object sender, EventArgs e)
        {
            var view = ((sender as View).Parent.Parent as Grid).Children.Where(x => x.GetType() == typeof(CarouselView)).First() as CarouselView;
            view.ScrollTo(Math.Min(view.Position + 1, (view.ItemsSource as List<MuscleInfo>).Count));
        }
    }
}
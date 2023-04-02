using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    public class MovementInfo
    {
        public string movementName { get; set; }
        public List<string> videoUrls { get; set; }
        public List<string> imageUrls { get; set; }
        public string imageDescription { get; set; }
    }
    public class MuscleInfo
    {
        public string muscle { get; set; }
        public ImageSource MuscleImage { get; set; }
        public ImageSource MuscleImageCover { get; set; }
        public string imageDescription { get; set; }
        public List<MovementInfo> movements { get; set; }
    }
    public class MuscleFullInfo
    {
        public List<MuscleInfo> mainVideos { get; set; }
        public string moreInfoText { get; set; }
        public string strengthVideoUrl { get; set; }
        public string stretchVideoUrl { get; set; }
        public string originAndInsertionVideoUrl { get; set; }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MuscleInfoPage : ContentPage
    {
        MuscleFullInfo info;
        public int InfoIndex { get; set; }
        public MuscleInfoPage(MuscleFullInfo info, int InfoIndex)
        {
            this.InfoIndex = InfoIndex;
            this.info = new MuscleFullInfo()
            {
                mainVideos = new List<MuscleInfo>(info.mainVideos),
                originAndInsertionVideoUrl = info.originAndInsertionVideoUrl,
                moreInfoText = info.moreInfoText,
                strengthVideoUrl = info.strengthVideoUrl,
                stretchVideoUrl = info.stretchVideoUrl
            };
            if (info.moreInfoText != null && info.moreInfoText != ""
                || info.originAndInsertionVideoUrl != null && info.originAndInsertionVideoUrl != ""
                || info.strengthVideoUrl != null && info.strengthVideoUrl != ""
                || info.stretchVideoUrl != null && info.stretchVideoUrl != "")
            this.info.mainVideos.Add(new MuscleInfo() {
                muscle = "Подробнее",
                imageDescription = info.moreInfoText,
                movements = new List<MovementInfo>()
                {
                    new MovementInfo()
                    {
                        movementName = "originAndInsertionVideoUrl",
                        videoUrls = new List<string>(){ info.originAndInsertionVideoUrl }
                    },
                    new MovementInfo()
                    {
                        movementName = "strengthVideoUrl",
                        videoUrls = new List<string>(){ info.strengthVideoUrl }
                    },
                    new MovementInfo()
                    {
                        movementName = "stretchVideoUrl",
                        videoUrls = new List<string>(){ info.stretchVideoUrl }
                    }
                }.Where(x => x.videoUrls[0] != null && x.videoUrls[0] != "").ToList()
            });
            BindingContext = this.info;
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            MainCarousel.ScrollTo(InfoIndex);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var info = button.BindingContext as MuscleInfo;
            Title = info.muscle;
        }

        private void Button_Clicked_2(object sender, EventArgs e)
        {
            var button = sender as Button;
            var info = button.BindingContext as MovementInfo;
            Navigation.PushAsync(new FullVideo(info.videoUrls) { Title = info.movementName });
        }

        private void CarouselView_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
        {
            var info = e.CurrentItem as MuscleInfo;
            Title = info.muscle;
        }
    }
}
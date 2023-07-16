using MuscleAnatomyAndMotion.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FullVideo : ContentPage
    {
        public class VideoInfo
        {
            public string videoUrl { get; set; }
            public string title { get; set; }
            public string description { get; set; }
        }
        public class Model : ViewModelBase
        {
            private List<VideoInfo> _videoUrls;
            public List<VideoInfo> videoUrls
            {
                get => _videoUrls;
                set
                {
                    _videoUrls = value;
                    RaisePropertyChanged("videoUrls");
                }
            }
            private string _imageUrl;
            public string imageUrl
            {
                get => _imageUrl;
                set
                {
                    _imageUrl = value;
                    RaisePropertyChanged("imageUrl");
                }
            }
        } 
        Model model = new Model();
        public FullVideo(List<VideoInfo> videoUrls)
        {
            model.videoUrls = videoUrls;
            BindingContext = model;
            InitializeComponent();
        }

        private Task Start(VideoInfo pathRow)
        {
            return Device.InvokeOnMainThreadAsync(async () => {
                if (Path.GetExtension(pathRow.videoUrl) == ".mp4")
                {
                    //await CrossMediaManager.Current.Stop();
                    Stream stream = await ResourceController.GetInputStream("ru", $"{pathRow.videoUrl}");
                    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{pathRow.videoUrl}");
                    if (WebResourceController.IsFileDownloaded("ru", $"{pathRow.videoUrl}"))
                    {
                        path = (stream as FileStream).Name;
                    }
                    else if (File.Exists(path))
                    {
                        File.Delete(path);
                        new FileInfo(path).Directory.Create();
                        using (var fileStream = File.OpenWrite(path))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }
                    videoView.IsVisible = true;
                    videoView.SetVideoPath(path);
                    videoView.Start();
                    videoView.SeekTo(TimeSpan.FromMilliseconds(0));
                    var size = videoView.GetOriginalVideoSize();
                    videoView.HeightRequest = size.Height * videoView.Width / size.Width;
                    model.imageUrl = "";
                }
                else
                {
                    Stream stream = await ResourceController.GetInputStream("ru", $"{pathRow.videoUrl}");
                    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{pathRow.videoUrl}");
                    new FileInfo(path).Directory.Create();
                    if (File.Exists(path) && !WebResourceController.IsFileDownloaded("ru", $"{pathRow.videoUrl}"))
                    {
                        File.Delete(path);
                    }
                    new FileInfo(path).Directory.Create();
                    using (var fileStream = File.OpenWrite(path))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                    videoView.Pause();
                    videoView.IsVisible = false;
                    model.imageUrl = path;
                }
                shareView.Text = $"{pathRow.title}\n\n{pathRow.description}";
                shareView.Urls = new List<string>() { pathRow.videoUrl };
                description.Text = pathRow.description;
                title.Text = pathRow.title;
            });
        }

        protected override void OnDisappearing()
        {
            videoView.Stop();
            base.OnDisappearing();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (Width > height)
            {
                mainStack.Orientation = StackOrientation.Horizontal;
            }
            else
            {
                mainStack.Orientation = StackOrientation.Vertical;
            }
        }

        bool isFirstOpen = true;

        private void CarouselView_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
        {
            Task.Run(async () => {

                if (!ResourceController.IsOffline)
                {
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
                        var loadingBanner = new LoadingBanner();
                        loadingBanner.Progress = "Загрузка видео";
                        await Navigation.PushModalAsync(loadingBanner);
                    });
                }
                if (isFirstOpen)
                {
                    await Task.Delay(100);
                    isFirstOpen = false;
                }
                await Start(e.CurrentItem as VideoInfo);
                if (!ResourceController.IsOffline)
                {
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
                        await Navigation.PopModalAsync();
                    });
                }
            });
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            videoView.SeekBackward();
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            if (videoView.IsPlaying)
            {
                videoView.Pause();
            }
            else
            {
                videoView.Play();
            }
        }

        private void Button_Clicked_2(object sender, EventArgs e)
        {
            videoView.SeekForward();
        }
        private void scrollLeftButton_Clicked(object sender, EventArgs e)
        {
            var view = ((sender as View).Parent.Parent as Grid).Children.Where(x => x.GetType() == typeof(CarouselView)).First() as CarouselView;
            view.ScrollTo(Math.Max(view.Position - 1, 0));
        }

        private void scrollRightButton_Clicked(object sender, EventArgs e)
        {
            var view = ((sender as View).Parent.Parent as Grid).Children.Where(x => x.GetType() == typeof(CarouselView)).First() as CarouselView;
            view.ScrollTo(Math.Min(view.Position + 1, (view.ItemsSource as List<string>).Count));
        }
    }
}
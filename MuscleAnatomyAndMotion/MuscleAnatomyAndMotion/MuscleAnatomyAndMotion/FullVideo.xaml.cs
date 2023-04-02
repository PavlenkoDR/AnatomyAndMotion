using System;
using System.Collections.Generic;
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
        public List<string> videoUrls { get; set; }
        public FullVideo(List<string> videoUrls)
        {
            this.videoUrls = videoUrls;
            BindingContext = this;
            InitializeComponent();
        }

        private async void Start(string pathRow)
        {
            //await CrossMediaManager.Current.Stop();
            var assembly = Application.Current.GetType().Assembly;

            Stream stream = assembly.GetManifestResourceStream($"MuscleAnatomyAndMotion.Assets._{pathRow.Replace("/", ".")}");
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"MuscleAnatomyAndMotion.Assets._{pathRow.Replace("/", ".")}");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            using (var fileStream = File.OpenWrite(path))
            {
                await stream.CopyToAsync(fileStream);
            }
            videoView.SetVideoPath(path);
            videoView.Start();
            videoView.SeekTo(TimeSpan.FromMilliseconds(0));
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            videoView.Stop();
        }

        private void CarouselView_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
        {
            Start(e.CurrentItem.ToString());
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
    }
}
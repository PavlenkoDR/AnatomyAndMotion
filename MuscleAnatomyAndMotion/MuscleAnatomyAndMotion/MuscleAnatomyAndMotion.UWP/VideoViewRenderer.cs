using CrossVideoView.UWP;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Graphics.Display;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(CrossVideoView.VideoView), typeof(VideoViewRenderer))]
namespace CrossVideoView.UWP
{
    public class VideoElementHandler : UserControl
    {
        public MediaPlayerElement video;
        public MediaPlayer mediaPlayer;

        public delegate void VideoEvent();
        public event VideoEvent OnVideoStart;
        public event VideoEvent OnVideoStop;
        public event VideoEvent OnVideoPlay;
        public event VideoEvent OnVideoPause;
        
        TimeSpan StepSizeForward = TimeSpan.FromSeconds(5);
        TimeSpan StepSizeBackward = TimeSpan.FromSeconds(5);
        
        public VideoElementHandler()
        {
            video = new MediaPlayerElement();
            mediaPlayer = new MediaPlayer();
            video.SetMediaPlayer(mediaPlayer);
            Content = video;
            video.Stretch = Stretch.Uniform;
        }

        public void Start()
        {
            mediaPlayer.Play();
            OnVideoStart?.Invoke();
        }
        public void Stop()
        {
            mediaPlayer.Pause();
            OnVideoStop?.Invoke();
        }
        public void Play()
        {
            mediaPlayer.Play();
            OnVideoPlay?.Invoke();
        }
        public void Pause()
        {
            mediaPlayer.Pause();
            OnVideoPause?.Invoke();
        }
        public void SetVideoURI(string uri)
        {
            mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(uri));
        }

        public void SetVideoPath(string path)
        {
            mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(path));
        }

        public void SeekTo(TimeSpan time)
        {
            mediaPlayer.PlaybackSession.Position = time + TimeSpan.FromMilliseconds(32);
        }

        public void SeekForward()
        {
            mediaPlayer.PlaybackSession.Position += StepSizeForward;
        }

        public void SeekBackward()
        {
            mediaPlayer.PlaybackSession.Position -= StepSizeBackward;
        }
    }
    public class VideoViewRenderer : ViewRenderer<VideoView, VideoElementHandler>
    {
        VideoElementHandler videoElementHandler;

        protected override void OnElementChanged(ElementChangedEventArgs<VideoView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    videoElementHandler = new VideoElementHandler();
                    e.NewElement.OnStart += videoElementHandler.Start;
                    e.NewElement.OnStop += videoElementHandler.Stop;
                    e.NewElement.OnPlay += videoElementHandler.Play;
                    e.NewElement.OnPause += videoElementHandler.Pause;
                    e.NewElement.OnSetVideoPath += videoElementHandler.SetVideoPath;
                    e.NewElement.OnSetVideoURI += videoElementHandler.SetVideoURI;
                    e.NewElement.OnSeekTo += videoElementHandler.SeekTo;
                    e.NewElement.OnSeekForward += videoElementHandler.SeekForward;
                    e.NewElement.OnSeekBackward += videoElementHandler.SeekBackward;
                    
                    videoElementHandler.OnVideoStart += () => e.NewElement.IsPlaying = videoElementHandler.mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;
                    videoElementHandler.OnVideoStop += () => e.NewElement.IsPlaying = videoElementHandler.mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;
                    videoElementHandler.OnVideoPlay += () => e.NewElement.IsPlaying = videoElementHandler.mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;
                    videoElementHandler.OnVideoPause += () => e.NewElement.IsPlaying = videoElementHandler.mediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;

                    SetNativeControl(videoElementHandler);
                }
            }
        }
    }
}

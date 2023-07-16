using System;
using System.ComponentModel;
using AndroidX.Fragment.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.FastRenderers;
using Android.OS;
using Android.Util;
using Android.Media;
using Java.IO;
using Android.Runtime;
using Android.Graphics;
using MuscleAnatomyAndMotion;
using System.Threading.Tasks;

[assembly: ExportRenderer(typeof(CrossVideoView.VideoView), typeof(CrossVideoView.Droid.VideoViewRenderer))]
namespace CrossVideoView.Droid
{
    class VideoElementHandler : Fragment, MediaPlayer.IOnPreparedListener, ISurfaceHolderCallback
    {
        public delegate void VideoEvent();
        public event VideoEvent OnVideoStart;
        public event VideoEvent OnVideoStop;
        public event VideoEvent OnVideoPlay;
        public event VideoEvent OnVideoPause;

        public Android.Widget.VideoView video = null;
        public MediaPlayer mediaPlayer = new MediaPlayer();
        TimeSpan StepSizeForward = TimeSpan.FromSeconds(5);
        TimeSpan StepSizeBackward = TimeSpan.FromSeconds(5);
        TimeSpan seekDelta = TimeSpan.FromSeconds(0);
        bool mediaChanged = false;

        public override Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }
            video = new Android.Widget.VideoView(Xamarin.Essentials.Platform.AppContext);
            video.LayoutParameters = new ViewGroup.LayoutParams(mediaPlayer.VideoWidth, mediaPlayer.VideoHeight);
            video.SetForegroundGravity(GravityFlags.Center);

            ISurfaceHolder holder = video.Holder;
            holder.AddCallback(this);

            mediaPlayer.Prepared += MediaPlayer_Prepared;
            mediaPlayer.SeekComplete += MediaPlayer_SeekComplete;

            LinearLayout layout = new LinearLayout(Xamarin.Essentials.Platform.AppContext);
            layout.SetGravity(GravityFlags.Center);
            layout.SetForegroundGravity(GravityFlags.Center);
            layout.LayoutChange += Layout_LayoutChange;
            layout.AddView(video);

            return layout;
        }

        private void MediaPlayer_SeekComplete(object sender, EventArgs e)
        {
            seekDelta = TimeSpan.FromSeconds(0);
        }

        private void Layout_LayoutChange(object sender, Android.Views.View.LayoutChangeEventArgs e)
        {
            if (!mediaChanged)
            {
                return;
            }
            mediaChanged = false;
            var scaleX = e.Right * 1.0f / mediaPlayer.VideoWidth;
            var scaleY = e.Bottom * 1.0f / mediaPlayer.VideoHeight;
            var scale = Math.Min(scaleX, scaleY);
            scale = (scale > 0) ? scale : Math.Max(scaleX, scaleY);
            if (video?.LayoutParameters != null)
            {
                video.LayoutParameters.Width = (int)(mediaPlayer.VideoWidth * scale);
                video.LayoutParameters.Height = (int)(mediaPlayer.VideoHeight * scale);
                video.RequestLayout();
            }
        }

        private void MediaPlayer_Prepared(object sender, EventArgs e)
        {
            mediaChanged = true;
            video.RequestLayout();
        }

        public void SetVideoURI(string uri)
        {
            mediaPlayer.Reset();
            mediaPlayer.SetDataSource(Xamarin.Essentials.Platform.AppContext, Android.Net.Uri.Parse(uri));
        }

        public void SetVideoPath(string path)
        {
            mediaPlayer.Reset();
            mediaPlayer.SetDataSource(path);
        }

        public void Start()
        {
            mediaPlayer.Prepare();
            mediaPlayer.Start();
            OnVideoStart?.Invoke();
        }

        public void Stop()
        {
            mediaPlayer.Stop();
            OnVideoStop?.Invoke();
        }

        public void Play()
        {
            mediaPlayer.Start();
            OnVideoPlay?.Invoke();
        }

        public void Pause()
        {
            mediaPlayer.Pause();
            OnVideoPause?.Invoke();
        }

        public void SeekTo(TimeSpan time)
        {
            mediaPlayer.SeekTo((int)time.TotalMilliseconds, MediaPlayerSeekMode.Closest);
        }

        public void SeekForward()
        {
            seekDelta += StepSizeForward;
            var newTime = mediaPlayer.CurrentPosition + (int)seekDelta.TotalMilliseconds;
            mediaPlayer.SeekTo(newTime, MediaPlayerSeekMode.Closest);
        }

        public void SeekBackward()
        {
            seekDelta -= StepSizeBackward;
            var newTime = mediaPlayer.CurrentPosition + (int)seekDelta.TotalMilliseconds;
            mediaPlayer.SeekTo(newTime, MediaPlayerSeekMode.Closest);
        }
        public Xamarin.Forms.Size GetOriginalVideoSize()
        {
            return new Xamarin.Forms.Size(mediaPlayer.VideoWidth, mediaPlayer.VideoHeight);
        }

        public void OnPrepared(MediaPlayer mp)
        {
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            mediaPlayer.SetDisplay(holder);
            mediaPlayer.SyncParams.SetSyncSource((int)AudioSyncSource.SystemClock);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
        }
    }

    public class VideoViewRenderer : FrameLayout, IVisualElementRenderer
    {
        int? defaultLabelFor;
        bool disposed;
        VideoView element;
        VisualElementTracker visualElementTracker;
        VisualElementRenderer visualElementRenderer;
        FragmentManager fragmentManager;
        VideoElementHandler fragment;

        FragmentManager FragmentManager => fragmentManager ??= Context.GetFragmentManager();
        VideoView Element
        {
            get => element;
            set
            {
                if (element == value)
                {
                    return;
                }

                var oldElement = element;
                element = value;
                OnElementChanged(new ElementChangedEventArgs<VideoView>(oldElement, element));
            }
        }

        public VideoViewRenderer(Context context) : base(context)
        {
            visualElementRenderer = new VisualElementRenderer(this);
        }

        void OnElementChanged(ElementChangedEventArgs<VideoView> e)
        {
            VideoElementHandler videoElementHandler = null;

            if (e.OldElement != null)
            {
                e.OldElement.PropertyChanged -= OnElementPropertyChanged;
                fragment.Dispose();
            }
            if (e.NewElement != null)
            {
                this.EnsureId();

                e.NewElement.PropertyChanged += OnElementPropertyChanged;

                ElevationHelper.SetElevation(this, e.NewElement);
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
                e.NewElement.OnGetOriginalVideoSize += videoElementHandler.GetOriginalVideoSize;

                videoElementHandler.OnVideoStart += () => e.NewElement.IsPlaying = videoElementHandler.mediaPlayer.IsPlaying;
                videoElementHandler.OnVideoStop += () => e.NewElement.IsPlaying = videoElementHandler.mediaPlayer.IsPlaying;
                videoElementHandler.OnVideoPlay += () => e.NewElement.IsPlaying = videoElementHandler.mediaPlayer.IsPlaying;
                videoElementHandler.OnVideoPause += () => e.NewElement.IsPlaying = videoElementHandler.mediaPlayer.IsPlaying;
            }

            FragmentManager.BeginTransaction().Replace(Id, fragment = videoElementHandler).Commit();
            ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));
            if (e.NewElement != null)
            {
                e.NewElement.Created();
            }
        }

        void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ElementPropertyChanged?.Invoke(this, e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            fragment.Dispose();
            disposed = true;

            if (disposing)
            {
                SetOnClickListener(null);
                SetOnTouchListener(null);

                if (visualElementTracker != null)
                {
                    visualElementTracker.Dispose();
                    visualElementTracker = null;
                }

                if (visualElementRenderer != null)
                {
                    visualElementRenderer.Dispose();
                    visualElementRenderer = null;
                }

                if (Element != null)
                {
                    Element.PropertyChanged -= OnElementPropertyChanged;

                    if (Platform.GetRenderer(Element) == this)
                    {
                        Platform.SetRenderer(Element, null);
                    }
                }
            }

            base.Dispose(disposing);
        }


        #region IVisualElementRenderer

        public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
        public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;


        VisualElement IVisualElementRenderer.Element => Element;

        VisualElementTracker IVisualElementRenderer.Tracker => visualElementTracker;

        ViewGroup IVisualElementRenderer.ViewGroup => null;

        Android.Views.View IVisualElementRenderer.View => this;

        SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
        {
            Measure(widthConstraint, heightConstraint);
            SizeRequest result = new SizeRequest(new Xamarin.Forms.Size(MeasuredWidth, MeasuredHeight), new Xamarin.Forms.Size(Context.ToPixels(20), Context.ToPixels(20)));
            return result;
        }

        void IVisualElementRenderer.SetElement(VisualElement element)
        {
            if (!(element is VideoView view))
            {
                throw new ArgumentException($"{nameof(element)} must be of type {nameof(VideoView)}");
            }

            if (visualElementTracker == null)
            {
                visualElementTracker = new VisualElementTracker(this);
            }
            Element = view;
        }

        void IVisualElementRenderer.SetLabelFor(int? id)
        {
            if (defaultLabelFor == null)
            {
                defaultLabelFor = LabelFor;
            }
            LabelFor = (int)(id ?? defaultLabelFor);
        }
        
        void IVisualElementRenderer.UpdateLayout() => visualElementTracker?.UpdateLayout();

        #endregion

    }
}

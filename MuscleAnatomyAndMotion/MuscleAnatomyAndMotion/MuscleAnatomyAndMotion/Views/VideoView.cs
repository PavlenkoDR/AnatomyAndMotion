using System;
using Xamarin.Forms;

namespace CrossVideoView
{
    public class VideoView : View
    {
        public delegate void VideoEvent();
        public delegate void VideoSetStringEvent(string uri);
        public delegate void VideoSetTimeEvent(TimeSpan time);
        public delegate Size VideoGetSize();

        public event VideoEvent OnStart;
        public event VideoEvent OnStop;
        public event VideoEvent OnPlay;
        public event VideoEvent OnPause;
        public event VideoSetStringEvent OnSetVideoURI;
        public event VideoSetStringEvent OnSetVideoPath;
        public event VideoSetTimeEvent OnSeekTo;
        public event VideoEvent OnSeekForward;
        public event VideoEvent OnSeekBackward;
        public event VideoGetSize OnGetOriginalVideoSize;
        public bool IsPlaying { get; set; }
        public void Start()
        {
            OnStart?.Invoke();
        }
        public void Stop()
        {
            OnStop?.Invoke();
        }
        public void Play()
        {
            OnPlay?.Invoke();
        }
        public void Pause()
        {
            OnPause?.Invoke();
        }
        public void SetVideoURI(string uri)
        {
            OnSetVideoURI?.Invoke(uri);
        }
        public void SetVideoPath(string path)
        {
            OnSetVideoPath?.Invoke(path);
        }
        public void SeekTo(TimeSpan time)
        {
            OnSeekTo?.Invoke(time);
        }
        public void SeekForward()
        {
            OnSeekForward?.Invoke();
        }
        public void SeekBackward()
        {
            OnSeekBackward?.Invoke();
        }
        public Size GetOriginalVideoSize()
        {
            return OnGetOriginalVideoSize.Invoke();
        }
    }
}

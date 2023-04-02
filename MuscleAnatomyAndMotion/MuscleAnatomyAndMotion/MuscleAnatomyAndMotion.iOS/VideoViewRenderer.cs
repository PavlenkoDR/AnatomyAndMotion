using System;
using AVFoundation;
using CrossVideoView.iOS;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer (typeof(CrossVideoView.VideoView), typeof(CameraPreviewRenderer))]
namespace CrossVideoView.iOS
{
	public class VideoElementHandler : UIView
	{
		public AVPlayer video = new AVPlayer();
		public VideoElementHandler()
		{
			var playerLayer = AVPlayerLayer.FromPlayer(video);
			Layer.AddSublayer(playerLayer);
		}
		public void SetVideoURI(string uri)
		{
			var avasset = AVAsset.FromUrl(NSUrl.FromString(uri));
			var avplayerItem = new AVPlayerItem(avasset);
			video.ReplaceCurrentItemWithPlayerItem(avplayerItem);
		}

		public void SetVideoPath(string path)
		{
			var avasset = AVAsset.FromUrl(NSUrl.FromString(path));
			var avplayerItem = new AVPlayerItem(avasset);
			video.ReplaceCurrentItemWithPlayerItem(avplayerItem);
		}

		public void Start()
		{
			video.Play();
		}
	}

	public class CameraPreviewRenderer : ViewRenderer<CrossVideoView.VideoView, VideoElementHandler>
	{
		VideoElementHandler videoElementHandler;

		protected override void OnElementChanged (ElementChangedEventArgs<CrossVideoView.VideoView> e)
		{
			base.OnElementChanged (e);

			if (e.NewElement != null) {
                if (Control == null)
                {
					videoElementHandler = new VideoElementHandler();
                    SetNativeControl(videoElementHandler);
				}
				e.NewElement.OnStart += videoElementHandler.Start;
				//e.NewElement.OnStop += videoElementHandler.Stop;
				//e.NewElement.OnPlay += videoElementHandler.Play;
				//e.NewElement.OnPause += videoElementHandler.Pause;
				e.NewElement.OnSetVideoPath += videoElementHandler.SetVideoPath;
				e.NewElement.OnSetVideoURI += videoElementHandler.SetVideoURI;
				//e.NewElement.OnSeekTo += videoElementHandler.SeekTo;
				//e.NewElement.OnSeekForward += videoElementHandler.SeekForward;
				//e.NewElement.OnSeekBackward += videoElementHandler.SeekBackward;
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				Control.Dispose ();
			}
			base.Dispose (disposing);
		}
	}
}

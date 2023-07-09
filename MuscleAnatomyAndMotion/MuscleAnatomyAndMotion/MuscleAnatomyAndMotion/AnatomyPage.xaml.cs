using Newtonsoft.Json.Linq;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MuscleAnatomyAndMotion
{
    public partial class AnatomyPage : ContentPage
    {
        public float _ContentScale { get; set; }
        public float ContentScale {
            get {
                if (Device.RuntimePlatform == Device.Android)
                {
                    return _ContentScale;
                }
                return 1;
            }
            set => _ContentScale = value; }
        public float ContentHeight { get
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    return 200;
                }
                return 400;
            }
        }
        private MuscleAsset muscleAsset;
        private int maxLayer;
        public float xOffset { get; set; }

        const long frameSize = TimeSpan.TicksPerSecond / 60;
        List<long> rotateTimings = new List<long>();
        int currentLayer = 0;
        int currentRotate = 0;

        class CanvasData
        {
            public float Opacity { get; set; }
            public string CanvasName { get; set; }
            public string Name { get; set; }
            public MuscleID Target { get; set; }
        }

        SortedDictionary<string, SKBitmap> bitmapsScaled = new SortedDictionary<string, SKBitmap>();
        SortedDictionary<string, SKBitmap> bitmapsOriginal = new SortedDictionary<string, SKBitmap>();
        List<CanvasData> data = new List<CanvasData>();

        public AnatomyPage(float xOffset, float contentScale, BodyPartID id, int maxLayer, List<int> rotateTimingFrames)
        {
            this.xOffset = xOffset;
            this.muscleAsset = App.muscleAssets[id];
            this.maxLayer = maxLayer;
            ContentScale = contentScale;
            for (int i = 0; i < rotateTimingFrames.Count; ++i)
            {
                rotateTimings.Add(frameSize * rotateTimingFrames[i]);
            }
            BindingContext = this;
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var assembly = Application.Current.GetType().Assembly;
            
            Stream stream = await ExternalResourceController.GetInputStream("ru", $"{muscleAsset.video_url}");
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{muscleAsset.video_url}");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            new FileInfo(path).Directory.Create();
            using (var fileStream = File.OpenWrite(path))
            {
                await stream.CopyToAsync(fileStream);
            }

            videoView.SetVideoPath(path);
            videoView.Start();
            await PlayTo(GetNextTimeSpan(), true);
            videoView.Pause();
            SetupCanvasHolder();
        }

        private TimeSpan lastSettedPosition = TimeSpan.FromSeconds(0);

        private Task PlayTo(TimeSpan destination, bool immideately)
        {
            return Task.Run(async () =>
            {
                if (destination > lastSettedPosition && !immideately)
                {
                    videoView.Play();
                    await Task.Delay(destination - lastSettedPosition - TimeSpan.FromMilliseconds(68));
                    videoView.Pause();
                }
                videoView.SeekTo(destination);
                lastSettedPosition = destination;
            });
        }

        private void SetupCanvasHolder()
        {
            data = muscleAsset.layers[currentLayer].sides[currentRotate].areas.Select(x => new CanvasData() { 
                CanvasName = x.area_image_url,
                Name = x.name,
                Target = x.target_id,
                Opacity = 0.0f
            }).ToList();

            Canvas.InvalidateSurface();
        }

        private TimeSpan GetNextTimeSpan()
        {
            return TimeSpan.FromTicks(rotateTimings[currentRotate]) + TimeSpan.FromMilliseconds(1034 * currentLayer);
        }
        bool isCompleted = true;
        private void SetupView(bool immideately)
        {
            if (!isCompleted)
            {
                return;
            }
            isCompleted = false;
            Device.BeginInvokeOnMainThread(async ()=> {
                bitmapsOriginal.Clear();
                bitmapsScaled.Clear();
                data.Clear();
                Canvas.InvalidateSurface();
                infoHolder.Children.Clear();
                var destination = GetNextTimeSpan();
                await PlayTo(destination, immideately).ContinueWith(async (task) =>
                {
                    if (currentRotate == rotateTimings.Count - 1)
                    {
                        currentRotate = 0;
                        destination = GetNextTimeSpan();
                        await PlayTo(destination, true);
                    }
                });
                SetupCanvasHolder();
                await Task.Delay(500);
                isCompleted = true;
            });
        }

        private void AddLayer_Clicked(object sender, EventArgs e)
        {
            currentLayer--;
            currentLayer = Math.Max(0, currentLayer);
            SetupView(true);
        }

        private void RemoveLayer_Clicked(object sender, EventArgs e)
        {
            currentLayer++;
            currentLayer = Math.Min(maxLayer - 1, currentLayer);
            SetupView(true);
        }

        private void Rotate_Clicked(object sender, EventArgs e)
        {
            currentRotate++;
            currentRotate %= rotateTimings.Count;
            SetupView(false);
        }

        Point? lastPoint = null;

        private void TouchEffect_TouchAction(object sender, TouchTracking.TouchActionEventArgs args)
        {
            try
            {
                var view = sender as View;
                if (args.Type == TouchTracking.TouchActionType.Released)
                {
                    foreach (var data in data)
                    {
                        data.Opacity = 0.0f;
                    }

                    if (args.Location.X < 0 || args.Location.Y < 0)
                    {
                        return;
                    }

                    foreach (var bitmap in bitmapsScaled)
                    {
                        var scale = bitmap.Value.Height / view.Height;
                        var xCasted = args.Location.X * scale - (view.Width - bitmap.Value.Width) / 2;
                        var yCasted = args.Location.Y * scale - (view.Height - bitmap.Value.Height) / 2;
                        var pixel = bitmap.Value.GetPixel((int)xCasted, (int)yCasted);
                        if (pixel.Alpha > 0)
                        {
                            foreach (var data in data)
                            {
                                if (data.CanvasName == bitmap.Key)
                                {
                                    data.Opacity = 0.5f;
                                    infoHolder.Children.Clear();
                                    var message = bitmap.Key;

                                    //var kkk = muscleAsset.layers.;

                                    message += $"\nValue: {data.CanvasName}";
                                    message += $"\nIndex: {data.Name}";
                                    message += $"\nMuscle: {data.Target}";

                                    var redirectView = new MuscleInfoRedirect(message, data.Target);
                                    infoHolder.Children.Add(redirectView);
                                    redirectView.TranslationX = Math.Min(args.Location.X + CanvasHolder.X, infoHolder.Width - redirectView.Width - 80.0);
                                    redirectView.TranslationY = args.Location.Y + CanvasHolder.Y + 30.0;
                                    _ = Task.Delay(4000).ContinueWith((task) =>
                                    {
                                        Device.BeginInvokeOnMainThread(() =>
                                        {
                                            _ = infoHolder.Children.Remove(redirectView);
                                        });
                                    });
                                    lastPoint = null;
                                    break;
                                }
                            }
                            break;
                        }
                        else
                        {
                            lastPoint = new Point(xCasted, yCasted);
                        }
                    }
                    Canvas.InvalidateSurface();
                }
            }
            catch (Exception ex)
            {
                debugLabel.Text = "Touch " + ex.Message;
            }
        }

        private void SKCanvasView_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            try
            {
                SKImageInfo info = e.Info;
                SKSurface surface = e.Surface;
                SKCanvas canvas = surface.Canvas;
                SKCanvasView view = sender as SKCanvasView;

                canvas.Clear();

                if (lastPoint.HasValue)
                {
                    canvas.DrawCircle((float)lastPoint.Value.X, (float)lastPoint.Value.Y, 5, new SKPaint() { Color = SKColor.Parse("#ff0000") });
                    lastPoint = null;
                    return;
                }

                foreach (var data in data)
                {
                    try
                    {
                        var path = data.CanvasName;
                        var assembly = Application.Current.GetType().Assembly;
                        Stream stream = ExternalResourceController.GetInputStream("ru", $"{path}").Result;

                        SKBitmap bitmap = null;
                        bool needResize = false;

                        if (bitmapsOriginal.ContainsKey(path))
                        {
                            bitmap = bitmapsOriginal[path];
                        }
                        else
                        {
                            bitmap = SKBitmap.Decode(stream);
                            needResize = true;
                        }

                        var scale = (float)view.Height * 1.0f / bitmap.Height;
                        var width = bitmap.Width * scale;
                        var height = bitmap.Height * scale;

                        if (needResize)
                        {
                            bitmapsOriginal.Add(path, bitmap.Copy());
                            SKImageInfo bitmapInfo = new SKImageInfo((int)width, (int)height);
                            bitmap = bitmap.Resize(bitmapInfo, SKFilterQuality.High);
                            bitmapsScaled.Add(path, bitmap);
                        }

                        if (data.Opacity + HighlightOpacity > 0.0)
                        {
                            var scaleInfo = info.Height * 1.0f / height;
                            var rect = new SKRect();
                            rect.Left = ((float)view.Width - width) * ContentScale / 2.0f;
                            rect.Top = ((float)view.Height - height) * ContentScale / 2.0f;
                            rect.Right = info.Width * 1.0f - rect.Left;
                            rect.Bottom = info.Height * 1.0f - rect.Top;
                            /*
                            rect.Left = (view.Width - width) / 2;
                            rect.Top = (view.Height - height) / 2;
                            rect.Right = width + rect.Left;
                            rect.Bottom = height + rect.Top;
                             */

                            canvas.DrawBitmap(bitmap, rect, new SKPaint() { Color = new SKColor(0, 0, 0, (byte)(128 + HighlightOpacity)) });
                        }
                    }
                    catch (Exception ex)
                    {
                        debugLabel.Text = "Paint " + ex.Message + "\n" + ex.StackTrace;
                        canvas.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                int kkk = 0;
            }
        }

        private void SeekBackward_Clicked(object sender, EventArgs e)
        {
            videoView.SeekBackward();
        }

        private void SeekForward_Clicked(object sender, EventArgs e)
        {
            videoView.SeekForward();
        }

        byte HighlightOpacity = 0;
        private void Highlight_Clicked(object sender, EventArgs e)
        {
            HighlightOpacity += 64;
            HighlightOpacity %= 128;

            Canvas.InvalidateSurface();
        }
    }
}

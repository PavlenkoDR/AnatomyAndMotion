using MuscleAnatomyAndMotion.Controllers;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MuscleAnatomyAndMotion
{
    public partial class AnatomyPage : ContentPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;
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
        public bool isDownloaded
        {
            get => WebResourceController.downloadedData.bodyPartIDs.Contains(muscleAsset.id);
            set
            {
                Task.Run(() => {
                    Device.BeginInvokeOnMainThread(async () => {
                        if (value)
                        {
                            if (!WebResourceController.downloadedData.bodyPartIDs.Contains(muscleAsset.id))
                            {
                                var loadingBanner = new LoadingBanner();
                                if (ResourceController.IsOffline)
                                {
                                    loadingBanner.Progress = $"Загрузка и сохранение\nПерезагрузите страницу после завершения";
                                }
                                else
                                {
                                    loadingBanner.Progress = $"Загрузка и сохранение";
                                }
                                await Application.Current.MainPage.Navigation.PushModalAsync(loadingBanner);
                                WebResourceController.downloadedData.bodyPartIDs.Add(muscleAsset.id);
                                await Application.Current.MainPage.Navigation.PopModalAsync();
                            }
                        }
                        else
                        {
                            var loadingBanner = new LoadingBanner();
                            loadingBanner.Progress = $"Удаление из сохраненных";
                            await Application.Current.MainPage.Navigation.PushModalAsync(loadingBanner);
                            WebResourceController.downloadedData.bodyPartIDs.Remove(muscleAsset.id);
                            await Application.Current.MainPage.Navigation.PopModalAsync();
                        }
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("isDownloaded"));
                    });
                    Task.Yield();
                });
            }
        }
        private MuscleAsset muscleAsset;
        public float xOffset { get; set; }

        const long frameSize = TimeSpan.TicksPerSecond / 60;
        List<long> rotateTimings = new List<long>();
        int currentLayer = 0;
        int currentRotate = 0;

        class CanvasData
        {
            public string CanvasName { get; set; }
            public float Opacity { get; set; }
            public string Name { get; set; }
            public MuscleID Target { get; set; }
        }

        SortedDictionary<string, SKBitmap> bitmapsScaled = new SortedDictionary<string, SKBitmap>();
        SortedDictionary<string, SKBitmap> bitmapsOriginal = new SortedDictionary<string, SKBitmap>();
        SortedDictionary<string, CanvasData> data = new SortedDictionary<string, CanvasData>();

        public AnatomyPage(float xOffset, float contentScale, BodyPartID id, List<int> rotateTimingFrames)
        {
            this.xOffset = xOffset;
            this.muscleAsset = MuscleDictionary.GetCurrent().muscleAssets[id];
            ContentScale = contentScale;
            for (int i = 0; i < rotateTimingFrames.Count; ++i)
            {
                rotateTimings.Add(frameSize * rotateTimingFrames[i]);
            }
            BindingContext = this;
            InitializeComponent();
        }
        private string videoPath;
        private bool isOnAppearingBusy = false;
        private bool needSetupVideoHolder = false;
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (isOnAppearingBusy)
            {
                return;
            }
            isOnAppearingBusy = true;

            var assembly = Application.Current.GetType().Assembly;
            Stream stream = await ResourceController.GetInputStream("ru", $"{muscleAsset.video_url}");
            videoPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{muscleAsset.video_url}");

            if (WebResourceController.IsFileDownloaded("ru", $"{muscleAsset.video_url}"))
            {
                videoPath = (stream as FileStream).Name;
            }
            else if (File.Exists(videoPath))
            {
                File.Delete(videoPath);
                new FileInfo(videoPath).Directory.Create();
                using (var fileStream = File.OpenWrite(videoPath))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
            if (needSetupVideoHolder)
            {
                SetupVideoHolder();
                SetupVideo();
            }
            isOnAppearingBusy = false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            needSetupVideoHolder = true;
        }

        private double approvedContentScale;

        private async void SetupVideoHolder()
        {
            await videoView.StartVideoLoad().ContinueWith(x => {
                Device.BeginInvokeOnMainThread(() =>
                {
                    var originalVideoSize = videoView.GetOriginalVideoSize();
                    approvedContentScale = (Width > Height ? 0.5  * Width / Height : 1.0) * ContentScale * Math.Min(BaseContentHolder.Height, BaseContentHolder.Width) / originalVideoSize.Height;
                    ContentHolder.HeightRequest = approvedContentScale * BaseContentHolder.Height;
                    ContentHolder.WidthRequest = approvedContentScale * originalVideoSize.Width * BaseContentHolder.Height / originalVideoSize.Height;
                    ContentHolder.TranslationX = (Width - ContentHolder.WidthRequest) * 0.5;
                    ContentHolder.TranslationY = (Height - ContentHolder.HeightRequest) * 0.5;
                });
            });
            SetupVideo();
        }

        private async void SetupVideo()
        {
            videoView.SetVideoPath(videoPath);
            videoView.Start();
            await PlayTo(GetNextTimeSpan(), true);
            videoView.Pause();
        }

        private void CanvasHolder_SizeChanged(object sender, EventArgs e)
        {
            SetupVideoHolder();
            SetupVideo();
            SetupCanvasHolder();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width > height)
            {
                mainStack.Orientation = StackOrientation.Horizontal;
                buttonStack.Orientation = StackOrientation.Vertical;
            }
            else
            {
                mainStack.Orientation = StackOrientation.Vertical;
                buttonStack.Orientation = StackOrientation.Horizontal;
            }
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

        private void SetupCanvasHolder(bool immideately = true)
        {
            if (currentLayer >= muscleAsset.layers.Count)
            {
                return;
            }
            data = new SortedDictionary<string, CanvasData>(muscleAsset.layers[currentLayer].sides[currentRotate % muscleAsset.layers[currentLayer].sides.Count].areas.ToDictionary(x => x.area_image_url, x => new CanvasData()
            {
                CanvasName = x.area_image_url,
                Name = x.name,
                Target = x.target_id,
                Opacity = 0.0f
            }));
            
            SetupView(immideately);
        }

        private TimeSpan GetNextTimeSpan()
        {
            return TimeSpan.FromTicks(rotateTimings[currentRotate]) + TimeSpan.FromMilliseconds(1034 * currentLayer);
        }
        private void SetupView(bool immideately)
        {
            Task.Run(() => {
                while (!isInited)
                {
                    Task.Yield();
                }
            }).ContinueWith(_ => {
                Device.BeginInvokeOnMainThread(async () => {
                    bitmapsOriginal.Clear();
                    bitmapsScaled.Clear();
                    infoHolder.Children.Clear();
                    var destination = GetNextTimeSpan();
                    await PlayTo(destination, immideately).ContinueWith(async __ =>
                    {
                        if (currentRotate == rotateTimings.Count - 1)
                        {
                            currentRotate = 0;
                            destination = GetNextTimeSpan();
                            await PlayTo(destination, true);
                        }
                    });

                    lastPoint = null;
                    CurrentCanvas.Source = "";
                    Canvas.InvalidateSurface();

                    foreach (var data in data)
                    {
                        var path = data.Key;
                        if (!bitmapsOriginal.ContainsKey(path))
                        {
                            SKBitmap bitmap = null;
                            int tryCount = 0;
                            while (bitmap == null)
                            {
                                if (++tryCount == 3)
                                {
                                    break;
                                }
                                var assembly = Application.Current.GetType().Assembly;
                                Stream stream = ResourceController.GetInputStream("ru", $"{path}").Result;
                                bitmap = SKBitmap.Decode(stream);
                                if (bitmap == null)
                                {
                                    if (!InternalResourceController.HaveManifestResource("ru", $"{path}"))
                                    {
                                        WebResourceController.GetDownloadedFilePath("ru", $"{path}", true);
                                    }
                                }
                            }
                            if (bitmap == null)
                            {
                                continue;
                            }

                            var scale = (float)ContentHolder.Height * 1.0f / bitmap.Height;
                            var width = bitmap.Width * scale;
                            var height = bitmap.Height * scale;

                            bitmapsOriginal.Add(path, bitmap.Copy());
                            SKImageInfo bitmapInfo = new SKImageInfo((int)width, (int)height);
                            bitmap = bitmap.Resize(bitmapInfo, SKFilterQuality.High);
                            bitmapsScaled.Add(path, bitmap);
                        }
                    }
                });
            });
        }

        private void AddLayer_Clicked(object sender, EventArgs e)
        {
            CurrentCanvas.Source = "";
            currentLayer--;
            currentLayer = Math.Max(0, currentLayer);
            SetupCanvasHolder(true);
        }

        private void RemoveLayer_Clicked(object sender, EventArgs e)
        {
            CurrentCanvas.Source = "";
            currentLayer++;
            currentLayer = Math.Min(muscleAsset.layers.Count - 1, currentLayer);
            SetupCanvasHolder(true);
        }

        private void Rotate_Clicked(object sender, EventArgs e)
        {
            CurrentCanvas.Source = "";
            currentRotate++;
            currentRotate %= muscleAsset.layers[currentLayer].sides.Count + 1;
            SetupCanvasHolder(false);
        }

        Point? lastPoint = null;
        bool isBusy = false;

        private void TouchEffect_TouchAction(object sender, TouchTracking.TouchActionEventArgs args)
        {
            if (isBusy)
            {
                return;
            }
            isBusy = true;
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var view = sender as View;
                    if (args.Type == TouchTracking.TouchActionType.Released)
                    {
                        foreach (var data in data)
                        {
                            data.Value.Opacity = 0.0f;
                        }

                        if (args.Location.X < 0 || args.Location.Y < 0)
                        {
                            return;
                        }

                        foreach (var bitmap in bitmapsScaled)
                        {
                            var xCasted = (args.Location.X);
                            var yCasted = (args.Location.Y);
                            var pixel = bitmap.Value.GetPixel((int)xCasted, (int)yCasted);
                            if (pixel.Alpha > 0)
                            {
                                if (data.TryGetValue(bitmap.Key, out var scaleData))
                                {
                                    if (scaleData.CanvasName == bitmap.Key)
                                    {
                                        lastPoint = null;
                                        scaleData.Opacity = 0.5f;
                                        infoHolder.Children.Clear();
                                        var message = "";

                                        //var kkk = muscleAsset.layers.;

                                        message += $"Index: {scaleData.Name}";
                                        message += $"\nMuscle: {scaleData.Target}";

                                        var redirectView = new MuscleInfoRedirect() { Description = message, Link = scaleData.Target };
                                        infoHolder.Children.Add(redirectView);
                                        if (Height > Width)
                                        {
                                            redirectView.TranslationX = Width * 0.5 - redirectView.Width * 0.5;
                                            redirectView.TranslationY = args.Location.Y + ContentHolder.TranslationY;
                                            if (redirectView.TranslationY > Height * 0.5)
                                            {
                                                redirectView.TranslationY -= redirectView.Height * 1.5;
                                            }
                                            else
                                            {
                                                redirectView.TranslationY += redirectView.Height * 0.5;
                                            }
                                        }
                                        else
                                        {
                                            redirectView.TranslationY = Height * 0.5 - redirectView.Height * 0.5;
                                            redirectView.TranslationX = args.Location.X + ContentHolder.TranslationX;
                                            if (redirectView.TranslationX > Width * 0.5)
                                            {
                                                redirectView.TranslationX -= redirectView.Width * 1.5;
                                            }
                                            else
                                            {
                                                redirectView.TranslationX += redirectView.Width * 0.5;
                                            }
                                        }
                                        _ = Task.Delay(4000).ContinueWith((task) =>
                                        {
                                            Device.BeginInvokeOnMainThread(() =>
                                            {
                                                _ = infoHolder.Children.Remove(redirectView);
                                            });
                                        });
                                        if (CurrentCanvas.ImgSourceFromObb != bitmap.Key)
                                        {
                                            CurrentCanvas.ImgSourceFromObb = bitmap.Key;
                                        }
                                        break;
                                    }
                                }
                                break;
                            }
                            else
                            {
                                lastPoint = new Point(xCasted * 2.0, yCasted * 2.0);
                            }
                        }
                        if (lastPoint != null)
                        {
                            CurrentCanvas.Source = "";
                        }
                        Canvas.InvalidateSurface();
                    }
                }
                catch (Exception ex)
                {
                    debugLabel.Text = "Touch " + ex.Message;
                }
                isBusy = false;
            });
        }

        private void SKCanvasView_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            canvas.Clear();

            if (lastPoint.HasValue)
            {
                canvas.DrawCircle((float)lastPoint.Value.X, (float)lastPoint.Value.Y, 5, new SKPaint() { Color = SKColor.Parse("#ff0000") });
                lastPoint = null;
                return;
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
        }

        private bool isInited = false;
        private async void videoView_OnCreated()
        {
            isInited = false;
            await Device.InvokeOnMainThreadAsync(async () => {
                LoadingBanner loadingBanner = null;
                if (!ResourceController.IsOffline)
                {
                    loadingBanner = new LoadingBanner();
                    loadingBanner.Progress = $"Загрузка модели";
                    await Navigation.PushModalAsync(loadingBanner);
                }
                SetupVideo();
                SetupVideoHolder();
                var areasCount = muscleAsset.layers
                .Select(x => x.sides.Select(y => y.areas.Count).ToList()).ToList()
                .Aggregate((x, y) => x.AddRangeSequence(y))
                .Aggregate((x, y) => x + y);
                if (!ResourceController.IsOffline)
                {
                    int progress = 0;
                    for (int i = 0; i < muscleAsset.layers.Count; ++i)
                    {
                        var layer = muscleAsset.layers[i];
                        for (int j = 0; j < layer.sides.Count; ++j)
                        {
                            var side = layer.sides[j];
                            for (int k = 0; k < side.areas.Count; ++k)
                            {
                                var area = side.areas[k];
                                await Task.Run(()=> {
                                    Device.BeginInvokeOnMainThread(() => {
                                        loadingBanner.Progress = $"Загрузка фрагментов {++progress}/{areasCount}";
                                    });
                                    if (!InternalResourceController.HaveManifestResource("ru", $"{area.area_image_url}"))
                                    {
                                        WebResourceController.GetDownloadedFilePath("ru", $"{area.area_image_url}");
                                    }
                                });
                            }
                        }
                    }
                    await Navigation.PopModalAsync();
                    loadingBanner = null;
                }
                isInited = true;
                SetupCanvasHolder();
            });
        }
    }
}

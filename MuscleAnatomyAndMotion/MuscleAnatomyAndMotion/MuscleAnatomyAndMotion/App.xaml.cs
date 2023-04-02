using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    public partial class App : Application
    {
        public class LinkInfo
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public int Index { get; set; }
        }
        public static SortedDictionary<int, MuscleFullInfo> muscleFullInfos = new SortedDictionary<int, MuscleFullInfo>();
        public static List<LinkInfo> links = new List<LinkInfo>();
        //
        // Сводка:
        //     Occurs when an item is added, removed, changed, moved, or the entire list is
        //     refreshed.
        public static event EventHandler CollectionChanged;
        public App()
        {
            ReadMuscleFullInfos();
            InitializeComponent();

            MainPage = new MainFlyoutPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private ImageSource getImageSourceFromMovementInfo(string info)
        {
            if (info == null)
            {
                return null;
            }
            var path = $"MuscleAnatomyAndMotion.Assets._{info.Replace("/", ".")}";
            var assembly = Application.Current.GetType().Assembly;
            var fileNames = assembly.GetManifestResourceNames().ToList();
            if (!fileNames.Contains(path))
            {
                return null;
            }
            var source = ImageSource.FromResource(path);
            return source;
        }

        private void ReadMuscleFullInfos()
        {
            Task.Run(async () => {
                muscleFullInfos.Clear();
                var assembly = Current.GetType().Assembly;
                var fileNames = assembly.GetManifestResourceNames().ToList();
                {
                    var stream = assembly.GetManifestResourceStream("MuscleAnatomyAndMotion.Assets.links.json");
                    var reader = new StreamReader(stream);
                    var json = await reader.ReadToEndAsync();
                    links = JsonConvert.DeserializeObject<List<LinkInfo>>(json);
                }
                for (int i = 1; i <= 95; i++)
                {
                    var path = $"MuscleAnatomyAndMotion.Assets.{i}.json";
                    if (fileNames.Contains(path))
                    {
                        var stream = assembly.GetManifestResourceStream(path);
                        var reader = new StreamReader(stream);
                        var json = await reader.ReadToEndAsync();
                        var obj = JsonConvert.DeserializeObject<MuscleFullInfo>(json);

                        obj.mainVideos.ForEach(x =>
                        {
                            var movement = x.movements.Count > 0 ? x.movements.First() : null;
                            var MuscleImagePath = movement?.imageUrls.Count > 0 ? movement?.imageUrls[0] : null;
                            var MuscleImageCoverPath = movement?.imageUrls.Count > 1 ? movement?.imageUrls[1] : null;
                            x.imageDescription = movement?.imageDescription;
                            x.MuscleImage = getImageSourceFromMovementInfo(MuscleImagePath);
                            x.MuscleImageCover = getImageSourceFromMovementInfo(MuscleImageCoverPath);
                        });

                        muscleFullInfos.Add(i, obj);
                    }
                }
            }).ContinueWith(x => {
                Device.BeginInvokeOnMainThread(() => {
                    CollectionChanged.Invoke(this, new EventArgs());
                });
            });
        }
    }
}

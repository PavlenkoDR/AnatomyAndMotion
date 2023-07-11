using MuscleAnatomyAndMotion.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            MainPage = new LoadingScreenPage();

            //string cachePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "filesCache.json");
            //if (File.Exists(cachePath))
            //{
            //    var jsonCache = File.ReadAllText(cachePath);
            //    var filesCache = JsonConvert.DeserializeObject<List<string>>(jsonCache);
            //    foreach (var file in filesCache)
            //    {
            //        File.Delete(file);
            //    }
            //    File.Delete(cachePath);
            //}
        }

        public void ManualCleanUp()
        {
            CleanUp();
            OnResume();
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
    }
}

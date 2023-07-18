using MuscleAnatomyAndMotion.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingScreenPage : ContentPage
    {
        public LoadingScreenPage()
        {
            InitializeComponent();
            RunLoad();
            LocalFilesController.Init();
        }

        private void RunLoad()
        {
            Task.Run(async () => {
                var loadTask = MuscleDictionary.Init();
                while (!loadTask.IsCompleted)
                {
                    Device.BeginInvokeOnMainThread(() => {
                        loadingProgress.Text = string.Format("{0:P1}", MuscleDictionary.GetProgress());
                    });
                    await Task.Delay(200);
                    await Task.Yield();
                }
                Device.BeginInvokeOnMainThread(() => {
                    Application.Current.MainPage = new MainFlyoutPage();
                });
            });
        }

    }
}
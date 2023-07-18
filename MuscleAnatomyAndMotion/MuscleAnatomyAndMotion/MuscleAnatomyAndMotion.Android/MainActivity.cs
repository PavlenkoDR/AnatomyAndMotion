using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Google.Android.Material.Tabs;
using System.IO;
using Google.Android.Vending.Expansion.ZipFile;
using Xamarin.Essentials;
using System.Threading.Tasks;
using Android.Content;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Java.IO;
using System.Drawing;
using Android.Graphics.Drawables;
using Android.Graphics;
using MuscleAnatomyAndMotion.Controllers;
using Firebase.Messaging;
using Firebase.Iid;
using Android.Support.V4.App;
using Android.Util;
using AndroidX.Core.App;
using Firebase.RemoteConfig;
using Java.Util.Concurrent.Locks;

[assembly: Xamarin.Forms.Dependency(typeof(MuscleAnatomyAndMotion.Droid.ExternalResourceReader))]
[assembly: Xamarin.Forms.Dependency(typeof(MuscleAnatomyAndMotion.Droid.IntentController))]
[assembly: Xamarin.Forms.Dependency(typeof(MuscleAnatomyAndMotion.Droid.UpdateController))]
[assembly: Xamarin.Forms.Dependency(typeof(MuscleAnatomyAndMotion.Droid.MyRemoteConfigurationService))]
namespace MuscleAnatomyAndMotion.Droid
{
    [Activity(Label = "MuscleAnatomyAndMotion", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        internal static readonly string CHANNEL_ID = "fb_notification_channel";
        internal static readonly int NOTIFICATION_ID = 100;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder()
                 .DetectLeakedClosableObjects()
                 .PenaltyLog();
            StrictMode.SetVmPolicy(builder.Build());
            builder.DetectFileUriExposure();
            CreateNotificationChannel();

            try
            {
                {
                    FirebaseRemoteConfigSettings configSettings = new FirebaseRemoteConfigSettings.Builder().Build();
                    var waiter = FirebaseRemoteConfig.Instance.SetConfigSettingsAsync(configSettings);
                    lock (waiter)
                    {
                        while(waiter.IsComplete == false && waiter.IsCanceled == false) { }
                    }
                }
                {
                    var waiter = FirebaseRemoteConfig.Instance.SetDefaultsAsync(Resource.Xml.remote_config_defaults);
                    lock (waiter)
                    {
                        while (waiter.IsComplete == false && waiter.IsCanceled == false) { }
                    }
                }
            }
            finally
            {
            }
            //string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            //var context = Application.Context;
            //var packageInfo = context.PackageManager.GetPackageInfo(context.PackageName, 0);
            //var expansionFile = APKExpansionSupport.GetAPKExpansionZipFile(context, context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionCode, 0);


            //int fff = 0;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var channel = new NotificationChannel(CHANNEL_ID,
                                                  "FCM Notifications",
                                                  NotificationImportance.Default)
            {

                Description = "Firebase Cloud Messages appear in this channel"
            };

            var notificationManager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }

    public class UpdateController : IUpdateController
    {
        public string getCurrentVersion()
        {
            return Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionCode.ToString();
        }
    }

    public class MyRemoteConfigurationService : IRemoteConfigurationService
    {
        public MyRemoteConfigurationService()
        {
            Run();
        }

        private async void Run()
        {
            await FetchAndActivateAsync();
        }

        public async Task FetchAndActivateAsync()
        {
            long cacheExpiration = 60 * 60;
            try
            {
                await FirebaseRemoteConfig.Instance.FetchAsync(cacheExpiration);
                FirebaseRemoteConfig.Instance.Activate().Wait();
            }
            catch
            {
            }
        }

        public async Task<TInput> GetAsync<TInput>(string key)
        {
            var settings = FirebaseRemoteConfig.Instance.GetString(key);
            return await Task.FromResult(JsonConvert.DeserializeObject<TInput>(settings));
        }

        public async Task<string> GetAsync(string key)
        {
            return FirebaseRemoteConfig.Instance.GetString(key);
        }
    }

    public class ExternalResourceReader : IExternalResourceReader
    {
        Task initTask = null;
        private ZipResourceFile expansionFile = null;
        private async Task ReadExpansionFile()
        {

            if (await Permissions.CheckStatusAsync<Permissions.StorageRead>() != PermissionStatus.Granted &&
                await Permissions.RequestAsync<Permissions.StorageRead>() != PermissionStatus.Granted)
            {
                return;
            }

            if (await Permissions.CheckStatusAsync<Permissions.StorageWrite>() != PermissionStatus.Granted &&
                await Permissions.RequestAsync<Permissions.StorageWrite>() != PermissionStatus.Granted)
            {
                return;
            }
            if (await Permissions.CheckStatusAsync<Permissions.Media>() != PermissionStatus.Granted &&
                await Permissions.RequestAsync<Permissions.Media>() != PermissionStatus.Granted)
            {
                return;
            }
            var root = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            var directoryPath = System.IO.Path.Combine(root, "Android/obb");
            var obbDirs = Application.Context.GetObbDirs().Select(x => x.AbsolutePath).ToList();
            obbDirs.Add(directoryPath);
            
            foreach (var obbPath in obbDirs)
            {
                var obbFilePath = System.IO.Path.Combine(obbPath, "main.1.com.companyname.muscleanatomyandmotion.obb");
                if (System.IO.File.Exists(obbFilePath))
                {
                    expansionFile = APKExpansionSupport.GetResourceZipFile(new[] { obbFilePath });
                }
                if (expansionFile != null)
                {
                    break;
                }
            }
        }
        public ExternalResourceReader()
        {
            initTask = ReadExpansionFile();
        }

        public Stream GetInputStream(string path)
        {
            return expansionFile?.GetInputStream(path); // en/training/1301.json
        }

        public Task GetInitTask()
        {
            return initTask;
        }

        public List<string> GetObbDirs()
        {
            var root = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            var directoryPath = System.IO.Path.Combine(root, "Android/obb");
            var obbDirs = Application.Context.GetObbDirs().Select(x => x.AbsolutePath).ToList();
            obbDirs.Add(directoryPath);
            return obbDirs.ToList();
        }
    }

    public class IntentController : Controllers.IIntentController
    {
        List<string> filesCache = new List<string>();
        public void ShareMediaFile(string text, List<string> filesPath)
        {
            for (int i = 0; i < filesPath.Count; ++i)
            {
                var pathRow = filesPath[i];
                if (System.IO.File.Exists(pathRow))
                {
                    continue;
                }
                string path = System.IO.Path.Combine(System.IO.Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim).AbsolutePath, "Camera"), $"{pathRow}");
                if (System.IO.File.Exists(path))
                {
                    filesPath[i] = path;
                    continue;
                }
                Stream stream = ResourceController.GetInputStream("ru", $"{pathRow}").Result;
                new FileInfo(path).Directory.Create();
                //FileOutputStream fileOutputStream = new FileOutputStream(new Java.IO.File(path));
                //var bytes = ((MemoryStream)stream).ToArray();
                using (var fileStream = System.IO.File.OpenWrite(path))
                {
                    stream.CopyTo(fileStream);
                    fileStream.Close();
                }
                //fileOutputStream.Write(bytes);
                //fileOutputStream.Close();
                filesPath[i] = path;
                WebResourceController.downloadedData.cache.Add(path);

                var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                mediaScanIntent.SetData(Android.Net.Uri.FromFile(new Java.IO.File(path)));
                Application.Context.SendBroadcast(mediaScanIntent);
            }
            if (Permissions.CheckStatusAsync<Permissions.Media>().Result != PermissionStatus.Granted &&
                Permissions.RequestAsync<Permissions.Media>().Result != PermissionStatus.Granted)
            {
                return;
            }

            string intentType = "";
            if (filesPath.Where(x => System.IO.Path.GetExtension(x) == ".mp4").Count() > 0)
            {
                intentType = "video/mp4";
            }
            else if (filesPath.Where(x => System.IO.Path.GetExtension(x) == ".mp3").Count() > 0)
            {
                intentType = "music/mp3";
            }
            else if (filesPath.Where(x => System.IO.Path.GetExtension(x) == ".png").Count() > 0)
            {
                intentType = "image/png";
            }
            else if (filesPath.Where(x => System.IO.Path.GetExtension(x) == ".jpg").Count() > 0)
            {
                intentType = "image/jpg";
            }
            else
            {
                intentType = "*/*";
            }

            if (filesPath.Count == 1)
            {
                var drawable = BitmapFactory.DecodeFile(filesPath[0]);
                // Media Share one file
                var file = new Java.IO.File(filesPath[0]);
                var uri = Android.Net.Uri.FromFile(file);

                var intent = new Intent(Intent.ActionSend);
                intent.SetType(intentType);
                intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                intent.PutExtra(Intent.ExtraText, text);
                intent.PutExtra(Intent.ExtraStream, uri);

                var chooserIntent = Intent.CreateChooser(intent, text);
                chooserIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                Platform.AppContext.StartActivity(chooserIntent);
            }
            else
            {
                // Media Share

                var intent = new Intent(Intent.ActionSendMultiple);
                intent.SetType(intentType);
                intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                var uris = new List<IParcelable>();
                foreach (var filePath in filesPath)
                {
                    var file = new Java.IO.File(filePath);
                    var uri = Android.Net.Uri.FromFile(file);
                    uris.Add(uri);
                }
                intent.PutExtra(Intent.ExtraText, text);
                intent.PutParcelableArrayListExtra(Intent.ExtraStream, uris);

                var chooserIntent = Intent.CreateChooser(intent, text);
                chooserIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                Platform.AppContext.StartActivity(chooserIntent);
            }
        }
    }
}
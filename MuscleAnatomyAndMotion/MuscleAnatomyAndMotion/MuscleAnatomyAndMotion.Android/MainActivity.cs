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

[assembly: Xamarin.Forms.Dependency(typeof(MuscleAnatomyAndMotion.Droid.ExternalResourceReader))]
[assembly: Xamarin.Forms.Dependency(typeof(MuscleAnatomyAndMotion.Droid.IntentController))]
namespace MuscleAnatomyAndMotion.Droid
{
    [Activity(Label = "MuscleAnatomyAndMotion", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

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

            var expansionFilePath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Android/obb/main.1.com.companyname.muscleanatomyandmotion.obb");
            expansionFile = APKExpansionSupport.GetResourceZipFile(new[] { expansionFilePath });
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
    }

    public class IntentController : Controllers.IIntentController
    {
        private static string cachePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "filesCache.json");
        List<string> filesCache = new List<string>();
        public void ShareMediaFile(string text, List<string> filesPath)
        {
            if (System.IO.File.Exists(cachePath))
            {
                var jsonCache = System.IO.File.ReadAllText(cachePath);
                filesCache = JsonConvert.DeserializeObject<List<string>>(jsonCache);
            }
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
                Stream stream = ExternalResourceController.GetInputStream("ru", $"{pathRow}").Result;
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

                var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                mediaScanIntent.SetData(Android.Net.Uri.FromFile(new Java.IO.File(path)));
                Application.Context.SendBroadcast(mediaScanIntent);
            }
            filesCache.AddRange(filesPath);
            System.IO.File.WriteAllText(cachePath, JsonConvert.SerializeObject(filesCache));
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
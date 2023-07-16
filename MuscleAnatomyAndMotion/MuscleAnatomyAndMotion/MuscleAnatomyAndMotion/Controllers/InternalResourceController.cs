using MuscleAnatomyAndMotion.Controllers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    public static class InternalResourceController
    {
        private class StreamData
        {
            public Stream enStream;
            public Stream langStream;
        }
        private static StreamData GetStreamData(string lang, string path)
        {
            if (path == null)
            {
                return null;
            }
            Path.ChangeExtension(path, "");
            var localPath = $"{Path.ChangeExtension(path, "").Replace("/", ".")}{Path.GetExtension(path).Replace(".", "")}";
            var assembly = Application.Current.GetType().Assembly;
            var ffffff = assembly.GetManifestResourceNames();

            var mode = ResourceController.IsOffline ? "Offline" : "Online";
            Stream langStream = assembly.GetManifestResourceStream($"MuscleAnatomyAndMotion.Assets.{mode}.{lang}.{localPath}");
            Stream enStream;
            if (langStream != null)
            {
                enStream = langStream;
            }
            else
            {
                enStream = assembly.GetManifestResourceStream($"MuscleAnatomyAndMotion.Assets.Offline.en.{localPath}");
            }
            return new StreamData() { enStream = enStream, langStream = langStream };
        }
        public static bool HaveManifestResource(string lang, string path)
        {
            if (path == null)
            {
                return false;
            }
            Path.ChangeExtension(path, "");
            var localPath = $"{Path.ChangeExtension(path, "").Replace("/", ".")}{Path.GetExtension(path).Replace(".", "")}";
            var assembly = Application.Current.GetType().Assembly;

            var mode = ResourceController.IsOffline ? "Offline" : "Online";
            var completedPath = $"MuscleAnatomyAndMotion.Assets.{mode}.{lang}.{localPath}";
            
            var recources = assembly.GetManifestResourceNames().ToList();
            return recources.Contains(completedPath);
        }
        public static string ReadString(string lang, string path)
        {
            var streamData = GetStreamData(lang, path);
            return ResourceReader.ReadString(streamData?.enStream, streamData?.langStream);
        }
        public static Stream GetInputStream(string lang, string path)
        {
            var streamData = GetStreamData(lang, path);
            return ResourceReader.ResolveInputStream(streamData?.enStream, streamData?.langStream);
        }
        public static ImageSource GetImageSource(string lang, string path)
        {
            var streamDataCheck = GetStreamData(lang, path);
            var stream = streamDataCheck.langStream ?? streamDataCheck.enStream;
            return stream == null ? null : ImageSource.FromStream(() => {
                var streamData = GetStreamData(lang, path);
                return streamData.langStream ?? streamData.enStream;
            });
        }
    }
}

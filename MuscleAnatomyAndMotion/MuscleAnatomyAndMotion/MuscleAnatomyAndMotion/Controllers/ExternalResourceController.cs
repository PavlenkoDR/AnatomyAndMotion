using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    public interface IExternalResourceReader
    {
        Task GetInitTask();
        Stream GetInputStream(string path);
        List<string> GetObbDirs();
    }

    public static class ExternalResourceController
    {
        public static string BuildFilePath(string lang, string path)
        {
            return $"{lang}/{path}";
        }
        public static async Task<string> ReadString(string lang, string path)
        {
            if (path == null)
            {
                return null;
            }
            await DependencyService.Get<IExternalResourceReader>().GetInitTask();
            var enStream = DependencyService.Get<IExternalResourceReader>().GetInputStream($"en/{path}");
            if (enStream == null)
            {
                return null;
            }
            var enJson = await new StreamReader(enStream).ReadToEndAsync();
            if (lang != "en")
            {
                var langStream = DependencyService.Get<IExternalResourceReader>().GetInputStream($"{lang}/{path}");
                if (langStream == null)
                {
                    return enJson;
                }
                var langJson = await new StreamReader(langStream).ReadToEndAsync();

                var enObj = JToken.Parse(enJson);
                var langObj = JToken.Parse(langJson);
                if (enObj.Type == JTokenType.Object && langObj.Type == JTokenType.Object)
                {
                    (enObj as JObject).Merge(langObj as JObject, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Merge,
                        MergeNullValueHandling = MergeNullValueHandling.Ignore
                    });
                    return enObj.ToString();
                }
                else
                {
                    return langJson;
                }
            }
            else
            {
                return enJson;
            }
        }
        public static async Task<Stream> GetInputStream(string lang, string path)
        {
            if (path == null)
            {
                return null;
            }
            await DependencyService.Get<IExternalResourceReader>().GetInitTask();
            var stream = DependencyService.Get<IExternalResourceReader>().GetInputStream(BuildFilePath(lang, path));
            if (stream != null)
            {
                return DependencyService.Get<IExternalResourceReader>().GetInputStream(BuildFilePath(lang, path));
            }
            return DependencyService.Get<IExternalResourceReader>().GetInputStream(BuildFilePath("en", path));
        }
        public static async Task<ImageSource> GetImageSource(string lang, string path)
        {
            if (path == null)
            {
                return null;
            }
            await DependencyService.Get<IExternalResourceReader>().GetInitTask();
            var stream = DependencyService.Get<IExternalResourceReader>().GetInputStream(BuildFilePath(lang, path));
            if (stream != null)
            {
                return ImageSource.FromStream(() => DependencyService.Get<IExternalResourceReader>().GetInputStream(BuildFilePath(lang, path)));
            }
            return ImageSource.FromStream(() => DependencyService.Get<IExternalResourceReader>().GetInputStream(BuildFilePath("en", path)));
        }
    }
}

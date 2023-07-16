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
    public static class ResourceReader
    {
        public static string ReadString(Stream enStream, Stream langStream)
        {
            if (enStream == null)
            {
                return null;
            }
            var enJson = new StreamReader(enStream).ReadToEnd();
            if (langStream != null)
            {
                var langJson = new StreamReader(langStream).ReadToEnd();

                if (langJson != null && langJson != "")
                {
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
                }
            }
            return enJson;
        }
        public static Stream ResolveInputStream(Stream enStream, Stream langStream)
        {
            if (langStream != null)
            {
                return langStream;
            }
            else
            {
                return enStream;
            }
        }
        public static ImageSource GetImageSource(Stream enStream, Stream langStream)
        {
            if (enStream == null)
            {
                return null;
            }
            if (langStream != null)
            {
                return ImageSource.FromStream(() => langStream);
            }
            return ImageSource.FromStream(() => enStream);
        }
    }
}

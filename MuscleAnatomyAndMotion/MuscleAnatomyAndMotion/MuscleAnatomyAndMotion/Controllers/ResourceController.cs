using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MuscleAnatomyAndMotion.Controllers
{
    public static class ResourceController
    {
        public static bool IsOffline { get; set; } = true;
        public static async Task<string> ReadString(string lang, string path)
        {
            if (!IsOffline)
            {
                return InternalResourceController.ReadString(lang, path) ?? WebResourceController.ReadString(lang, path);
            }
            else
            {
                return await ExternalResourceController.ReadString(lang, path);
            }
        }
        public static async Task<Stream> GetInputStream(string lang, string path)
        {
            if (!IsOffline)
            {
                return InternalResourceController.GetInputStream(lang, path) ?? WebResourceController.GetInputStream(lang, path);
            }
            else
            {
                return await ExternalResourceController.GetInputStream(lang, path);
            }
        }
        public static async Task<ImageSource> GetImageSource(string lang, string path)
        {
            if (!IsOffline)
            {
                return InternalResourceController.GetImageSource(lang, path) ?? WebResourceController.GetImageSource(lang, path);
            }
            else
            {
                return await ExternalResourceController.GetImageSource(lang, path);
            }
        }
    }
}

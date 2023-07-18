using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace MuscleAnatomyAndMotion.Controllers
{
    static class LocalFilesController
    {
        public static void Init()
        {
            FavoriteController.Init();
            WebResourceController.Init();
        }
    }
}

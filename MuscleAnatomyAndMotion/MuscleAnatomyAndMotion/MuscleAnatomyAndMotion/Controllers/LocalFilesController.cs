using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

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

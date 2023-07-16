using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace MuscleAnatomyAndMotion.Controllers
{
    class FavoriteController
    {
        private static string path = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "favorite.json");
        public class FavoriteData
        {
            public ObservableCollection<BaseMuscleID> muscleIDs { get; set; } = new ObservableCollection<BaseMuscleID>();
            public ObservableCollection<MuscleID> subMuscleIDs { get; set; } = new ObservableCollection<MuscleID>();
            public ObservableCollection<ExerciseID> exerciseIDs { get; set; } = new ObservableCollection<ExerciseID>();
        }
        public static FavoriteData favoriteData = new FavoriteData();
        public static void Init()
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                favoriteData = JsonConvert.DeserializeObject<FavoriteData>(json);
                favoriteData.muscleIDs = new ObservableCollection<BaseMuscleID>(favoriteData.muscleIDs.Where(x => x != null).ToList());
                favoriteData.subMuscleIDs = new ObservableCollection<MuscleID>(favoriteData.subMuscleIDs.Where(x => x != null).ToList());
                favoriteData.exerciseIDs = new ObservableCollection<ExerciseID>(favoriteData.exerciseIDs.Where(x => x != null).ToList());
            }
            favoriteData.muscleIDs.CollectionChanged += Save;
            favoriteData.subMuscleIDs.CollectionChanged += Save;
            favoriteData.exerciseIDs.CollectionChanged += Save;
        }

        private static void Save(object sender, NotifyCollectionChangedEventArgs e)
        {
            var json = JsonConvert.SerializeObject(favoriteData);
            File.WriteAllText(path, json);
        }
    }
}

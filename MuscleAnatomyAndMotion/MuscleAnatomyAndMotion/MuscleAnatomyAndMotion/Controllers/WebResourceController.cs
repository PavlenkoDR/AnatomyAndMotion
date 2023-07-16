using MuscleAnatomyAndMotion.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    public static class WebResourceController
    {
        private static string path = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "cache.json");
        public class DownloadedData
        {
            public ObservableCollection<string> cache { get; set; } = new ObservableCollection<string>();
            public ObservableCollection<string> saves { get; set; } = new ObservableCollection<string>();
            public ObservableCollection<BaseMuscleID> muscleIDs { get; set; } = new ObservableCollection<BaseMuscleID>();
            public ObservableCollection<ExerciseID> exerciseIDs { get; set; } = new ObservableCollection<ExerciseID>();
            public ObservableCollection<BodyPartID> bodyPartIDs { get; set; } = new ObservableCollection<BodyPartID>();

        }
        public static DownloadedData downloadedData = new DownloadedData();
        public static void Init()
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                downloadedData = JsonConvert.DeserializeObject<DownloadedData>(json);
            }
            var savePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DownloadedHDResouces");
            if (Directory.Exists(savePath))
            {
                string[] allfiles = Directory.GetFiles(savePath, "*", SearchOption.AllDirectories);
                foreach (var file in allfiles)
                {
                    if (!downloadedData.cache.Contains(file) && !downloadedData.saves.Contains(file))
                    {
                        File.Delete(file);
                    }
                }
            }
            downloadedData.cache.CollectionChanged += Save;
            downloadedData.saves.CollectionChanged += Save;
            downloadedData.muscleIDs.CollectionChanged += MuscleIDs_CollectionChanged;
            downloadedData.exerciseIDs.CollectionChanged += ExerciseIDs_CollectionChanged;
            downloadedData.bodyPartIDs.CollectionChanged += BodyPartIDs_CollectionChanged;
        }

        private static void BodyPartIDs_SaveChange(IList items, Action<string> saveChanger)
        {
            if (items == null)
            {
                return;
            }
            foreach (var idRaw in items)
            {
                var id = idRaw as BodyPartID;
                if (id == null)
                {
                    continue;
                }
                {
                    var bodyPart = MuscleDictionary.muscleAssets[id];
                    saveChanger(bodyPart.video_url);
                    foreach (var layer in bodyPart.layers)
                    {
                        foreach (var side in layer.sides)
                        {
                            foreach (var area in side.areas)
                            {
                                saveChanger(area.area_image_url);
                            }
                        }
                    }
                }
            }
        }

        private static void BodyPartIDs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            BodyPartIDs_SaveChange(e.NewItems, WriteToSaves);
            BodyPartIDs_SaveChange(e.OldItems, RemoveFromSaves);
            Save(sender, e);
        }

        private static void MuscleIDs_SaveChange(IList items, Action<string> saveChanger)
        {
            if (items == null)
            {
                return;
            }
            foreach (var idRaw in items)
            {
                var id = idRaw as BaseMuscleID;
                if (id == null)
                {
                    continue;
                }
                {
                    var muscle = MuscleDictionary.musclesExtended[id];
                    saveChanger(muscle.thumbnail_image_url);
                    saveChanger(muscle.strength_video_url);
                    saveChanger(muscle.stretch_video_url);
                    saveChanger(muscle.origin.audio_url);
                    saveChanger(muscle.origin.subtitles_url);
                    saveChanger(muscle.origin.video_url);
                    foreach (var part in muscle.parts)
                    {
                        saveChanger(part.image_url);
                        saveChanger(part.overlay_image_url);
                        saveChanger(part.thumbnail_image_url);
                        foreach (var action in part.actions)
                        {
                            foreach (var video in action.videos)
                            {
                                saveChanger(video.video_url);
                                saveChanger(video.subtitles_url);
                            }
                        }
                    }
                }
            }
        }

        private static void MuscleIDs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            MuscleIDs_SaveChange(e.NewItems, WriteToSaves);
            MuscleIDs_SaveChange(e.OldItems, RemoveFromSaves);
            Save(sender, e);
        }

        private static void ExerciseIDs_SaveChange(IList items, Action<string> saveChanger)
        {
            if (items == null)
            {
                return;
            }
            foreach (var idRaw in items)
            {
                var id = idRaw as ExerciseID;
                if (id == null)
                {
                    continue;
                }
                {
                    var exercise = MuscleDictionary.exercises[id];
                    saveChanger(exercise.thumbnail_image_url);
                }
                {
                    var exercise = MuscleDictionary.exercisesExtended[id];
                    saveChanger(exercise.thumbnail_image_url);
                    saveChanger(exercise.analyse_table_image_url);
                    foreach (var videos in new[] { exercise.correct_videos, exercise.mistake_videos })
                    {
                        foreach (var video in videos)
                        {
                            saveChanger(video.asset_url);
                            saveChanger(video.audio_url);
                            saveChanger(video.subtitles_url);
                        }
                    }
                }
            }
        }
        private static void ExerciseIDs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ExerciseIDs_SaveChange(e.NewItems, WriteToSaves);
            ExerciseIDs_SaveChange(e.OldItems, RemoveFromSaves);
            Save(sender, e);
        }
        private static void WriteToSaves(string path)
        {
            var pathUrl = GetDownloadedFilePath(LanguageController.CurrentLanguage, path);
            if (pathUrl != null && !downloadedData.saves.Contains(pathUrl))
            {
                downloadedData.saves.Add(pathUrl);
            }
        }
        private static void RemoveFromSaves(string path)
        {
            var pathUrl = BuildFilePath(LanguageController.CurrentLanguage, path);
            if (pathUrl != null && !downloadedData.saves.Contains(pathUrl))
            {
                downloadedData.saves.Remove(pathUrl);
            }
        }

        public static void ClearCache()
        {
            List<string> removed = new List<string>();
            foreach(var file in downloadedData.cache)
            {
                if (!downloadedData.saves.Contains(file))
                {
                    if (file == null)
                    {
                        continue;
                    }
                    File.Delete(file);
                    removed.Add(file);
                }
            }
            foreach (var file in removed)
            {
                downloadedData.cache.Remove(file);
            }
        }

        private static void Save(object sender, NotifyCollectionChangedEventArgs e)
        {
            var json = JsonConvert.SerializeObject(downloadedData);
            File.WriteAllText(path, json);
        }
        public static bool IsFileDownloaded(string lang, string path)
        {
            var savePath = Path.Combine($"DownloadedHDResouces/{lang}", Regex.Replace(path, "https://.*\\..*?/", ""));
            savePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), savePath);
            var result = downloadedData.cache.Contains(savePath) || downloadedData.saves.Contains(savePath);
            return result && File.Exists(savePath);
        }
        public static bool IsFileHDDownloaded(string lang, string path)
        {
            var savePath = Path.Combine($"DownloadedHDResouces/{lang}", Regex.Replace(path, "https://.*\\..*?/", ""));
            savePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), savePath);
            var result = downloadedData.saves.Contains(savePath);
            return result && File.Exists(savePath);
        }
        public static string BuildFilePath(string lang, string path)
        {
            if ((path == null) || (path == ""))
            {
                return null;
            }
            var savePath = Path.Combine($"DownloadedHDResouces/{lang}", Regex.Replace(path, "https://.*\\..*?/", ""));
            savePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), savePath);
            return savePath;
        }
        public static string GetDownloadedFilePath(string lang, string path, bool reload = false)
        {
            if ((path == null) || (path == ""))
            {
                return null;
            }
            WebClient client = new WebClient();
            var savePath = BuildFilePath(lang, path);
            var fileInfo = new FileInfo(savePath);
            if (fileInfo.Exists)
            {
                if (reload)
                {
                    fileInfo.Delete();
                }
                else
                {
                    return savePath;
                }
            }
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }
            client.DownloadFile(path, savePath);
            downloadedData.cache.Add(savePath);
            return savePath;
        }
        private static Stream GetStream(string lang, string path)
        {
            var localPath = GetDownloadedFilePath(lang, path);
            return new FileStream(localPath, FileMode.Open, FileAccess.Read);
        }
        public static string ReadString(string lang, string path)
        {
            var streamData = GetStream(lang, path);
            return ResourceReader.ReadString(streamData, null);
        }
        public static Stream GetInputStream(string lang, string path)
        {
            var streamData = GetStream(lang, path);
            return ResourceReader.ResolveInputStream(streamData, null);
        }
        public static ImageSource GetImageSource(string lang, string path)
        {
            return ImageSource.FromStream(() => GetStream(lang, path));
        }
    }
}

using EO.WebBrowser;
using EO.WebEngine;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnatomyAndMotionScrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        async void ParseJson()
        {
            WebClient client = new WebClient();
            Directory.CreateDirectory("../../../../ParsedContent");
            for (int i = 1; i <= 95; ++i)
            {
                if (!File.Exists($"../../../../RawContent/{i}.json"))
                {
                    continue;
                }
                var json = File.ReadAllText($"../../../../RawContent/{i}.json");
                var jsonObj = JsonConvert.DeserializeObject<JObject>(json);

                Directory.CreateDirectory($"../../../../ParsedContent/{i}");

                foreach (var key in new []{ "strengthVideoUrl", "stretchVideoUrl", "originAndInsertionVideoUrl" })
                {
                    if (!jsonObj.ContainsKey(key))
                    {
                        continue;
                    }
                    var url = jsonObj[key].ToString();
                    if (url.Length == 0)
                    {
                        continue;
                    }
                    var extention = System.IO.Path.GetExtension(url);
                    await client.DownloadFileTaskAsync(url, $"../../../../ParsedContent/{i}/{key}{extention}");
                    jsonObj[key] = $"{i}/{key}.mp4";
                }

                var mainVideos = jsonObj["mainVideos"] as JArray;
                for (int j = 0; j < mainVideos.Count; ++j)
                {
                    var movements = mainVideos[j]["movements"] as JArray;
                    for (int k = 0; k < movements.Count; ++k)
                    {
                        foreach (var key in new[] { "videoUrls", "imageUrls" })
                        {
                            if (!(movements[k] as JObject).ContainsKey(key))
                            {
                                continue;
                            }
                            var urls = movements[k][key] as JArray;
                            for (int ii = 0; ii < urls.Count; ++ii)
                            {
                                var url = movements[k][key][ii].ToString();
                                if (url.Length == 0)
                                {
                                    continue;
                                }
                                var extention = System.IO.Path.GetExtension(url);
                                await client.DownloadFileTaskAsync(url, $"../../../../ParsedContent/{i}/{j + 1}.{k + 1}.{ii + 1}{extention}");
                                movements[k][key][ii] = $"{i}/{j + 1}.{k + 1}.{ii + 1}{extention}";
                            }
                        }
                    }
                }

                File.WriteAllText($"../../../../ParsedContent/{i}.json", JsonConvert.SerializeObject(jsonObj));
            }
        }

        Task reduceMp4(string path, string destinationPath)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "../../../../ffmpeg/bin/ffmpeg.exe";
            startInfo.Arguments = $" -i {path} -vcodec libx265 -crf 35 {destinationPath}";
            process.StartInfo = startInfo;
            process.Start();
            return process.WaitForExitAsync();
        }

        Task reduceMp3(string path, string destinationPath)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "../../../../ffmpeg/bin/ffmpeg.exe";
            startInfo.Arguments = $" -i {path} -codec:a libmp3lame -b:a 48k {destinationPath}";
            process.StartInfo = startInfo;
            process.Start();
            return process.WaitForExitAsync();
        }

        Task convertM4aToReduceMp3(string path, string destinationPath)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "../../../../ffmpeg/bin/ffmpeg.exe";
            startInfo.Arguments = $" -i {path} -acodec libmp3lame -ab 48k {destinationPath}";
            process.StartInfo = startInfo;
            process.Start();
            return process.WaitForExitAsync();
        }

        Task reducePng(string path, string destinationPath)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "../../../../pngquant.exe";
            startInfo.Arguments = $" 16 {path} --output {destinationPath}";
            process.StartInfo = startInfo;
            process.Start();
            return process.WaitForExitAsync();
        }

        Task reduceJpg(string path, string destinationPath)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "../../../../convert.exe";
            startInfo.Arguments = $" {path} -sampling-factor 4:2:0 -strip -quality 70 -interlace JPEG -colorspace RGB {destinationPath}";
            process.StartInfo = startInfo;
            process.Start();
            return process.WaitForExitAsync();
        }

        List<string> filesToReduce = new List<string>();
        SortedDictionary<string, long> fileSizes = new SortedDictionary<string, long>();
        SortedDictionary<long, List<string>> sizeOfFile = new SortedDictionary<long, List<string>>();

        async Task ReduceFilesSizeDFS(string rootPath)
        {
            if (Directory.Exists(rootPath))
            {
                foreach(var directory in Directory.GetDirectories(rootPath))
                {
                    var destinationPath = directory.Replace("ParsedContent2", "CuttedContent2");
                    Directory.CreateDirectory(destinationPath);
                    await ReduceFilesSizeDFS(directory);
                }
                foreach (var path in Directory.GetFiles(rootPath))
                {
                    long length = new FileInfo(path).Length;
                    filesToReduce.Add(path);
                    fileSizes.Add(path, length);
                    if (!sizeOfFile.ContainsKey(length))
                    {
                        sizeOfFile.Add(length, new List<string>());
                    }
                    sizeOfFile[length].Add(path);
                    var destinationPath = path.Replace("ParsedContent2", "CuttedContent2");
                }
            }
        }

        async void ReduceFilesSize()
        {
            Directory.CreateDirectory("../../../../CuttedContent2");
            Directory.CreateDirectory("../../../../CuttedContent2/Assets");
            await ReduceFilesSizeDFS($"../../../../ParsedContent2/Assets");

            var sizeOfFileReversed = sizeOfFile.ToList();
            sizeOfFileReversed.Reverse();

            var xmlArray = filesToReduce.Select(x => $"    <EmbeddedResource Include=\"{ x.Replace("../../../../ParsedContent2/", "")}\" /> ");
            var xml = string.Join("\n", xmlArray);

            for (int i = 0; i < filesToReduce.Count; ++i)
            {
                var path = filesToReduce[i];
                debugLabel.Content = $"Progress: {i * 100.0 / filesToReduce.Count}%\n         {i}/{filesToReduce.Count}\n         {path}";
                var destinationPath = path.Replace("ParsedContent2", "CuttedContent2");

                if (File.Exists(destinationPath))
                {
                    continue;
                }
                if (System.IO.Path.GetExtension(path) == ".mp4")
                {
                    await reduceMp4(path, destinationPath);
                }
                else if (System.IO.Path.GetExtension(path) == ".mp3")
                {
                    await reduceMp3(path, destinationPath);
                }
                else if (System.IO.Path.GetExtension(path) == ".m4a")
                {
                    destinationPath = destinationPath.Replace(".m4a", ".mp3");
                    if (File.Exists(destinationPath))
                    {
                        continue;
                    }
                    await convertM4aToReduceMp3(path, destinationPath);
                }
                else if (System.IO.Path.GetExtension(path) == ".srt")
                {
                    File.Copy(path, destinationPath);
                }
                else if (System.IO.Path.GetExtension(path) == ".json")
                {
                }
                else
                {
                    {
                        await reducePng(path, destinationPath);
                    }
                    if (File.Exists(destinationPath))
                    {
                        continue;
                    }
                    {
                        await reduceJpg(path, destinationPath);
                    }
                }
                if (!File.Exists(destinationPath))
                {
                    continue;
                }
            }
        }

        void MassRename()
        {
            //for (int i = 1; i <= 95; ++i)
            foreach (var i in new[] { "hand_bottom", "hand_top", "head", "leg_bottom", "leg_top", "tors" })
            {
                foreach (var file in Directory.GetFiles($"../../../../CuttedContent/{i}"))
                {
                    File.Copy(file, file.Replace($"\\{i}", "\\").Replace(" (", ".").Replace(")", ""));
                    File.Delete(file);
                }
            }
        }

        List<ResponseEventArgs> streams = new List<ResponseEventArgs>();

        List<KeyValuePair<WebClient, Task>> downloadTasks = new List<KeyValuePair<WebClient, Task>>();
        SortedDictionary<string, string> urlCache = new SortedDictionary<string, string>();

        async Task<string> getUrlLinkAsync(string url, string id, string assetPath, string lang, bool isIdName = false)
        {
            if (url == null || url == "")
            {
                return "";
            }
            var filename = System.IO.Path.GetFileName(url);
            if (isIdName)
            {
                filename = id + System.IO.Path.GetExtension(url);
            }
            var path = $"../../../../ParsedContent2/Assets/{lang}/{assetPath}/{id}/{filename}";
            if (isIdName)
            {
                path = $"../../../../ParsedContent2/Assets/{lang}/{assetPath}/{filename}";
            }
            var assetPathLocal = $"{assetPath}/{id}/{filename}";
            if (isIdName)
            {
                assetPathLocal = $"{assetPath}/{filename}";
            }

            if (urlCache.ContainsKey(url))
            {
                return urlCache[url];
            }
            else
            {
                urlCache.Add(url, assetPathLocal);
            }
            if (File.Exists(path))
            {
                if (new FileInfo(path).Length == 0)
                {
                    assetPathLocal = "";
                    urlCache[url] = assetPathLocal;
                    File.Delete(path);
                }
                return urlCache[url];
            }
            //return "";
            WebClient client = new WebClient();

            if (downloadTasks.Count >= 10)
            {
                var task = await Task.WhenAny(downloadTasks.Select(x => x.Value).ToArray());
                downloadTasks.Remove(downloadTasks.Where(x => x.Value == task).First());
            }
            if (!isIdName && !Directory.Exists($"../../../../ParsedContent2/Assets/{lang}/{assetPath}/{id}"))
            {
                Directory.CreateDirectory($"../../../../ParsedContent2/Assets/{lang}/{assetPath}/{id}");
            }
            downloadTasks.Add(new KeyValuePair<WebClient, Task>(client, client.DownloadFileTaskAsync(url, path)));
            
            return assetPathLocal;
        }

        async Task<Task> DownloadMusclesContent(string lang)
        {
            if (!Directory.Exists($"../../../../RawContent/Response/{lang}/muscles"))
            {
                Directory.CreateDirectory($"../../../../RawContent/Response/{lang}/muscles");
            }
            for (int i = 0; i < 96; ++i)
            {
                if (!File.Exists($"../../../../RawContent/Response/{lang}/muscles/{i}.json"))
                {
                    continue;
                }
                var raw = File.ReadAllText($"../../../../RawContent/Response/{lang}/muscles/{i}.json");
                var rawJObj = JsonConvert.DeserializeObject<JObject>(raw);
                var submuscle = rawJObj["submuscle"] as JObject;
                if (submuscle == null)
                {
                    continue;
                }

                foreach (var key in new[] {
                    "thumbnail_image_url",
                    "stretch_video_url",
                    "stretch_audio_url",
                    "stretch_subtitles_url",
                    "strength_video_url",
                    "strength_audio_url",
                    "strength_subtitles_url" })
                {
                    submuscle[key] = await getUrlLinkAsync(submuscle[key]?.ToString(), $"{i}", "muscles", lang);
                }

                var origin = submuscle["origin"] as JObject;

                foreach (var key in new[] {
                    "video_url",
                    "audio_url",
                    "subtitles_url" })
                {
                    if (origin != null)
                    {
                        origin[key] = await getUrlLinkAsync(origin[key]?.ToString(), $"{i}", "muscles", lang);
                    }
                }

                var parts = submuscle["parts"] as JArray;

                for (int j = 0; j < parts.Count; ++j)
                {
                    foreach (var key in new[] {
                    "image_url",
                    "overlay_image_url",
                    "thumbnail_image_url" })
                    {
                        parts[j][key] = await getUrlLinkAsync(parts[j][key]?.ToString(), $"{i}", "muscles", lang);
                    }

                    var actions = parts[j]["actions"] as JArray;

                    for (int k = 0; k < actions.Count; ++k)
                    {
                        var videos = actions[k]["videos"] as JArray;
                        
                        for (int ii = 0; ii < videos.Count; ++ii)
                        {
                            var ttt = videos[ii]["video_url"];
                            var tstt = videos[ii]["subtitles_url"];
                            videos[ii]["video_url"] = await getUrlLinkAsync(videos[ii]["video_url"]?.ToString(), $"{i}", "muscles", lang);
                            videos[ii]["subtitles_url"] = await getUrlLinkAsync(videos[ii]["subtitles_url"]?.ToString(), $"{i}", "muscles", lang);
                        }
                    }
                }

                File.WriteAllText($"../../../../ParsedContent2/Response/{lang}/muscles/{i}.json", JsonConvert.SerializeObject(rawJObj, Formatting.Indented));
            
            }

            // C:/dev/AnatomyAndMotion/AnatomyAndMotionScrapper/ParsedContent/Response/muscles

            return Task.WhenAll(downloadTasks.Select(x => x.Value).ToArray());
        }

        async Task<Task> DownloadBonesContent(string lang)
        {
            for (int i = 0; i < 22; ++i)
            {
                if (!File.Exists($"../../../../RawContent/Response/{lang}/bones/jointHead{i}.json"))
                {
                    continue;
                }
                var raw = File.ReadAllText($"../../../../RawContent/Response/{lang}/bones/jointHead{i}.json");
                var rawJObj = JsonConvert.DeserializeObject<JObject>(raw);
                var subjoint = rawJObj["subjoint"] as JObject;
                if (subjoint == null)
                {
                    continue;
                }

                var image_urls = subjoint["image_urls"] as JArray;
                for (int j = 0; j < image_urls.Count; ++j)
                {
                    image_urls[j] = await getUrlLinkAsync(image_urls[j]?.ToString(), $"jointHead{i}", "bones", lang);
                }

                foreach (var key in new[] {
                    "thumbnail_image_url" })
                {
                    subjoint[key] = await getUrlLinkAsync(subjoint[key]?.ToString(), $"jointHead{i}", "bones", lang);
                }

                var planes = subjoint["planes"] as JArray;

                for (int j = 0; j < planes.Count; ++j)
                {
                    var actions = planes[j]["actions"] as JArray;

                    for (int k = 0; k < actions.Count; ++k)
                    {
                        foreach (var key in new[] {
                            "thumbnail_image_url" })
                        {
                            actions[k][key] = await getUrlLinkAsync(actions[k][key]?.ToString(), $"jointHead{i}", "bones", lang);
                        }
                        var videos = actions[k]["videos"] as JArray;

                        for (int ii = 0; ii < videos.Count; ++ii)
                        {
                            var ttt = videos[ii]["video_url"];
                            var tstt = videos[ii]["subtitles_url"];
                            videos[ii]["video_url"] = await getUrlLinkAsync(videos[ii]["video_url"]?.ToString(), $"jointHead{i}", "bones", lang);
                            videos[ii]["subtitles_url"] = await getUrlLinkAsync(videos[ii]["subtitles_url"]?.ToString(), $"jointHead{i}", "bones", lang);
                        }
                    }
                }

                File.WriteAllText($"../../../../ParsedContent2/Response/{lang}/bones/jointHead{i}.json", JsonConvert.SerializeObject(rawJObj, Formatting.Indented));

            }

            // C:/dev/AnatomyAndMotion/AnatomyAndMotionScrapper/ParsedContent/Response/muscles

            return Task.WhenAll(downloadTasks.Select(x => x.Value).ToArray());
        }

        async Task<Task> DownloadBonesComplexContent(string lang)
        {
            for (int i = 0; i < 22; ++i)
            {
                if (!File.Exists($"../../../../RawContent/Response/{lang}/bones/complex/id_bone_{i}.json"))
                {
                    continue;
                }
                var raw = File.ReadAllText($"../../../../RawContent/Response/{lang}/bones/complex/id_bone_{i}.json");
                var rawJObj = JsonConvert.DeserializeObject<JObject>(raw);
                var bone = rawJObj["bone"] as JObject;
                if (bone == null)
                {
                    continue;
                }

                foreach (var key in new[] {
                    "thumbnail_image_url",
                    "video_url"})
                {
                    bone[key] = await getUrlLinkAsync(bone[key]?.ToString(), $"id_bone_{i}", "bones/complex", lang);
                }

                var stops = bone["stops"] as JArray;

                for (int j = 0; j < stops.Count; ++j)
                {
                    var clickables = stops[j]["clickables"] as JArray;

                    for (int k = 0; k < clickables.Count; ++k)
                    {
                        clickables[k]["image_url"] = await getUrlLinkAsync(clickables[k]["image_url"]?.ToString(), $"id_bone_{i}", "bones/complex", lang);

                    }
                }

                File.WriteAllText($"../../../../ParsedContent2/Response/{lang}/bones/complex/id_bone_{i}.json", JsonConvert.SerializeObject(rawJObj, Formatting.Indented));

            }

            // C:/dev/AnatomyAndMotion/AnatomyAndMotionScrapper/ParsedContent/Response/muscles

            return Task.WhenAll(downloadTasks.Select(x => x.Value).ToArray());
        }

        async Task<Task> DownloadTrainingContent(string lang)
        {
            for (int i = 0; i < 1325; ++i)
            {
                if (!File.Exists($"../../../../RawContent/Response/{lang}/training/{i}.json"))
                {
                    continue;
                }
                var raw = File.ReadAllText($"../../../../RawContent/Response/{lang}/training/{i}.json");
                var rawJObj = JsonConvert.DeserializeObject<JObject>(raw);
                var exercise = rawJObj["exercise"] as JObject;
                if (exercise == null)
                {
                    continue;
                }

                foreach (var key in new[] {
                    "thumbnail_image_url",
                    "analyse_table_image_url" })
                {
                    exercise[key] = await getUrlLinkAsync(exercise[key]?.ToString(), $"{i}", "training", lang);
                }

                foreach (var key1 in new[] {
                    "correct_videos",
                    "mistake_videos" })
                {
                    var parts = exercise[key1] as JArray;

                    for (int j = 0; j < parts?.Count; ++j)
                    {
                        foreach (var key in new[] {
                            "asset_url",
                            "audio_url",
                            "subtitles_url" })
                        {
                            parts[j][key] = await getUrlLinkAsync(parts[j][key]?.ToString(), $"{i}", "training", lang);
                        }
                    }
                }

                File.WriteAllText($"../../../../ParsedContent2/Response/{lang}/training/{i}.json", JsonConvert.SerializeObject(rawJObj, Formatting.Indented));

            }

            return Task.WhenAll(downloadTasks.Select(x => x.Value).ToArray());
        }

        async Task<Task> DownloadMusclesAssets(string lang)
        {
            var raw = File.ReadAllText($"../../../../RawContent/Response/{lang}/muscles_assets.json");
            var rawJObj = JsonConvert.DeserializeObject<JObject>(raw);
            var assets = rawJObj["assets"] as JArray;

            for (var i = 0; i < assets.Count; ++i)
            {
                assets[i]["video_url"] = await getUrlLinkAsync(assets[i]["video_url"]?.ToString(), $"{i + 1}", "muscles_assets", lang);

                var layers = assets[i]["layers"] as JArray;
                for (var j = 0; j < layers.Count; ++j)
                {
                    var sides = layers[j]["sides"] as JArray;
                    for (var k = 0; k < sides.Count; ++k)
                    {
                        var areas = sides[k]["areas"] as JArray;
                        for (var ii = 0; ii < areas.Count; ++ii)
                        {
                            areas[ii]["area_image_url"] = await getUrlLinkAsync(areas[ii]["area_image_url"]?.ToString(), $"{i + 1}/{j + 1}.{k + 1}.{ii + 1}", "muscles_assets", lang, true);
                        }
                    }
                }
            }
            File.WriteAllText($"../../../../ParsedContent2/Response/{lang}/muscles_assets.json", JsonConvert.SerializeObject(rawJObj, Formatting.Indented));


            return Task.WhenAll(downloadTasks.Select(x => x.Value).ToArray());
        }

        async Task<Task> DownloadFiltersAZ(string lang)
        {
            var raw = File.ReadAllText($"../../../../RawContent/Response/{lang}/filters_a-z.json");
            var rawJObj = JsonConvert.DeserializeObject<JObject>(raw);

            foreach (var objFilter in rawJObj)
            {
                var objFilterCasted = objFilter.Value as JArray;
                for (var i = 0; i < objFilterCasted?.Count; ++i)
                {
                    objFilterCasted[i]["thumbnail_image_url"] = await getUrlLinkAsync(objFilterCasted[i]["thumbnail_image_url"]?.ToString(), $"{i + 1}", "filters", lang);
                }
            }

            File.WriteAllText($"../../../../ParsedContent2/Response/{lang}/filters_a-z.json", JsonConvert.SerializeObject(rawJObj, Formatting.Indented));

            return Task.WhenAll(downloadTasks.Select(x => x.Value).ToArray());
        }

        async Task<Task> DownloadExercises(string lang)
        {
            var raw = File.ReadAllText($"../../../../RawContent/Response/{lang}/exercises.json");
            var rawJObj = JsonConvert.DeserializeObject<JObject>(raw)["exercises"] as JArray;

            foreach (var objFilter in rawJObj)
            {
                var id = objFilter["id"].ToString();
                objFilter["thumbnail_image_url"] = await getUrlLinkAsync(objFilter["thumbnail_image_url"]?.ToString(), id, "training", lang);
            }

            File.WriteAllText($"../../../../ParsedContent2/Response/{lang}/exercises.json", JsonConvert.SerializeObject(rawJObj, Formatting.Indented));

            return Task.WhenAll(downloadTasks.Select(x => x.Value).ToArray());
        }

        async Task<Task> DownloadCategories(string lang)
        {
            var raw = File.ReadAllText($"../../../../RawContent/Response/{lang}/categories.json");
            var rawJObj = JsonConvert.DeserializeObject<JObject>(raw);
            var categories = rawJObj["categories"] as JArray;

            foreach (var category in categories)
            {
                var exercises = category["exercises"] as JArray;
                for (var i = 0; i < exercises.Count; ++i)
                {
                    exercises[i]["thumbnail_image_url"] = await getUrlLinkAsync(exercises[i]["thumbnail_image_url"]?.ToString(), $"{i + 1}", "categories/exercises", lang);
                }
            }

            File.WriteAllText($"../../../../ParsedContent2/Response/{lang}/categories.json", JsonConvert.SerializeObject(rawJObj, Formatting.Indented));

            return Task.WhenAll(downloadTasks.Select(x => x.Value).ToArray());
        }

        async Task<Task> DownloadTheory(string lang)
        {

            for (int i = 0; i < 58; ++i)
            {
                if (!File.Exists($"../../../../RawContent/Response/{lang}/theory/{i}.json"))
                {
                    continue;
                }
                var raw = File.ReadAllText($"../../../../RawContent/Response/{lang}/theory/{i}.json");
                var rawJObj = JsonConvert.DeserializeObject<JObject>(raw);
                var subcategory = rawJObj["subcategory"] as JObject;
                if (subcategory == null)
                {
                    continue;
                }
                var items = subcategory["items"] as JArray;
                for (int j = 0; j < items.Count; ++j)
                {
                    foreach (var key in new[] { "thumbnail_url", "subtitles_url", "video_url", "audio_url", "link_video_url" })
                    {
                        items[j][key] = await getUrlLinkAsync(items[j][key]?.ToString(), $"{i}", "theory", lang);
                    }
                }
                File.WriteAllText($"../../../../ParsedContent2/Response/{lang}/theory/{i}.json", JsonConvert.SerializeObject(rawJObj, Formatting.Indented));
            }

            return Task.WhenAll(downloadTasks.Select(x => x.Value).ToArray());
        }

        async void RunDownloads()
        {
            //await DownloadMusclesContent("en");
            //await DownloadBonesContent("en");
            //await DownloadBonesComplexContent("en");
            //await DownloadTrainingContent("en");
            //await DownloadMusclesAssets("en");
            //await DownloadFiltersAZ("en");
            //await DownloadCategories("en");
            //await DownloadTheory("en");
            await DownloadExercises("en");

            //await DownloadMusclesContent("ru");
            //await DownloadBonesContent("ru");
            //await DownloadBonesComplexContent("ru");
            //await DownloadTrainingContent("ru");
            //await DownloadMusclesAssets("ru");
            //await DownloadFiltersAZ("ru");
            //await DownloadCategories("ru");
            //await DownloadTheory("ru");
            await DownloadExercises("ru");

            ReduceFilesSize();
        }

        public MainWindow()
        {
            InitializeComponent();

            //ParseJson();
            //MassRename();

            RunDownloads();
        }

        private void video_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void browser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            int kkk = 0;
        }

        private void browser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            int kkk = 0;
        }

        private void browser_Loaded(object sender, RoutedEventArgs e)
        {
            int kkk = 0;
        }
    }
}

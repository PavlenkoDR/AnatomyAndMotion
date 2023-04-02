using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
            Directory.CreateDirectory("../../../../ParsedContent");
            var client = new WebClient();
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

        async void ReduceFilesSize()
        {
            Directory.CreateDirectory("../../../../CuttedContent");
            //for (int i = 1; i <= 95; ++i)
            foreach (var i in new[] { "hand_bottom", "hand_top", "head", "leg_bottom", "leg_top", "tors" })
            {
                if (!Directory.Exists($"../../../../ParsedContent/{i}"))
                {
                    continue;
                }
                Directory.CreateDirectory($"../../../../CuttedContent/{i}");
                foreach (var path in Directory.GetFiles($"../../../../ParsedContent/{i}"))
                {
                    var destinationPath = path.Replace("ParsedContent", "CuttedContent");
                    if (File.Exists(destinationPath))
                    {
                        continue;
                    }
                    if (System.IO.Path.GetExtension(path) == ".mp4")
                    {
                        System.Diagnostics.Process process = new System.Diagnostics.Process();
                        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        startInfo.FileName = "../../../../ffmpeg/bin/ffmpeg.exe";
                        startInfo.Arguments = $" -i {path} -vcodec libx265 -crf 35 {destinationPath}";
                        process.StartInfo = startInfo;
                        process.Start();
                        await process.WaitForExitAsync();
                    }
                    else
                    {
                        {
                            System.Diagnostics.Process process = new System.Diagnostics.Process();
                            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            startInfo.FileName = "../../../../pngquant.exe";
                            startInfo.Arguments = $" 16 {path} --output {destinationPath}";
                            process.StartInfo = startInfo;
                            process.Start();
                            await process.WaitForExitAsync();
                        }
                        if (File.Exists(destinationPath))
                        {
                            continue;
                        }
                        {
                            System.Diagnostics.Process process = new System.Diagnostics.Process();
                            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            startInfo.FileName = "../../../../convert.exe";
                            startInfo.Arguments = $" {path} -sampling-factor 4:2:0 -strip -quality 70 -interlace JPEG -colorspace RGB {destinationPath}";
                            process.StartInfo = startInfo;
                            process.Start();
                            await process.WaitForExitAsync();
                        }
                    }
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

        public MainWindow()
        {
            InitializeComponent();
            video.Play();
            video.Position = TimeSpan.FromSeconds(0);

            //ParseJson();
            //ReduceFilesSize();
            MassRename();
        }

        private void video_Loaded(object sender, RoutedEventArgs e)
        {
            video.Pause();
        }
    }
}

using MuscleAnatomyAndMotion.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingScreenPage : ContentPage
    {
        public class LoadingProgressModel : ViewModelBase
        {
            private string _CurrentLoad;
            public string CurrentLoad
            {
                get => _CurrentLoad;
                set
                {
                    _CurrentLoad = value;
                    RaisePropertyChanged("CurrentLoad");
                    RaisePropertyChanged("LoadingProgress");
                }
            }
            public string LoadingProgress { get => CurrentLoad; }
        }

        LoadingProgressModel model = new LoadingProgressModel();

        public LoadingScreenPage()
        {
            InitializeComponent();

            BindingContext = model;

            ReadMuscleFullInfos();
            LocalFilesController.Init();
        }

        private async void ReadMuscleFullInfos()
        {
            try
            {
                var testJson = await ExternalResourceController.ReadString("ru", "muscles_assets.json");
            }
            catch
            {
                Application.Current.MainPage = new ErrorPage();
                return;
            }

            await Task.Run(async () => {
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "en"));
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ru"));

                MuscleDictionary.muscleAssets.Clear();
                {
                    model.CurrentLoad = "Загрузка частей тела";
                    var json = await ExternalResourceController.ReadString("ru", "muscles_assets.json");
                    var rawObject = JsonConvert.DeserializeObject<JObject>(json)["assets"] as JArray;
                    rawObject.ForEach(x => MuscleDictionary.muscleAssets.Add((BodyPartID)(x["id"].ToString()), JsonConvert.DeserializeObject<MuscleAsset>(JsonConvert.SerializeObject(x), new IDConverter<MuscleID>(id => (MuscleID)id))));
                }
                {
                    model.CurrentLoad = "Загрузка списка подкатегорий мышц";
                    var json = await ExternalResourceController.ReadString("ru", "submuscles.json");
                    var rawObject = JsonConvert.DeserializeObject<JObject>(json)["submuscles"] as JArray;
                    rawObject.ForEach(x => MuscleDictionary.submuscles[(MuscleID)(x["id"].ToString())] = x.ToObject<Submuscle>());
                    int count = MuscleDictionary.submuscles.Count;
                    foreach (var submuscle in MuscleDictionary.submuscles)
                    {
                        model.CurrentLoad = $"Загрузка списка подкатегорий мышц {MuscleDictionary.submuscles.Count - count} / {MuscleDictionary.submuscles.Count}";
                        var id = submuscle.Key;
                        if (!MuscleDictionary.submusclesClastered.ContainsKey(id.baseID))
                        {
                            MuscleDictionary.submusclesClastered[id.baseID] = new SortedDictionary<SubMuscleID, Submuscle>();
                        }
                        MuscleDictionary.submusclesClastered[id.baseID].Add(id.subID, submuscle.Value);
                        --count;
                    }
                }
                {
                    int count = MuscleDictionary.submusclesClastered.Count;
                    foreach (var submuscle in MuscleDictionary.submusclesClastered)
                    {
                        model.CurrentLoad = $"Загрузка мышц {MuscleDictionary.submusclesClastered.Count - count} / {MuscleDictionary.submusclesClastered.Count}";
                        var json = await ExternalResourceController.ReadString("ru", $"muscles/{submuscle.Key}.json");
                        var muscleExtended = JsonConvert.DeserializeObject<JObject>(json)["submuscle"];
                        MuscleDictionary.musclesExtended.Add(
                            submuscle.Key,
                            JsonConvert.DeserializeObject<MuscleExtended>(
                                JsonConvert.SerializeObject(muscleExtended),
                                new IDConverter<MuscleID>(id => (MuscleID)id),
                                new IDConverter<ActionID>(id => (ActionID)id),
                                new IDConverter<VideoID>(id => (VideoID)id),
                                new IDConverter<BaseMuscleID>(id => (BaseMuscleID)id)
                                )
                            );
                        --count;
                    }
                }
                {
                    var json = await ExternalResourceController.ReadString("ru", "exercises.json");
                    var rawObject = JsonConvert.DeserializeObject<JArray>(json);
                    int count = rawObject.Count;
                    foreach (var exercise in rawObject)
                    {
                        model.CurrentLoad = $"Загрузка списка упражнений {rawObject.Count - count} / {rawObject.Count}";
                        var exercisesId = (ExerciseID)exercise["id"].ToString();
                        MuscleDictionary.exercises.Add(
                            exercisesId,
                            JsonConvert.DeserializeObject<Exercise>(
                                JsonConvert.SerializeObject(exercise),
                                new IDConverter<FilterID>(id => (FilterID)id),
                                new IDConverter<ExerciseVideoID>(id => (ExerciseVideoID)id)
                                )
                            );
                        --count;
                    }
                }
                {
                    int count = MuscleDictionary.exercises.Count;
                    foreach (var exercise in MuscleDictionary.exercises)
                    {
                        model.CurrentLoad = $"Загрузка упражнений {MuscleDictionary.exercises.Count - count} / {MuscleDictionary.exercises.Count}";
                        --count;
                        var json = await ExternalResourceController.ReadString("ru", $"training/{exercise.Key}.json");
                        if (json == null)
                        {
                            continue;
                        }
                        var rawObject = JsonConvert.DeserializeObject<JObject>(json)["exercise"];
                        var exerciseExtended = JsonConvert.DeserializeObject<ExerciseExtended>(
                            JsonConvert.SerializeObject(rawObject),
                            new IDConverter<ExerciseID>(id => (ExerciseID)id),
                            new IDConverter<AssociationID>(id => (AssociationID)id),
                            new IDConverter<FilterID>(id => (FilterID)id),
                            new IDConverter<MuscleID>(id => (MuscleID)id),
                            new IDConverter<ExerciseVideoID>(id => (ExerciseVideoID)id)
                        );

                        exerciseExtended.muscle_ids.target = exerciseExtended.muscle_ids.target.Where(x => x.baseID.id != "0").ToList();
                        exerciseExtended.muscle_ids.stabilizer = exerciseExtended.muscle_ids.stabilizer.Where(x => x.baseID.id != "0").ToList();
                        exerciseExtended.muscle_ids.synergist = exerciseExtended.muscle_ids.synergist.Where(x => x.baseID.id != "0").ToList();
                        exerciseExtended.muscle_ids.lengthening = exerciseExtended.muscle_ids.lengthening.Where(x => x.baseID.id != "0").ToList();

                        foreach (var muscleID in exerciseExtended.muscle_ids.target)
                        {
                            MuscleDictionary.musclesExtended[muscleID.baseID].parts.Where(x => x.id.CompareTo(muscleID) == 0).First().exerciseIDs.target.Add(exercise.Key);
                        }
                        foreach (var muscleID in exerciseExtended.muscle_ids.stabilizer)
                        {
                            MuscleDictionary.musclesExtended[muscleID.baseID].parts.Where(x => x.id.CompareTo(muscleID) == 0).First().exerciseIDs.stabilizer.Add(exercise.Key);
                        }
                        foreach (var muscleID in exerciseExtended.muscle_ids.synergist)
                        {
                            MuscleDictionary.musclesExtended[muscleID.baseID].parts.Where(x => x.id.CompareTo(muscleID) == 0).First().exerciseIDs.synergist.Add(exercise.Key);
                        }
                        foreach (var muscleID in exerciseExtended.muscle_ids.lengthening)
                        {
                            MuscleDictionary.musclesExtended[muscleID.baseID].parts.Where(x => x.id.CompareTo(muscleID) == 0).First().exerciseIDs.lengthening.Add(exercise.Key);
                        }
                        MuscleDictionary.exercisesExtended.Add(exercise.Key, exerciseExtended);
                    }
                }
                {
                    var json = await ExternalResourceController.ReadString("ru", $"filters.json");
                    var rawObject = JsonConvert.DeserializeObject<JObject>(json)["filters"] as JArray;
                    int count = rawObject.Count;
                    foreach (var obj in rawObject)
                    {
                        model.CurrentLoad = $"Загрузка фильтров {rawObject.Count - count} / {rawObject.Count}";
                        --count;
                        var baseFilterID = (BaseFilterID)obj["id"].ToString();
                        if (!MuscleDictionary.filters.ContainsKey(baseFilterID))
                        {
                            MuscleDictionary.filters.Add(baseFilterID, JsonConvert.DeserializeObject<Filter>(
                                JsonConvert.SerializeObject(obj),
                                new IDConverter<FilterID>(id => (FilterID)id)
                            ));
                        }
                    }
                }

            }).ContinueWith(x => {
                Device.BeginInvokeOnMainThread(() => {
                    Application.Current.MainPage = new MainFlyoutPage();
                });
            });
        }
    }
}
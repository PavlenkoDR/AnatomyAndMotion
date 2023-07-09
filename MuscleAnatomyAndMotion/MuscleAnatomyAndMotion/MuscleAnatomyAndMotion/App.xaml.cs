using MuscleAnatomyAndMotion.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public abstract class ID : IComparable<ID>, IEquatable<ID>, IEqualityComparer<ID>
    {
        public string id { get; set; }
        public int CompareTo(ID obj)
        {
            return id.CompareTo(obj?.id);
        }
        public override bool Equals(object other)
        {
            return id == (other as ID)?.id;
        }
        public override int GetHashCode()
        {
            return int.Parse(id);
        }

        public bool Equals(ID other)
        {
            return id == other?.id;
        }

        public override string ToString()
        {
            return id;
        }

        public bool Equals(ID x, ID y)
        {
            return x?.id == y?.id;
        }

        public int GetHashCode(ID obj)
        {
            return obj.GetHashCode();
        }
    }
    public class IDConverter<IDType> : JsonConverter<IDType> where IDType : ID
    {
        Func<string, ID> converter;
        public IDConverter(Func<string, ID> converter)
        {
            this.converter = converter;
        }
        public override void WriteJson(JsonWriter writer, IDType value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override IDType ReadJson(JsonReader reader, Type objectType, IDType existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;

            return (IDType)converter.Invoke(s);
        }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class BaseMuscleID : ID
    {
        public static explicit operator BaseMuscleID(in string id)
        {
            return new BaseMuscleID() { id = id };
        }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class SubMuscleID : ID
    {
        public static explicit operator SubMuscleID(in string id)
        {
            return new SubMuscleID() { id = id };
        }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MuscleID : ID, IComparable<MuscleID>
    {
        public BaseMuscleID baseID { get; private set; }
        public SubMuscleID subID { get; private set; }
        public MuscleID(BaseMuscleID baseID, SubMuscleID subID)
        {
            this.baseID = baseID;
            this.subID = subID;
            id = $"{baseID.id}.{subID.id}";
        }
        public static explicit operator MuscleID(in string id)
        {
            var splitted = id.Split('.');
            return new MuscleID((BaseMuscleID)splitted[0], (SubMuscleID)(splitted.Length > 1 ? splitted[1] : "0"));
        }
        public override string ToString()
        {
            return baseID.ToString() + ((subID.ToString() != "0") ? $".{subID}" : "");
        }
        public int CompareTo(MuscleID other)
        {
            var result = baseID.CompareTo(other.baseID);
            return result != 0 ? result : subID.CompareTo(other.subID);
        }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class BodyPartID : ID
    {
        public static explicit operator BodyPartID(in string id)
        {
            return new BodyPartID() { id = id };
        }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ActionID : ID
    {
        public static explicit operator ActionID(in string id)
        {
            return new ActionID() { id = id };
        }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class VideoID : ID
    {
        public static explicit operator VideoID(in string id)
        {
            return new VideoID() { id = id };
        }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ExerciseID : ID
    {
        public static explicit operator ExerciseID(in string id)
        {
            return new ExerciseID() { id = id };
        }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class AssociationID : ID
    {
        public static explicit operator AssociationID(in string id)
        {
            return new AssociationID() { id = id };
        }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class BaseFilterID : ID
    {
        public static explicit operator BaseFilterID(in string id)
        {
            return new BaseFilterID() { id = id };
        }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class FilterID : ID
    {
        public static explicit operator FilterID(in string id)
        {
            return new FilterID() { id = id };
        }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ExerciseVideoID : ID
    {
        public static explicit operator ExerciseVideoID(in string id)
        {
            return new ExerciseVideoID() { id = id };
        }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MuscleLayerSideArea
    {
        public string area_image_url { get; set; }
        public string name { get; set; }
        public MuscleID target_id { get; set; }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MuscleLayerSide
    {
        public int side_index { get; set; }
        public int stop { get; set; }
        public int next_stop { get; set; }
        public List<MuscleLayerSideArea> areas { get; set; }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MuscleLayer
    {
        public int layer_index { get; set; }
        public List<MuscleLayerSide> sides { get; set; }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MuscleAsset
    {
        public string body_part { get; set; }
        public string title { get; set; }
        public string video_url { get; set; }
        public int fps { get; set; }
        public bool is_paid { get; set; }
        public List<MuscleLayer> layers { get; set; }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class Submuscle
    {
        public string name { get; set; }
        public bool is_paid { get; set; }
        public string thumbnail_image_url { get; set; }
        public string first_video { get; set; }
        public string name_en { get; set; }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class Description
    {
        public string structure { get; set; }
        public string actions { get; set; }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MusclePart : ViewModelBase
    {
        public MuscleID id { get; set; }
        public string name { get; set; }
        public string origin { get; set; }
        public string insertion { get; set; }
        public string image_url { get; set; }
        public string overlay_image_url { get; set; }
        public string thumbnail_image_url { get; set; }
        public bool is_paid { get; set; }
        public List<MuscleAction> actions { get; set; }
        public MuscleExerciseIDs exerciseIDs { get; set; } = new MuscleExerciseIDs();
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MuscleActionVideo
    {
        public VideoID id { get; set; }
        public string video_url { get; set; }
        public string subtitles_url { get; set; }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MuscleAction
    {
        public ActionID id { get; set; }
        public string name { get; set; }
        public List<MuscleActionVideo> videos { get; set; }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MuscleOrigin
    {
        public string name { get; set; }
        public string video_url { get; set; }
        public string audio_url { get; set; }
        public string subtitles_url { get; set; }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MuscleExtended : ViewModelBase
    {
        public BaseMuscleID id { get; set; }
        public string name { get; set; }
        public Description description { get; set; }
        public bool is_paid { get; set; }
        public string thumbnail_image_url { get; set; }
        public string stretch_video_url { get; set; }
        public string strength_video_url { get; set; }
        public List<MusclePart> parts { get; set; }
        public MuscleOrigin origin { get; set; }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MuscleExerciseIDs
    {
        public List<ExerciseID> target { get; set; } = new List<ExerciseID>();
        public List<ExerciseID> synergist { get; set; } = new List<ExerciseID>();
        public List<ExerciseID> stabilizer { get; set; } = new List<ExerciseID>();
        public List<ExerciseID> lengthening { get; set; } = new List<ExerciseID>();
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ExerciseExtended
    {
        public ExerciseID id { get; set; }
        public AssociationID association_id { get; set; }
        public string language_id { get; set; }
        public string name { get; set; }
        public bool is_paid { get; set; }
        public bool is_draft { get; set; }
        public bool comingsoon { get; set; }
        public string thumbnail_image_url { get; set; }
        public List<FilterID> filter_ids { get; set; }
        public List<ExerciseVideo> correct_videos { get; set; }
        public List<ExerciseVideo> mistake_videos { get; set; }
        public ExerciseMuscleIDs muscle_ids { get; set; }
        public string analyse_table_image_url { get; set; }
        public bool is_asana { get; set; }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ExerciseVideo
    {
        public ExerciseVideoID id { get; set; }
        public int index { get; set; }
        public string asset_url { get; set; }
        public string audio_url { get; set; }
        public string description { get; set; }
        public string subtitles_url { get; set; }
        public bool description_is_en { get; set; }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ExerciseMuscleIDs
    {
        public List<MuscleID> target { get; set; }
        public List<MuscleID> synergist { get; set; }
        public List<MuscleID> stabilizer { get; set; }
        public List<MuscleID> lengthening { get; set; }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class Exercise : ViewModelBase
    {
        public string name { get; set; }
        public bool comingsoon { get; set; }
        public string name_en { get; set; }
        public bool is_paid { get; set; }
        public string thumbnail_image_url { get; set; }
        public List<FilterID> filter_ids { get; set; }
        public string language_id { get; set; }
        public string vertical_id { get; set; }
        public string target_muscle { get; set; }
        public ExerciseVideoID first_video { get; set; }
        public long createdAt { get; set; }
        public int index { get; set; }
        public bool favorite { get; set; }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class Filter
    {
        public string language_id { get; set; }
        public string title { get; set; }
        public List<Subfilter> subfilters { get; set; }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class Subfilter
    {
        public FilterID id { get; set; }
        public string title { get; set; }
    }
    public partial class App : Application
    {

        public static SortedDictionary<BaseFilterID, Filter> filters = new SortedDictionary<BaseFilterID, Filter>();
        public static SortedDictionary<BodyPartID, MuscleAsset> muscleAssets = new SortedDictionary<BodyPartID, MuscleAsset>();
        public static SortedDictionary<MuscleID, Submuscle> submuscles = new SortedDictionary<MuscleID, Submuscle>();
        public static SortedDictionary<BaseMuscleID, SortedDictionary<SubMuscleID, Submuscle>> submusclesClastered = new SortedDictionary<BaseMuscleID, SortedDictionary<SubMuscleID, Submuscle>>();
        public static SortedDictionary<BaseMuscleID, MuscleExtended> musclesExtended = new SortedDictionary<BaseMuscleID, MuscleExtended>();
        public static SortedDictionary<ExerciseID, ExerciseExtended> exercisesExtended = new SortedDictionary<ExerciseID, ExerciseExtended>();
        public static SortedDictionary<ExerciseID, Exercise> exercises = new SortedDictionary<ExerciseID, Exercise>();
        
        public App()
        {
            InitializeComponent();

            MainPage = new LoadingScreenPage();

            //string cachePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "filesCache.json");
            //if (File.Exists(cachePath))
            //{
            //    var jsonCache = File.ReadAllText(cachePath);
            //    var filesCache = JsonConvert.DeserializeObject<List<string>>(jsonCache);
            //    foreach (var file in filesCache)
            //    {
            //        File.Delete(file);
            //    }
            //    File.Delete(cachePath);
            //}

            ReadMuscleFullInfos();
            LocalFilesController.Init();
        }

        public void ManualCleanUp()
        {
            CleanUp();
            OnResume();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private async void ReadMuscleFullInfos()
        {
            try
            {
                var testJson = await ExternalResourceController.ReadString("ru", "muscles_assets.json");
            }
            catch
            {
                MainPage = new ErrorPage();
                return;
            }

            await Task.Run(async () => {
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "en"));
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ru"));

                muscleAssets.Clear();
                {
                    var json = await ExternalResourceController.ReadString("ru", "muscles_assets.json");
                    var rawObject = JsonConvert.DeserializeObject<JObject>(json)["assets"] as JArray;
                    rawObject.ForEach(x => muscleAssets.Add((BodyPartID)(x["id"].ToString()), JsonConvert.DeserializeObject<MuscleAsset>(JsonConvert.SerializeObject(x), new IDConverter<MuscleID>(id => (MuscleID)id))));
                }
                {
                    var json = await ExternalResourceController.ReadString("ru", "submuscles.json");
                    var rawObject = JsonConvert.DeserializeObject<JObject>(json)["submuscles"] as JArray;
                    rawObject.ForEach(x => submuscles[(MuscleID)(x["id"].ToString())] = x.ToObject<Submuscle>());
                    foreach (var submuscle in submuscles)
                    {
                        var id = submuscle.Key;
                        if (!submusclesClastered.ContainsKey(id.baseID))
                        {
                            submusclesClastered[id.baseID] = new SortedDictionary<SubMuscleID, Submuscle>();
                        }
                        submusclesClastered[id.baseID].Add(id.subID, submuscle.Value);
                    }
                }
                {
                    foreach (var submuscle in submusclesClastered)
                    {
                        var json = await ExternalResourceController.ReadString("ru", $"muscles/{submuscle.Key}.json");
                        var muscleExtended = JsonConvert.DeserializeObject<JObject>(json)["submuscle"];
                        musclesExtended.Add(
                            submuscle.Key,
                            JsonConvert.DeserializeObject<MuscleExtended>(
                                JsonConvert.SerializeObject(muscleExtended),
                                new IDConverter<MuscleID>(id => (MuscleID)id),
                                new IDConverter<ActionID>(id => (ActionID)id),
                                new IDConverter<VideoID>(id => (VideoID)id),
                                new IDConverter<BaseMuscleID>(id => (BaseMuscleID)id)
                                )
                            );
                    }
                }
                {
                    var json = await ExternalResourceController.ReadString("ru", "exercises.json");
                    var rawObject = JsonConvert.DeserializeObject<JArray>(json);
                    foreach (var exercise in rawObject)
                    {
                        var exercisesId = (ExerciseID)exercise["id"].ToString();
                        exercises.Add(
                            exercisesId,
                            JsonConvert.DeserializeObject<Exercise>(
                                JsonConvert.SerializeObject(exercise),
                                new IDConverter<FilterID>(id => (FilterID)id),
                                new IDConverter<ExerciseVideoID>(id => (ExerciseVideoID)id)
                                )
                            );
                    }
                }
                {
                    foreach (var exercise in exercises)
                    {
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
                            musclesExtended[muscleID.baseID].parts.Where(x => x.id.CompareTo(muscleID) == 0).First().exerciseIDs.target.Add(exercise.Key);
                        }
                        foreach (var muscleID in exerciseExtended.muscle_ids.stabilizer)
                        {
                            musclesExtended[muscleID.baseID].parts.Where(x => x.id.CompareTo(muscleID) == 0).First().exerciseIDs.stabilizer.Add(exercise.Key);
                        }
                        foreach (var muscleID in exerciseExtended.muscle_ids.synergist)
                        {
                            musclesExtended[muscleID.baseID].parts.Where(x => x.id.CompareTo(muscleID) == 0).First().exerciseIDs.synergist.Add(exercise.Key);
                        }
                        foreach (var muscleID in exerciseExtended.muscle_ids.lengthening)
                        {
                            musclesExtended[muscleID.baseID].parts.Where(x => x.id.CompareTo(muscleID) == 0).First().exerciseIDs.lengthening.Add(exercise.Key);
                        }
                        exercisesExtended.Add(exercise.Key, exerciseExtended);
                    }
                }
                {
                    var json = await ExternalResourceController.ReadString("ru", $"filters.json");
                    var rawObject = JsonConvert.DeserializeObject<JObject>(json)["filters"] as JArray;
                    foreach (var obj in rawObject)
                    {
                        var baseFilterID = (BaseFilterID)obj["id"].ToString();
                        if (!filters.ContainsKey(baseFilterID))
                        {
                            filters.Add(baseFilterID, JsonConvert.DeserializeObject<Filter>(
                                JsonConvert.SerializeObject(obj),
                                new IDConverter<FilterID>(id => (FilterID)id)
                            ));
                        }
                    }
                }

            }).ContinueWith(x => {
                Device.BeginInvokeOnMainThread(() => {
                    MainPage = new MainFlyoutPage();
                });
            });
        }
    }
}

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

namespace MuscleAnatomyAndMotion.Controllers
{
    public class MuscleDictionary
    {
        public enum DictionaryType
        {
            OFFLINE,
            ONLINE
        }
        public SortedDictionary<BaseFilterID, Filter> filters { get; private set; } = new SortedDictionary<BaseFilterID, Filter>();
        public SortedDictionary<BodyPartID, MuscleAsset> muscleAssets { get; private set; } = new SortedDictionary<BodyPartID, MuscleAsset>();
        public SortedDictionary<MuscleID, Submuscle> submuscles { get; private set; } = new SortedDictionary<MuscleID, Submuscle>();
        public SortedDictionary<BaseMuscleID, SortedDictionary<SubMuscleID, Submuscle>> submusclesClastered { get; private set; } = new SortedDictionary<BaseMuscleID, SortedDictionary<SubMuscleID, Submuscle>>();
        public SortedDictionary<BaseMuscleID, MuscleExtended> musclesExtended { get; private set; } = new SortedDictionary<BaseMuscleID, MuscleExtended>();
        public SortedDictionary<ExerciseID, ExerciseExtended> exercisesExtended { get; private set; } = new SortedDictionary<ExerciseID, ExerciseExtended>();
        public SortedDictionary<ExerciseID, Exercise> exercises { get; private set; } = new SortedDictionary<ExerciseID, Exercise>();
        public Task InitTask { get; private set; }
        public double Progress { get; private set; }
        public bool Success { get; private set; } = false;
        public static SortedDictionary<DictionaryType, MuscleDictionary> Current { get; private set; } = new SortedDictionary<DictionaryType, MuscleDictionary>();
        public static void Clear()
        {
            Current.Clear();
        }
        public static async Task Init()
        {
            ResourceController.IsOffline = true;
            Current.Add(DictionaryType.OFFLINE, new MuscleDictionary());
            await Current[DictionaryType.OFFLINE].InitTask;
            ResourceController.IsOffline = false;
            Current.Add(DictionaryType.ONLINE, new MuscleDictionary());
            await Current[DictionaryType.ONLINE].InitTask;
            ResourceController.IsOffline = true;
            try
            {
                var testJson = await ExternalResourceController.ReadString("ru", "muscles_assets.json");
                if (testJson == null)
                {
                    ResourceController.IsOffline = false;
                }
            }
            catch
            {
                ResourceController.IsOffline = false;
            }
        }
        public static double GetProgress()
        {
            bool r1 = Current.TryGetValue(DictionaryType.OFFLINE, out var v1);
            bool r2 = Current.TryGetValue(DictionaryType.ONLINE, out var v2);
            return ((r1 ? v1.Progress : 0.0) + (r2 ? v2.Progress : 0.0)) * 0.5;
        }
        public static MuscleDictionary GetCurrent()
        {
            return GetCurrent(ResourceController.IsOffline);
        }
        public static MuscleDictionary GetCurrent(bool isOffline)
        {
            return isOffline ? Current[DictionaryType.OFFLINE] : Current[DictionaryType.ONLINE];
        }
        private MuscleDictionary()
        {
            InitTask = ReadMuscleFullInfos();
        }

        private Task ReadMuscleFullInfos()
        {
            return Task.Run(async () =>
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    var testJson = await ResourceController.ReadString("ru", "muscles_assets.json");
                    if (testJson == null)
                    {
                        Progress = 1.0;
                        return;
                    }
                }
                catch
                {
                    Progress = 1.0;
                    return;
                }
                try
                {
                    Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "en"));
                    Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ru"));

                    {
                        Progress = 0.0 / 6;
                        var json = await ResourceController.ReadString("ru", "muscles_assets.json");
                        var rawObject = JsonConvert.DeserializeObject<JObject>(json)["assets"] as JArray;
                        rawObject.ForEach(x => muscleAssets.Add(
                            (BodyPartID)(x["id"].ToString()),
                                JsonConvert.DeserializeObject<MuscleAsset>(
                                    JsonConvert.SerializeObject(x),
                                    new IDConverter<MuscleID>(id => (MuscleID)id),
                                    new IDConverter<BodyPartID>(id => (BodyPartID)id)
                                )
                            )
                        );
                    }
                    {
                        var json = await ResourceController.ReadString("ru", "submuscles.json");
                        var rawObject = JsonConvert.DeserializeObject<JObject>(json)["submuscles"] as JArray;
                        rawObject.ForEach(x => submuscles[(MuscleID)(x["id"].ToString())] = x.ToObject<Submuscle>());
                        int count = submuscles.Count;
                        foreach (var submuscle in submuscles)
                        {
                            Progress = 1.0 / 6 + 1.0 / 6 * ((submuscles.Count - count) / submuscles.Count);
                            var id = submuscle.Key;
                            if (!submusclesClastered.ContainsKey(id.baseID))
                            {
                                submusclesClastered[id.baseID] = new SortedDictionary<SubMuscleID, Submuscle>();
                            }
                            submusclesClastered[id.baseID].Add(id.subID, submuscle.Value);
                            --count;
                        }
                    }
                    {
                        int count = submusclesClastered.Count;
                        foreach (var submuscle in submusclesClastered)
                        {
                            Progress = 2.0 / 6 + 1.0 / 6 * ((submusclesClastered.Count - count) / submusclesClastered.Count);
                            var json = await ResourceController.ReadString("ru", $"muscles/{submuscle.Key}.json");
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
                            --count;
                        }
                    }
                    {
                        var json = await ResourceController.ReadString("ru", "exercises.json");
                        var rawObject = JsonConvert.DeserializeObject<JArray>(json);
                        int count = rawObject.Count;
                        foreach (var exercise in rawObject)
                        {
                            Progress = 3.0 / 6 + 1.0 / 6 * ((rawObject.Count - count) / rawObject.Count);
                            var exercisesId = (ExerciseID)exercise["id"].ToString();
                            exercises.Add(
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
                        int count = exercises.Count;
                        foreach (var exercise in exercises)
                        {
                            Progress = 4.0 / 6 + 1.0 / 6 * ((exercises.Count - count) / exercises.Count);
                            --count;
                            var json = await ResourceController.ReadString("ru", $"training/{exercise.Key}.json");
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
                        var json = await ResourceController.ReadString("ru", $"filters.json");
                        var rawObject = JsonConvert.DeserializeObject<JObject>(json)["filters"] as JArray;
                        int count = rawObject.Count;
                        foreach (var obj in rawObject)
                        {
                            Progress = 5.0 / 6 + 1.0 / 6 * ((rawObject.Count - count) / rawObject.Count);
                            --count;
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
                    Progress = 1.0;
                }
                catch (Exception)
                {
                    Progress = 1.0;
                    return;
                }
                Success = true;
            }));
        }
    }
}

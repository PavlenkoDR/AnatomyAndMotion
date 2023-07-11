using System;
using System.Collections.Generic;
using System.Text;

namespace MuscleAnatomyAndMotion.Controllers
{
    static class MuscleDictionary
    {
        public static SortedDictionary<BaseFilterID, Filter> filters = new SortedDictionary<BaseFilterID, Filter>();
        public static SortedDictionary<BodyPartID, MuscleAsset> muscleAssets = new SortedDictionary<BodyPartID, MuscleAsset>();
        public static SortedDictionary<MuscleID, Submuscle> submuscles = new SortedDictionary<MuscleID, Submuscle>();
        public static SortedDictionary<BaseMuscleID, SortedDictionary<SubMuscleID, Submuscle>> submusclesClastered = new SortedDictionary<BaseMuscleID, SortedDictionary<SubMuscleID, Submuscle>>();
        public static SortedDictionary<BaseMuscleID, MuscleExtended> musclesExtended = new SortedDictionary<BaseMuscleID, MuscleExtended>();
        public static SortedDictionary<ExerciseID, ExerciseExtended> exercisesExtended = new SortedDictionary<ExerciseID, ExerciseExtended>();
        public static SortedDictionary<ExerciseID, Exercise> exercises = new SortedDictionary<ExerciseID, Exercise>();
    }
}

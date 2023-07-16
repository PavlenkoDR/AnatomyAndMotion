using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    public abstract class ViewModelBase : BindableObject, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

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
        public BodyPartID id { get; set; }
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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    public class FilterData
    {
        public ExerciseID exerciseID;
        public List<FilterID> filterIDs;
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FiltersView : ContentView
    {
        public event EventHandler OnCheckedChanged;
        public List<Filter> filters { get; set; }
        private List<FilterID> rawSelectedFilter = new List<FilterID>();
        public List<List<FilterID>> selectedFilters = new List<List<FilterID>>();
        public FiltersView()
        {
            filters = App.filters.Values.ToList();

            InitializeComponent();

            BindingContext = this;
        }
        public List<FilterData> FilterBySelected(List<FilterData> values)
        {
            return values.Where(x => selectedFilters.Where(y => x.filterIDs.Intersect(y).Count() == 0).Count() > 0).ToList();
        }

        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var id = (FilterID)checkBox.ClassId;
            if (e.Value)
            {
                rawSelectedFilter.Add(id);
            }
            else
            {
                rawSelectedFilter.Remove(id);
            }
            selectedFilters.Clear();
            foreach (var filter in filters)
            {
                bool isSomeChecked = false;
                List<FilterID> filtered = new List<FilterID>();
                foreach (var subfilter in filter.subfilters)
                {
                    if (rawSelectedFilter.Contains(subfilter.id))
                    {
                        isSomeChecked = true;
                        filtered.Add(subfilter.id);
                    }
                }
                if (!isSomeChecked)
                {
                    filtered.AddRange(filter.subfilters.Select(x => x.id));
                }
                selectedFilters.Add(filtered);
            }
            OnCheckedChanged.Invoke(this, new EventArgs());
        }
    }
}
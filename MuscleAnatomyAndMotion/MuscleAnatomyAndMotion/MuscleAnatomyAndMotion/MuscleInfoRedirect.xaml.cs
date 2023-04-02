using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MuscleInfoRedirect : ContentView
    {
        public string Description { get; set; }
        public int FullInfoIndex { get; set; }
        public int InfoIndex { get; set; }
        public bool IsButtonEnabled { get => InfoIndex >= 0; }
        public MuscleInfoRedirect(string Description, int FullInfoIndex, int InfoIndex)
        {
            this.Description = Description;
            this.FullInfoIndex = FullInfoIndex;
            this.InfoIndex = InfoIndex;
            BindingContext = this;
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MuscleInfoPage(App.muscleFullInfos[FullInfoIndex], InfoIndex));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static MuscleAnatomyAndMotion.App;

namespace MuscleAnatomyAndMotion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MuscleInfoRedirect : ContentView
    {
        public string Description { get; set; }
        public MuscleID link { get; set; }
        public bool IsButtonEnabled { get => link != null; }
        public MuscleInfoRedirect(string Description, MuscleID link)
        {
            this.Description = Description;
            this.link = link;
            BindingContext = this;
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MuscleInfoRedirectPage(link));
        }
    }
}
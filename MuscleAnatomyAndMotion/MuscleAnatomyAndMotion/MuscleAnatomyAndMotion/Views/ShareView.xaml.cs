using MuscleAnatomyAndMotion.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShareView : ContentView
    {
        public ShareView()
        {
            InitializeComponent();
            ShareImage = ImageSource.FromResource("MuscleAnatomyAndMotion.Assets.share.png");
            Content.BindingContext = this;
        }

        public ImageSource ShareImage { get; private set; }

        public static readonly BindableProperty UrlsProperty = BindableProperty.Create(
            nameof(Urls),
            typeof(List<string>),
            typeof(ShareView),
            defaultBindingMode: BindingMode.TwoWay
        );

        public List<string> Urls
        {
            get
            {
                return (List<string>)GetValue(UrlsProperty);
            }
            set
            {
                SetValue(UrlsProperty, value);
            }
        }

        public static readonly BindableProperty TextProperty = BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(ShareView),
            defaultBindingMode: BindingMode.TwoWay
        );

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            DependencyService.Get<IIntentController>().ShareMediaFile(Text, Urls);
        }
    }
}
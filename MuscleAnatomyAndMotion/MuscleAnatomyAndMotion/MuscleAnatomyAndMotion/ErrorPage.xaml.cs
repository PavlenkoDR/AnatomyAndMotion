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
    public partial class ErrorPage : ContentPage
    {
        public ErrorPage()
        {
            InitializeComponent();

            var obbDirs = DependencyService.Get<IExternalResourceReader>().GetObbDirs();

            var message = "Положите файл 'main.1.com.companyname.muscleanatomyandmotion.obb' в одну из папок и перезагрузите приложение:\n";
            foreach (var path in obbDirs)
            {
                message += $"{path}\n";
            }
            message += "Если файл в папке лежит, попробуйте другую из списка";
            errorLabel.Text = message;

        }
    }
}
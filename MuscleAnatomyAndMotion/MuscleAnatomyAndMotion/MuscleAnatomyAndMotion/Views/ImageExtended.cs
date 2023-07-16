using MuscleAnatomyAndMotion.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion
{
    public class ImageExtended : Image
    {
        public ImageExtended() : base()
        {

        }
        public static readonly BindableProperty ImgSourceFromObbProperty = BindableProperty.Create(
            nameof(ImgSourceFromObb),
            typeof(string),
            typeof(ImageExtended),
            propertyChanged: (bindableObject, oldValue, newValue) =>
            {
                Device.BeginInvokeOnMainThread(async () => {
                    if (newValue == null || newValue.ToString() == "")
                    {
                        (bindableObject as ImageExtended).Source = "";
                    }
                    else
                    {
                        (bindableObject as ImageExtended).Source = await ResourceController.GetImageSource("ru", $"{newValue}");
                    }
                });
            }
        );

        public string ImgSourceFromObb
        {
            get
            {
                return GetValue(ImgSourceFromObbProperty)?.ToString();
            }
            set 
            {
                SetValue(ImgSourceFromObbProperty, value);
            }
        }
    }
}

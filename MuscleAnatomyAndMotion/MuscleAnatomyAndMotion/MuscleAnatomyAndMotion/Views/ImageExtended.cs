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
                Task.Run(async () => {
                    if (newValue == null || newValue == "")
                    {
                        (bindableObject as ImageExtended).Source = "";
                    }
                    else
                    {
                        (bindableObject as ImageExtended).Source = await ExternalResourceController.GetImageSource("ru", $"{newValue}");
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

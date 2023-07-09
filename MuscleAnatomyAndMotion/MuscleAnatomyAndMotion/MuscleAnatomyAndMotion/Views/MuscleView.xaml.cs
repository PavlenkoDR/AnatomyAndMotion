﻿using MuscleAnatomyAndMotion.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuscleAnatomyAndMotion.Views
{
    public class MuscleViewModel : MuscleExtended
    {

        public MuscleID subID { get; set; }
        public MuscleViewModel SetSubID(MuscleID id)
        {
            subID = id;
            isUseSubDescription = true;
            return this;
        }
        private bool _isSelected = true;
        public bool isSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                RaisePropertyChanged("isSelected");
            }
        }
        public string image
        {
            get
            {
                MusclePart part = null;
                if (subID != null)
                {
                    part = parts?.Where(x => x.id.CompareTo(subID) == 0).First();
                }
                else
                {
                    if (thumbnail_image_url != null && thumbnail_image_url != "")
                    {
                        return thumbnail_image_url;
                    }
                    part = parts?.ElementAt(0);
                }
                {
                    var tbn = part?.thumbnail_image_url;
                    if (tbn != null && tbn != "")
                    {
                        return tbn;
                    }
                }
                {
                    var tbn = part?.image_url;
                    if (tbn != null && tbn != "")
                    {
                        return tbn;
                    }
                }
                if (thumbnail_image_url != null && thumbnail_image_url != "")
                {
                    return thumbnail_image_url;
                }
                return "";
            }
        }
        public string subMuscleName
        {
            get
            {
                if (subID != null)
                {
                    var part = parts?.Where(x => x.id.CompareTo(subID) == 0).First();
                    var result = part?.name;
                    if (result != null)
                    {
                        return result;
                    }
                }
                return name;
            }
        }
        public class SubMuscleDescription
        {
            public string origin { get; set; }
            public string insertion { get; set; }
        }

        public SubMuscleDescription subMuscleDescription
        {
            get
            {
                var part = parts?.Where(x => x.id.CompareTo(subID ?? (MuscleID)id.id) == 0).First();
                var result = part?.name;
                return new SubMuscleDescription() { origin = part?.origin, insertion = part?.insertion };
            }
        }
        public bool isFavorite
        {
            get => subID != null ?
                LocalFilesController.favoriteData.subMuscleIDs.Contains(subID) :
                LocalFilesController.favoriteData.muscleIDs.Contains(id);
            set
            {
                if (value)
                {
                    if (subID == null)
                    {
                        if (!LocalFilesController.favoriteData.muscleIDs.Contains(id))
                        {
                            LocalFilesController.favoriteData.muscleIDs.Add(id);
                        }
                    }
                    else
                    {
                        if (!LocalFilesController.favoriteData.subMuscleIDs.Contains(subID))
                        {
                            LocalFilesController.favoriteData.subMuscleIDs.Add(subID);
                        }
                    }
                }
                else
                {
                    LocalFilesController.favoriteData.muscleIDs.Remove(id);
                    LocalFilesController.favoriteData.subMuscleIDs.Remove(subID);
                }
                RaisePropertyChanged("isFavorite");
            }
        }
        private bool _isUseSubDescription = false;
        public bool isUseSubDescription
        {
            get => _isUseSubDescription;
            set
            {
                _isUseSubDescription = value;
                RaisePropertyChanged("isUseBaseDescription");
                RaisePropertyChanged("isUseSubDescription");
            }
        }
        public bool isUseBaseDescription
        {
            get => !_isUseSubDescription;
            set
            {
                _isUseSubDescription = !value;
                RaisePropertyChanged("isUseBaseDescription");
                RaisePropertyChanged("isUseSubDescription");
            }
        }
        public string shareText { get => $"{name}\n\n{description.structure}\n\n{description.actions}"; }
        public List<string> shareUrls { get => new List<string>() { image }; }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MuscleView : ContentView, INotifyPropertyChanged
    {
        public MuscleView()
        {
            InitializeComponent();
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        public static readonly BindableProperty ModelProperty = BindableProperty.Create(
            nameof(Model),
            typeof(MuscleViewModel),
            typeof(MuscleView),
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: (bindableObject, oldValue, newValue) =>
            {
                (bindableObject as MuscleView).updateModel();
            }
        );

        public MuscleViewModel Model
        {
            get
            {
                return (MuscleViewModel)GetValue(ModelProperty);
            }
            set
            {
                SetValue(ModelProperty, value);
                updateModel();
            }
        }
        private void updateModel()
        {
            Content.BindingContext = Model;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            Model = BindingContext as MuscleViewModel ?? Model;
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            ((Application.Current.MainPage as MainFlyoutPage).Detail as NavigationPage).PushAsync(new MuscleInfoPage(Model.id, Model.subID?.subID ?? (SubMuscleID)"0"));
        }
    }
}
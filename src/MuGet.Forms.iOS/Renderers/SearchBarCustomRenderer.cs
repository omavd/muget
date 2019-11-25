﻿using System.ComponentModel;
using Foundation;
using MuGet.Forms.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(SearchBar), typeof(SearchBarCustomRenderer))]
namespace MuGet.Forms.iOS.Renderers
{
    [Preserve(AllMembers = true)]
    public class SearchBarCustomRenderer : SearchBarRenderer
    {
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == SearchBar.TextProperty.PropertyName)
            {
                Control.AutocorrectionType = UITextAutocorrectionType.No;
                Control.ShowsCancelButton = false;
            }
        }
    }
}
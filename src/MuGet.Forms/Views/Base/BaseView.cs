﻿using MuGet.Forms.Services;
using MuGet.Forms.ViewModels;
using System;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace MuGet.Forms.Views
{
    public abstract class BaseView<T> : ContentView where T : BaseViewModel
    {
        private ContentPage _parentPage;
        private bool _registered;

        public T ViewModel => BindingContext as T;

        public BaseView()
        {            
        }       
        
        private void OnAppearing(object sender, EventArgs e)
        {
            ViewModel.OnAppearing();
        }

        private void OnDisappearing(object sender, EventArgs e)
        {
            ViewModel.OnDisappearing();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName.Equals("Renderer", StringComparison.OrdinalIgnoreCase))
            {
                var rr = DependencyService.Get<IRendererResolver>();

                if (rr.HasRenderer(this))
                {
                    Unregister();

                    var parent = Parent;
                    while (parent?.Parent != null && !(parent is ContentPage))
                    {
                        parent = parent.Parent;
                    }

                    if (parent is ContentPage page)
                    {
                        _parentPage = page;
                        Register();
                    }

                    ViewModel.OnCreate();
                    ViewModel.OnAppearing();
                }
                else
                {
                    Unregister();

                    ViewModel.OnDisappearing();
                    ViewModel.OnDestory();
                }
            }
        }

        private void Register()
        {
            if (_parentPage != null && !_registered)
            {                
                _parentPage.Appearing += OnAppearing;
                _parentPage.Disappearing += OnDisappearing;

                _registered = true;
            }
        }

        private void Unregister()
        {
            if (_parentPage != null && _registered)
            {                
                _parentPage.Appearing -= OnAppearing;
                _parentPage.Disappearing -= OnDisappearing;

                _registered = false;
                _parentPage = null;
            }
        }
    }
}

﻿namespace RambleJungle.ViewModel
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using RambleJungle.Base;
    using System;
    using System.Windows;

    public class JungleObjectStatusViewModel : ObservableRecipient, IDisposable
    {
        private readonly JungleModel jungleModel = Ioc.Default.GetService<JungleModel>() ??
            throw new Exception(string.Format(Consts.ServiceNotFound, nameof(JungleModel)));

        private readonly JungleObject jungleObject;

        public string Name => jungleObject.Name;
        public FrameworkElement Shape => ShapesHelper.GetShape(jungleObject.JungleObjectType);
        public int Count => jungleModel.CountOf(jungleObject.JungleObjectType);

        public JungleObjectStatusViewModel(JungleObject firstJungleObject)
        {
            jungleObject = firstJungleObject;
            if (firstJungleObject != null)
            {
                foreach (JungleObject jungleObject in jungleModel.GetJungleObjects(jungleObject.JungleObjectType))
                {
                    jungleObject.StatusChanged += StatusChanged;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (JungleObject jungleObject in jungleModel.GetJungleObjects(jungleObject.JungleObjectType))
                {
                    jungleObject.StatusChanged -= StatusChanged;
                }
            }
        }

        private void StatusChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Count));
        }
    }
}
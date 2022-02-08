﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using RambleJungle.Model;
using System;
using System.Windows;

namespace RambleJungle.ViewModel
{
    public class ActionViewModel : ObservableRecipient
    {
        private readonly GameModel gameModel;
        private JungleObjectViewModel? forgottenCityViewModel;

        private JungleObjectViewModel? currentJungleObject;
        public JungleObjectViewModel? CurrentJungleObject
        {
            get => currentJungleObject;
            set => SetProperty(ref currentJungleObject, value);
        }

        private Visibility actionVisibility = Visibility.Hidden;
        public Visibility ActionVisibility
        {
            get => actionVisibility;
            set
            {
                SetProperty(ref actionVisibility, value);
                if (value == Visibility.Visible)
                {
                    OnPropertyChanged(nameof(CurrentJungleObject));
                }
            }
        }

        public ActionViewModel(GameModel gameModel)
        {
            this.gameModel = gameModel;
            if (gameModel != null)
            {
                gameModel.ForgottenCityModeChanged += ForgottenCityModeChanged;
            }
        }

        private void ForgottenCityModeChanged(object? sender, EventArgs e)
        {
            if (gameModel.IsForgottenCityMode)
            {
                if (forgottenCityViewModel == null)
                {
                    forgottenCityViewModel = CurrentJungleObject;
                }
                CurrentJungleObject = new JungleObjectViewModel(gameModel.CurrentJungleObject);
            }
            else
            {
                CurrentJungleObject = forgottenCityViewModel;
                forgottenCityViewModel = null;
            }
        }
    }
}

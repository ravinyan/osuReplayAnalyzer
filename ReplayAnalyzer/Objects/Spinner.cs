﻿using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.Skins;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ReplayAnalyzer;

namespace ReplayAnalyzer.Objects
{
    public class Spinner : HitObject
    {
        public Spinner(SpinnerData spinnerData)
        {
            X = spinnerData.X;
            Y = spinnerData.Y;
            SpawnPosition = spinnerData.SpawnPosition;
            SpawnTime = spinnerData.SpawnTime;

            EndTime = spinnerData.EndTime;
        }

        public int EndTime { get; set; }

        private static MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static Spinner CreateSpinner(SpinnerData spinner, double radius, int i)
        {
            Spinner spinnerObject = new Spinner(spinner);
            spinnerObject.Width = Window.playfieldCanva.Width;
            spinnerObject.Height = Window.playfieldCanva.Height;
            spinnerObject.Name = $"SpinnerHitObject{i}";

            double acRadius = radius * 6;
            Image approachCircle = new Image()
            {
                Source = new BitmapImage(new Uri(SkinElement.SpinnerApproachCircle())),
                Width = acRadius,
                Height = acRadius,
            };

            Image background = new Image()
            {
                Source = new BitmapImage(new Uri(SkinElement.SpinnerBackground())),
                Width = Window.playfieldCanva.Width,
                Height = Window.playfieldCanva.Height,
            };

            double rbRadius = radius * 3;
            Image rotatingBody = new Image()
            {
                Source = new BitmapImage(new Uri(SkinElement.SpinnerCircle())),
                Width = rbRadius,
                Height = rbRadius,
            };

            spinnerObject.Visibility = Visibility.Collapsed;

            spinnerObject.Children.Add(rotatingBody);
            spinnerObject.Children.Add(background);
            spinnerObject.Children.Add(approachCircle);

            HitObjectAnimations.ApplySpinnerAnimations(spinnerObject);

            SetLeft(approachCircle, spinner.SpawnPosition.X - acRadius / 2);
            SetTop(approachCircle, spinner.SpawnPosition.Y - acRadius / 2);

            SetLeft(rotatingBody, spinner.SpawnPosition.X - rbRadius / 2);
            SetTop(rotatingBody, spinner.SpawnPosition.Y - rbRadius / 2);

            SetLeft(spinnerObject, spinner.SpawnPosition.X - Window.playfieldCanva.Width / 2);
            SetTop(spinnerObject, spinner.SpawnPosition.Y - Window.playfieldCanva.Height / 2);

            return spinnerObject;
        }
    }
}

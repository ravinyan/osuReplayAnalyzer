using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.SettingsMenu;
using System.Configuration;
using System.Windows;
using System.Windows.Input;

namespace ReplayAnalyzer.KeyboardShortcuts
{
    public class ShortcutManager
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static void Initialize()
        {
            Window.KeyDown += ShortcutPicker;
        }

        public static void ShortcutPicker(object sender, KeyEventArgs e)
        {
            string optionName = "";
            foreach (KeyValueConfigurationElement keyValue in SettingsOptions.config.AppSettings.Settings)
            {
                if (keyValue.Value == e.Key.ToString())
                {
                    optionName = keyValue.Key;
                    break;
                }
            }

            switch (optionName)
            {
                case "Open Menu":
                    SettingsButton.OpenClose();
                    break;
                case "Pause":
                    PlayPauseControls.PlayPauseButton(null!, null!);
                    break;
                case "Jump 1 Frame to Left":
                    SongSliderControls.SeekByFrame(-727);
                    break;
                case "Jump 1 Frame to Right":
                    SongSliderControls.SeekByFrame(727);
                    break;
                case "Jump to closest past miss":
                    FindClosestMiss(-727);
                    break;
                case "Jump to closest future miss":
                    FindClosestMiss(727);
                    break;
                case "Rate Change -0.25x":
                    RateChangerControls.ChangeRateShortcut(-727);
                    break;
                case "Rate Change +0.25x":
                    RateChangerControls.ChangeRateShortcut(727);
                    break;
                default:
                    break;
            }
        }

        // other functions fit for their respective classes, this one tho doesnt fit anywhere so will just leave it here
        private static void FindClosestMiss(int direction)
        {
            if (GamePlayClock.IsPaused() == false)
            {
                GamePlayClock.Pause();
                MusicPlayer.MusicPlayer.Pause();
                Window.playerButton.Style = Window.Resources["PlayButton"] as Style;
            }
            
            FindMiss(direction);
        }

        private static void FindMiss(int direction)
        {
            long time = (long)GamePlayClock.TimeElapsed;

            // long long maaaaaaaaaaaan
            HitObjectData? banana = direction > 0
                ? MainWindow.map.HitObjects!.FirstOrDefault(ho => time < ho.SpawnTime && ho.Judgement == (int)HitJudgementManager.HitObjectJudgement.Miss) ?? null
                : MainWindow.map.HitObjects!.LastOrDefault(ho => time > ho.SpawnTime && ho.Judgement == (int)HitJudgementManager.HitObjectJudgement.Miss) ?? null;

            if (banana != null)
            {
                HitObjectManager.ClearAliveObjects();
                HitObjectManager.GetAliveDataObjects().Clear();

                GamePlayClock.Seek(banana.SpawnTime);
                Window.songSlider.Value = banana.SpawnTime;
                HitObjectSpawner.CatchUpToAliveHitObjects(banana.SpawnTime);

                // LastOrDefault updates cursor position correctly even tho it is performance hit especially on long maps... need to improve one day < who am i lying to i wont touch this
                ReplayFrame f = MainWindow.replay.FramesDict.LastOrDefault(f => f.Value.Time <= banana.SpawnTime).Value ?? MainWindow.replay.FramesDict[0];
                CursorManager.UpdateCursorPositionAfterSeek(f);

                HitMarkerManager.UpdateHitMarkerAfterSeek(direction, banana.SpawnTime);

                HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());
            }
        }
    }
}

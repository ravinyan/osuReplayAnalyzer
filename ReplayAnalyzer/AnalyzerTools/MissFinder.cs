using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldGameplay;
using System.Windows;

namespace ReplayAnalyzer.AnalyzerTools
{
    public class MissFinder
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        // other functions fit for their respective classes, this one tho doesnt fit anywhere so will just leave it here
        public static void FindClosestMiss(int direction)
        {
            if (SettingsMenu.SettingsPanel.SettingsPanelBox.IsVisible)
            {
                // dont do this by accident if user changes slider values using key arrows in settings menu
                return;
            }

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
            if (MainWindow.map == null)
            { 
                return; 
            }

            long time = (long)GamePlayClock.TimeElapsed;

            // long long maaaaaaaaaaaan
            HitObjectData? banana = direction > 0
                ? MainWindow.map.HitObjects!.FirstOrDefault(ho => time < ho.SpawnTime && ho.Judgement == (int)HitJudgementManager.HitObjectJudgement.Miss) ?? null
                : MainWindow.map.HitObjects!.LastOrDefault(ho => time > ho.SpawnTime && ho.Judgement == (int)HitJudgementManager.HitObjectJudgement.Miss) ?? null;

            if (banana != null)
            {
                HitObjectManager.ClearAliveObjects();

                GamePlayClock.Seek(banana.SpawnTime);
                Window.songSlider.Value = banana.SpawnTime;
                HitObjectSpawner.CatchUpToAliveHitObjects(banana.SpawnTime);

                // LastOrDefault updates cursor position correctly even tho it is performance hit especially on long maps... need to improve one day < who am i lying to i wont touch this
                ReplayFrame f = MainWindow.replay.FramesDict.LastOrDefault(f => f.Value.Time <= banana.SpawnTime).Value ?? MainWindow.replay.FramesDict[0];
                CursorManager.UpdateCursorPositionAfterSeek(f);

                HitMarkerManager.UpdateHitMarkerAfterSeek(direction, banana.SpawnTime);
                FrameMarkerManager.GetFrameMarkerAfterSeek(f);
                CursorPathManager.GetCursorPathAfterSeek(f);

                HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());
            }
        }
    }
}

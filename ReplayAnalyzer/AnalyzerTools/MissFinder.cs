using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows;
using System.Windows.Controls;

namespace ReplayAnalyzer.AnalyzerTools
{
    public class MissFinder
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static int index = 0;
        private static List<HitObjectData> MissedHitObjects = null;

        public static void ResetFields()
        {
            MissedHitObjects = null;
        }

        public static int UpdateIndex(double time, int direction)
        {
            if (MissedHitObjects == null)
            {
                MissedHitObjects = MainWindow.map.HitObjects.Where(ho => ho.Judgement.HitJudgement == 0 || ho is SliderData s && s.AllTicksHit == false).ToList();
            }

            int index = -1;
            if (direction > 0)
            {
                for (int i = 0; i < MissedHitObjects.Count; i++)
                {
                    if (MissedHitObjects[i].SpawnTime > time)
                    {
                        index = i;
                        break;
                    }
                }
            }
            else
            {
                for (int i = MissedHitObjects.Count - 1; i >= 0; i--)
                {
                    if (MissedHitObjects[i].SpawnTime < time)
                    {
                        index = i;
                        break;
                    }
                }
            }

            return index;
        }

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

            int index = UpdateIndex(GamePlayClock.TimeElapsed, direction);
            if (index == -1)
            {
                return;
            }

            HitObjectData banana = MissedHitObjects[index];

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

            MusicPlayer.MusicPlayer.Seek(f.Time);

            HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());
        }
    }
}

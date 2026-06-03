using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows;

namespace ReplayAnalyzer.AnalyzerTools
{
    public class MissFinder
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static List<HitObjectData> MissedHitObjects = null;

        public static void ResetFields()
        {
            MissedHitObjects = null;
        }

        public static int UpdateIndex(double time, int direction)
        {
            if (MissedHitObjects == null)
            {// thats a chunker... i think making for loop would be better... nah people say how linq is good so wont change it...
                MissedHitObjects = MainWindow.map.HitObjects.Where(
                    ho => ho.Judgement.Judgement == 0 
                || (ho is SliderData s && (s.AllTicksHit == false 
                || (StrictTrackingMod.IsStrictTrackingEnabled == true && s.SliderEndJudgement.Judgement == -2)))).ToList();
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
            if (SettingsMenu.SettingsPanel.SettingsPanelBox.IsVisible || MainWindow.map.FileVersion == -1)
            {
                // return when options menu is open coz user can use arrows to change sliders or when map was not initialized
                return;
            }

            int missIndex = UpdateIndex(GamePlayClock.TimeElapsed, direction);
            if (missIndex == -1)
            {
                return;
            }

            if (GamePlayClock.IsPaused() == false)
            {
                GamePlayClock.Pause();
                MusicPlayer.MusicPlayer.Pause();
                PlayPauseControls.ChangeButtonStyle();
            }

            FindMiss(direction, missIndex);
        }

        private static void FindMiss(int direction, int index)
        {
            HitObjectManager.ClearAliveObjects();

            HitObjectData missedHitObject = MissedHitObjects[index];

            GamePlayClock.Seek(missedHitObject.SpawnTime);
            Window.songSlider.Value = missedHitObject.SpawnTime;
            HitObjectSpawner.CatchUpToAliveHitObjects(missedHitObject.SpawnTime);

            ReplayFrame f = MainWindow.replay.FramesDict.LastOrDefault(f => f.Value.Time <= missedHitObject.SpawnTime).Value ?? MainWindow.replay.FramesDict[0];
            CursorManager.UpdateCursorPositionAfterSeek(f);
            HitMarkerManager.UpdateHitMarkerAfterSeek(f.Time);
            FrameMarkerManager.HandleAliveFrameMarkers(direction, f);
            CursorPathManager.UpdateIndexAfterSeek(direction, f);
            Slider.UpdateAliveSliderEvents();

            MusicPlayer.MusicPlayer.Seek(f.Time);
        }
    }
}

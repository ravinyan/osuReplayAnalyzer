using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Catch;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using ReplayAnalyzer.SettingsMenu;
using System.Windows.Controls;

namespace ReplayAnalyzer.PlayfieldUI.GamePlayfields
{
    public class PlayfieldManager
    {
        public static bool IsReplayPlayingForward { get; set; } = true;

        private static GameMode PreviousGamemode = GameMode.None;

        public static Canvas GetActivePlayfield()
        {
            switch (MainWindow.replay.GameMode)
            {
                case GameMode.Osu:
                    return OsuPlayfield.Playfield;
                case GameMode.OsuMania:
                    return ManiaPlayfield.Playfield;
                case GameMode.OsuTaiko:
                    return TaikoPlayfield.Playfield;
                case GameMode.OsuCatch:
                    return CatchPlayfield.Playfield;
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        public static bool CreatePlayfield()
        {
            GameMode mode = MainWindow.replay.GameMode;
            if (PreviousGamemode != GameMode.None && PreviousGamemode != mode)
            {
                ClearPreviousPlayfield(PreviousGamemode);
            }

            PreviousGamemode = mode;
            // if gamemode is different that what previous gamemode was then previous one should be cleared
            // also mania gamemode should be always cleared i guess coz im too lazy to make code to not clear it
            // if key count is same as previous replay
            switch (mode)
            {
                case GameMode.Osu:
                    EnableOsuUIElements();
                    return OsuPlayfield.Create();
                case GameMode.OsuMania:
                    HideOsuUIElements();
                    return ManiaPlayfield.Create();
                case GameMode.OsuTaiko:
                    HideOsuUIElements();
                    return TaikoPlayfield.Create();
                case GameMode.OsuCatch:
                    HideOsuUIElements();
                    CatchRNG.ResetSeed();
                    return CatchPlayfield.Create();
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        public static void UpdateLoop()
        {
            switch (MainWindow.replay.GameMode)
            {
                case GameMode.Osu:
                    OsuPlayfield.UpdateGameplayLoop();
                    break;
                case GameMode.OsuMania: 
                    ManiaPlayfield.UpdateGameplayLoop();
                    break;
                case GameMode.OsuTaiko:
                    TaikoPlayfield.UpdateGameplayLoop();
                    break;
                case GameMode.OsuCatch:
                    CatchPlayfield.UpdateGameplayLoop();
                    break;
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        public static void PreloadLoop()
        {
            IsReplayPlayingForward = true;
            switch (MainWindow.replay.GameMode)
            {
                case GameMode.Osu:
                    OsuPlayfield.PreloadReplay();
                    break;
                case GameMode.OsuMania:
                    ManiaPlayfield.PreloadReplay();
                    break;
                case GameMode.OsuTaiko:
                    TaikoPlayfield.PreloadReplay();
                    break;
                case GameMode.OsuCatch:
                    CatchPlayfield.PreloadReplay();
                    break;
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        public static void UpdateClickUI(bool isSeekingForward = false)
        {
            switch (MainWindow.replay.GameMode)
            {
                case GameMode.Osu:
                    KeyOverlay.UpdateHoldPositions(isSeekingForward);
                    break;
                // for these game modes clicks are shown very well outside of catch dashes but IT IS visible
                // when catcher speeds up (dashes) even for me and i dont play catch even if playing replay frame by frame
                case GameMode.OsuMania:
                    break;
                case GameMode.OsuTaiko:
                    break;
                case GameMode.OsuCatch:
                    break;
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        public static void ResizePlayfield()
        {
            switch (MainWindow.replay.GameMode)
            {
                case GameMode.Osu:
                    OsuPlayfield.Resize();
                    break;
                case GameMode.OsuMania:
                    ManiaPlayfield.Resize();
                    break;
                case GameMode.OsuTaiko:
                    TaikoPlayfield.Resize();
                    break;
                case GameMode.OsuCatch:
                    CatchPlayfield.Resize();
                    break;
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        // might be deleted? will see after taiko and catch is done
        public static void SeekGameplay(double direction, ReplayFrame f, bool seekByFrame = false)
        {
            switch (MainWindow.replay.GameMode)
            {
                case GameMode.Osu:
                    // this has some annoying variations so will either think how to do it cleanly or will do it a bit less cleanly
                    break;
                case GameMode.OsuMania:
                    ManiaPlayfield.SeekGameplay(direction, f);
                    break;
                case GameMode.OsuTaiko:
                    TaikoPlayfield.SeekGameplay(direction, f);
                    break;
                case GameMode.OsuCatch:
                    CatchPlayfield.SeekGameplay(direction, f);
                    break;
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        private static void ClearPreviousPlayfield(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Osu:
                    OsuPlayfield.Dispose();
                    break;
                case GameMode.OsuMania:
                    ManiaPlayfield.Dispose();
                    break;
                case GameMode.OsuTaiko:
                    TaikoPlayfield.Dispose();
                    break;
                case GameMode.OsuCatch:
                    CatchPlayfield.Dispose();
                    break;
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        private static void HideOsuUIElements()
        {
            HitMap.HitMapUI.Visibility = System.Windows.Visibility.Collapsed;
            KeyOverlay.KeyOverlayUI.Visibility = System.Windows.Visibility.Collapsed;
        }

        private static void EnableOsuUIElements()
        {
            if (SettingsOptions.GetConfigValue("ShowHitMap") == "true")
            {
                HitMap.HitMapUI.Visibility = System.Windows.Visibility.Visible;
            }

            if (SettingsOptions.GetConfigValue("ShowKeyOverlay") == "true")
            {
                KeyOverlay.KeyOverlayUI.Visibility = System.Windows.Visibility.Visible;
            }    
        }
    }
}

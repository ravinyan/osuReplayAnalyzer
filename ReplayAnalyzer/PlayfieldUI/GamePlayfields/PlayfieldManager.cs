using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using ReplayAnalyzer.SettingsMenu;
using System.Diagnostics;
using System.Windows.Controls;

namespace ReplayAnalyzer.PlayfieldUI.GamePlayfields
{
    public class PlayfieldManager
    {
        private static GameMode PreviousGamemode = GameMode.None;

        public static Canvas GetActivePlayfield()
        {
            GameMode mode = MainWindow.replay.GameMode;
            switch (mode)
            {
                case GameMode.Osu:
                    return OsuPlayfield.Playfield;
                case GameMode.OsuMania:
                    return ManiaPlayfield.Playfield;
                case GameMode.OsuTaiko:
                    return TaikoPlayfield.Playfield;
                case GameMode.OsuCatch:
                    return new Canvas();
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
                    return false;
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        public static void UpdateLoop()
        {
            GameMode mode = MainWindow.replay.GameMode;
            switch (mode)
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
                    break;
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        public static void PreloadLoop()
        {
            GameMode mode = MainWindow.replay.GameMode;
            switch (mode)
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
                    break;
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        public static void UpdateClickUI(bool isSeekingForward = false)
        {
            GameMode mode = MainWindow.replay.GameMode;
            switch (mode)
            {
                case GameMode.Osu:
                    KeyOverlay.UpdateHoldPositions(isSeekingForward);
                    break;
                case GameMode.OsuMania:
                    ManiaPlayfield.UpdateClickUI(isSeekingForward);
                    break;
                case GameMode.OsuTaiko:
                    TaikoPlayfield.UpdateClickUI(isSeekingForward);
                    break;
                case GameMode.OsuCatch:
                    break;
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        public static void ResizePlayfield()
        {
            GameMode mode = MainWindow.replay.GameMode;
            switch (mode)
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
                    break;
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        // might be deleted? will see after taiko and catch is done
        public static void SeekGameplay(double direction, ReplayFrame f, bool seekByFrame = false)
        {
            GameMode mode = MainWindow.replay.GameMode;
            switch (mode)
            {
                case GameMode.Osu:
                    OsuPlayfield.SeekGameplay(direction, f);
                    if (seekByFrame == true)
                    {
                        KeyOverlay.UpdateHoldPositions(true);
                    }
                    else
                    {
                        //Slider.UpdateAliveSliderEvents();
                    }
                    break;
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

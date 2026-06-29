using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.PlayfieldUI.UIElements;
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
                    return OsuPlayfield.Create();
                case GameMode.OsuMania:
                    return ManiaPlayfield.Create();
                case GameMode.OsuTaiko:
                    return TaikoPlayfield.Create();
                case GameMode.OsuCatch:
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

        public static void UpdateClickUI()
        {
            GameMode mode = MainWindow.replay.GameMode;
            switch (mode)
            {
                case GameMode.Osu:
                    KeyOverlay.UpdateHoldPositions();
                    break;
                case GameMode.OsuMania:
                    ManiaPlayfield.UpdateClickUI();
                    break;
                case GameMode.OsuTaiko:
                    TaikoPlayfield.UpdateClickUI();
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
    }
}

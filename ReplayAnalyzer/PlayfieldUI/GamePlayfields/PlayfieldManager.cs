using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.HitObjects.Osu;
using ReplayAnalyzer.PlayfieldUI.UIElements;

namespace ReplayAnalyzer.PlayfieldUI.GamePlayfields
{
    public class PlayfieldManager
    {
        private static GameMode PreviousGamemode = GameMode.None;

        public static void CreatePlayfield(GameMode mode)
        {
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
                    OsuPlayfield.Create();
                    break;
                case GameMode.OsuMania:
                    ManiaPlayfield.Create();
                    break;
                case GameMode.OsuTaiko:
                    break;
                case GameMode.OsuCatch:
                    break;
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        public static void UpdateLoop(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Osu:
                    OsuPlayfield.UpdateGameplayLoop();
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

        public static void PreloadLoop(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Osu:
                    OsuPlayfield.PreloadReplay();
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

        public static void UpdateClickUI(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Osu:
                    KeyOverlay.UpdateHoldPositions();
                    break;
                case GameMode.OsuMania:
                    ManiaPlayfield.UpdateClickUI();
                    break;
                case GameMode.OsuTaiko:
                    break;
                case GameMode.OsuCatch:
                    break;
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        public static void ResizePlayfield(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Osu:
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

        public static void SeekGameplay(GameMode mode, double direction, ReplayFrame f, bool seekByFrame = false)
        {
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
                        Slider.UpdateAliveSliderEvents();
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
                    break;
                case GameMode.OsuCatch:
                    break;
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }
    }
}

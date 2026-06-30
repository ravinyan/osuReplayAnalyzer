using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Osu;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ReplayAnalyzer.GameplaySkin
{
    public static class SkinElement
    {
        private static string CurrentSkinFolderPath = "";
        private static string DefaultSkinFolderPath = $"{AppContext.BaseDirectory}\\Skins\\Komori - PeguLian II (PwV)";

        // well... i dont see better way to do this
        // this is special thing for hit circle, rest is just cached BitmapSource
        private static bool IsCircleColourSaved = false;
        private static WriteableBitmap[] HitCirclesColoured = Array.Empty<WriteableBitmap>();

        private static Dictionary<SkinElements, BitmapSource> SkinElementsDictionary = new Dictionary<SkinElements, BitmapSource>()
        {
            // std (which contains elements for other gamemodes based on what osu wiki says)
            { SkinElements.Cursor,                     null! },
            { SkinElements.ApproachCircle,             null! }, // osu! osu!taiko
            { SkinElements.HitCircle,                  null! },
            { SkinElements.HitCircleOverlay,           null! },
            { SkinElements.ComboNumber0,               null! },
            { SkinElements.ComboNumber1,               null! },
            { SkinElements.ComboNumber2,               null! },
            { SkinElements.ComboNumber3,               null! },
            { SkinElements.ComboNumber4,               null! },
            { SkinElements.ComboNumber5,               null! },
            { SkinElements.ComboNumber6,               null! },
            { SkinElements.ComboNumber7,               null! },
            { SkinElements.ComboNumber8,               null! },
            { SkinElements.ComboNumber9,               null! },
            { SkinElements.ReverseArrow,               null! },
            { SkinElements.SliderBall,                 null! },
            { SkinElements.SliderBallCircle,           null! },
            { SkinElements.Hit300,                     null! },
            { SkinElements.Hit100,                     null! },
            { SkinElements.Hit50 ,                     null! },
            { SkinElements.Hit0 ,                      null! },
            { SkinElements.SliderEndMiss,              null! },
            { SkinElements.SliderTickMiss,             null! },
            { SkinElements.SliderTick,                 null! }, // osu! osu!taiko
            { SkinElements.SpinnerApproachCircle,      null! }, // osu! osu!taiko
            { SkinElements.SpinnerBackground,          null! },
            { SkinElements.SpinnerCircle,              null! }, // osu! osu!taiko
            { SkinElements.Lighting,                   null! }, // osu! osu!taiko osu!catch <idk if i want that 
                                                                
            // mania
            { SkinElements.ManiaHit0,                  null! },
            { SkinElements.ManiaHit50,                 null! },
            { SkinElements.ManiaHit100,                null! },
            { SkinElements.ManiaHit200,                null! },
            { SkinElements.ManiaHit300,                null! },
            { SkinElements.ManiaHit320,                null! },
            { SkinElements.ManiaKey1Idle,              null! },
            { SkinElements.ManiaKey1Pressed,           null! },
            { SkinElements.ManiaKey2Idle,              null! },
            { SkinElements.ManiaKey2Pressed,           null! },
            { SkinElements.ManiaKey3Idle,              null! },
            { SkinElements.ManiaKey3Pressed,           null! },
            { SkinElements.ManiaNote1,                 null! },
            { SkinElements.ManiaNote2,                 null! },
            { SkinElements.ManiaNote3,                 null! },
            { SkinElements.ManiaLongNoteHead1,         null! },
            { SkinElements.ManiaLongNoteHead2,         null! },
            { SkinElements.ManiaLongNoteHead3,         null! },
            { SkinElements.ManiaLongNoteBody1,         null! },
            { SkinElements.ManiaLongNoteBody2,         null! },
            { SkinElements.ManiaLongNoteBody3,         null! },
            { SkinElements.ManiaLongNoteTail1,         null! },
            { SkinElements.ManiaLongNoteTail2,         null! },
            { SkinElements.ManiaLongNoteTail3,         null! },
            { SkinElements.ManiaStageLeft,             null! },
            { SkinElements.ManiaStageRight,            null! },
            { SkinElements.ManiaStageBottom,           null! },
            { SkinElements.ManiaStageLight,            null! },
            { SkinElements.ManiaStageJudgementLine,    null! },
            { SkinElements.ManiaHitLightning,          null! },
            { SkinElements.ManiaHitLightningHold,      null! },
                                                       
            // taiko
            { SkinElements.TaikoHit0,                  null! },
            { SkinElements.TaikoHit100,                null! },
            { SkinElements.TaikoHit300,                null! },
            { SkinElements.TaikoHitCircleBigDon,       null! },
            { SkinElements.TaikoHitCircleBigKat,       null! },
            { SkinElements.TaikoHitCircleOverlayBig,   null! },
            { SkinElements.TaikoHitCircleDon,          null! },
            { SkinElements.TaikoHitCircleKat,          null! },
            { SkinElements.TaikoHitCircleOverlay,      null! },
            { SkinElements.TaikoGlow,                  null! },
            { SkinElements.TaikoButtonsUI,             null! },
            { SkinElements.TaikoInnerButton,           null! },
            { SkinElements.TaikoOuterButton,           null! },
            { SkinElements.TaikoPlayBar,               null! },
            { SkinElements.TaikoPlayBarGlow,           null! },
            { SkinElements.TaikoRollMiddle,            null! },
            { SkinElements.TaikoRollEnd,               null! },
            { SkinElements.TaikoSpinnerWarning,        null! },
            { SkinElements.TaikoSliderBackground,      null! }, // idk if i want that
            { SkinElements.TaikoSliderFailBackground,  null! }, // idk if i want that
            { SkinElements.TaikoBarLine,               null! }, // idk if i want that         
                                                       
            // catch
            { SkinElements.CatchFruitCatcherIdle,      null! },
            { SkinElements.CatchFruitCatcherFail,      null! }, // idk if i want that but high chance i will do it
            { SkinElements.CatchFruitCatcherKiai,      null! }, // idk if i want that but high chance i will do it
            { SkinElements.CatchFruitPear,             null! },
            { SkinElements.CatchFruitPearOverlay,      null! },
            { SkinElements.CatchFruitGrapes,           null! },
            { SkinElements.CatchFruitGrapesOverlay,    null! },
            { SkinElements.CatchFruitApple,            null! },
            { SkinElements.CatchFruitAppleOverlay,     null! },
            { SkinElements.CatchFruitOrange,           null! },
            { SkinElements.CatchFruitOrangeOverlay,    null! },
            { SkinElements.CatchFruitBananas,          null! },
            { SkinElements.CatchFruitBananasOverlay,   null! },
            { SkinElements.CatchFruitDrop,             null! },
            { SkinElements.CatchFruitDropOverlay,      null! },
        };

        // https://osu.ppy.sh/wiki/en/Skinning/osu%21
        // https://osu.ppy.sh/wiki/en/Skinning/Interface
        public static void UpdateSkinPath(string path)
        {
            if (Directory.Exists(path))
            {
                CurrentSkinFolderPath = path;
            }
            else
            {
                CurrentSkinFolderPath = DefaultSkinFolderPath;
            }

            SkinIniProperties.ResetComboColours();

            // clear every single skin element
            foreach (SkinElements element in SkinElementsDictionary.Keys)
            {
                SkinElementsDictionary[element] = null!;
            }
            CursorSkin.Apply();

            // reset combo colours saved property
            IsCircleColourSaved = false;
        }
        
        public static string SkinPath()
        {
            return CurrentSkinFolderPath;
        }

        public static BitmapSource GetElement(SkinElements skinElement, string index = "0")
        {
            if (SkinElementsDictionary[skinElement] == null)
            {
                try
                {
                    if (MainWindow.replay.GameMode == OsuFileParsers.Classes.Replay.GameMode.OsuTaiko
                    && (skinElement == SkinElements.TaikoHitCircleBigDon || skinElement == SkinElements.TaikoHitCircleDon
                    ||  skinElement == SkinElements.TaikoHitCircleBigKat || skinElement == SkinElements.TaikoHitCircleKat))
                    {
                        return GetColouredTaikoCircle(skinElement);
                    }
                    else
                    {
                        SkinElementsDictionary[skinElement] = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }     
                }
                catch
                {   // for skin elements that are optional like mania LongNoteTail, which if not skinned then it defaults to LongNoteHead
                    // disgusting recursion it makes me want to vomit ewww i hate it... but i dont know how else to do it without it being pain in the ass
                    switch (skinElement)
                    {
                        case SkinElements.ManiaLongNoteTail1:
                            return GetElement(SkinElements.ManiaLongNoteHead1);
                        case SkinElements.ManiaLongNoteTail2:
                            return GetElement(SkinElements.ManiaLongNoteHead2);
                        case SkinElements.ManiaLongNoteTail3:
                            return GetElement(SkinElements.ManiaLongNoteHead3);
                        default:
                            MessageBox.Show($"Current skin \"{CurrentSkinFolderPath.Split("\\").Last()}\" doesnt contain skin element for {skinElement} (name may differ from osu wiki skin naming)");
                            return null!;
                    }
                }
            }

            // special case coz coloured hit circles are made in a bit different way... change it soon tho
            if (skinElement == SkinElements.HitCircle)
            {
                return GetColouredHitCircle(int.Parse(index));
            }

            return SkinElementsDictionary[skinElement];
        }

        // 99.99% i wont implement it but just in case i feel like doing it
        // spinner-rpm.png | spinner-clear.png | spinner-spin.png | spinner-metre.png and some more https://osu.ppy.sh/wiki/en/Skinning/osu%21
        public static string GetElementPath(SkinElements skinElement)
        {
            // i dont give a fuck what is anyone opinion for this i like it this way
            switch (skinElement)
            {
                case SkinElements.Cursor:
                    return SkinElementPath("cursor");
                case SkinElements.ApproachCircle:
                    return SkinElementPath("approachcircle");
                case SkinElements.HitCircle:
                    return SkinElementPath("hitcircle");
                case SkinElements.HitCircleOverlay:
                    return SkinElementPath("hitcircleoverlay");
                case SkinElements.ComboNumber0:
                    return SkinElementPath("default-0");
                case SkinElements.ComboNumber1:
                    return SkinElementPath("default-1");
                case SkinElements.ComboNumber2:
                    return SkinElementPath("default-2");
                case SkinElements.ComboNumber3:
                    return SkinElementPath("default-3");
                case SkinElements.ComboNumber4:
                    return SkinElementPath("default-4");
                case SkinElements.ComboNumber5:
                    return SkinElementPath("default-5");
                case SkinElements.ComboNumber6:
                    return SkinElementPath("default-6");
                case SkinElements.ComboNumber7:
                    return SkinElementPath("default-7");
                case SkinElements.ComboNumber8:
                    return SkinElementPath("default-8");
                case SkinElements.ComboNumber9:
                    return SkinElementPath("default-9");
                case SkinElements.ReverseArrow:
                    return SkinElementPath("reversearrow");
                case SkinElements.SliderBall:
                    return SkinElementPath("sliderb0");
                case SkinElements.SliderBallCircle:
                    return SkinElementPath("sliderfollowcircle");
                case SkinElements.Hit300:
                    return AnimatableSkinElementPath("hit300");
                case SkinElements.Hit100:
                    return AnimatableSkinElementPath("hit100");
                case SkinElements.Hit50:
                    return AnimatableSkinElementPath("hit50");
                case SkinElements.Hit0:
                    return AnimatableSkinElementPath("hit0");
                case SkinElements.SliderEndMiss:
                    return SkinElementPath("sliderendmiss");
                case SkinElements.SliderTickMiss:
                    return SkinElementPath("slidertickmiss");
                case SkinElements.SliderTick:
                    return SkinElementPath("sliderscorepoint");
                case SkinElements.SpinnerApproachCircle:
                    return SkinElementPath("spinner-approachcircle");
                case SkinElements.SpinnerBackground:
                    return SkinElementPath("spinner-background");
                case SkinElements.SpinnerCircle:
                    return SkinElementPath("spinner-circle");
                case SkinElements.ManiaHit0:
                    return AnimatableSkinElementPath("mania-hit0");
                case SkinElements.ManiaHit50:
                    return AnimatableSkinElementPath("mania-hit50");
                case SkinElements.ManiaHit100:
                    return AnimatableSkinElementPath("mania-hit100");
                case SkinElements.ManiaHit200:
                    return AnimatableSkinElementPath("mania-hit200");
                case SkinElements.ManiaHit300:
                    return AnimatableSkinElementPath("mania-hit300");
                case SkinElements.ManiaHit320:
                    return AnimatableSkinElementPath("mania-hit300g");
                case SkinElements.ManiaKey1Idle:
                    return SkinElementPath("mania-key1");
                case SkinElements.ManiaKey2Idle:
                    return SkinElementPath("mania-key2");
                case SkinElements.ManiaKey3Idle:
                    return SkinElementPath("mania-keyS");
                case SkinElements.ManiaKey1Pressed:
                    return SkinElementPath("mania-key1D");
                case SkinElements.ManiaKey2Pressed:
                    return SkinElementPath("mania-key2D");
                case SkinElements.ManiaKey3Pressed:
                    return SkinElementPath("mania-keySD");
                case SkinElements.ManiaNote1:
                    return AnimatableSkinElementPath("mania-note1");
                case SkinElements.ManiaNote2:
                    return AnimatableSkinElementPath("mania-note2");
                case SkinElements.ManiaNote3:
                    return AnimatableSkinElementPath("mania-noteS");
                case SkinElements.ManiaLongNoteHead1:
                    return AnimatableSkinElementPath("mania-note1H");
                case SkinElements.ManiaLongNoteHead2:
                    return AnimatableSkinElementPath("mania-note2H");
                case SkinElements.ManiaLongNoteHead3:
                    return AnimatableSkinElementPath("mania-noteSH");
                case SkinElements.ManiaLongNoteBody1:
                    return AnimatableSkinElementPath("mania-note1L");
                case SkinElements.ManiaLongNoteBody2:
                    return AnimatableSkinElementPath("mania-note2L");
                case SkinElements.ManiaLongNoteBody3:
                    return AnimatableSkinElementPath("mania-noteSL");
                case SkinElements.ManiaLongNoteTail1:
                    return AnimatableSkinElementPath("mania-note1T");
                case SkinElements.ManiaLongNoteTail2:
                    return AnimatableSkinElementPath("mania-note2T");
                case SkinElements.ManiaLongNoteTail3:
                    return AnimatableSkinElementPath("mania-noteST");
                case SkinElements.ManiaStageLeft:
                    return SkinElementPath("mania-stage-left");
                case SkinElements.ManiaStageRight:
                    return SkinElementPath("mania-stage-right");
                case SkinElements.ManiaStageBottom:
                    return AnimatableSkinElementPath("mania-stage-bottom");
                case SkinElements.ManiaStageLight:
                    return AnimatableSkinElementPath("mania-stage-light");
                case SkinElements.ManiaStageJudgementLine:
                    return SkinElementPath("mania-stage-hint");
                case SkinElements.ManiaHitLightning:
                    return AnimatableSkinElementPath("lightingL");
                case SkinElements.ManiaHitLightningHold:
                    return AnimatableSkinElementPath("lightingN");
                case SkinElements.TaikoHit0:
                    return AnimatableSkinElementPath("taiko-hit0");
                case SkinElements.TaikoHit100:
                    return AnimatableSkinElementPath("taiko-hit100");
                case SkinElements.TaikoHit300:
                    return AnimatableSkinElementPath("taiko-hit300");
                case SkinElements.TaikoHitCircleBigDon:
                case SkinElements.TaikoHitCircleBigKat:
                    return SkinElementPath("taikobigcircle");
                case SkinElements.TaikoHitCircleOverlayBig:
                    return AnimatableSkinElementPath("taikobigcircleoverlay");
                case SkinElements.TaikoHitCircleDon:
                case SkinElements.TaikoHitCircleKat:
                    return SkinElementPath("taikohitcircle");
                case SkinElements.TaikoHitCircleOverlay:
                    return AnimatableSkinElementPath("taikohitcircleoverlay");
                case SkinElements.TaikoGlow:
                    return SkinElementPath("taiko-glow");
                case SkinElements.TaikoSliderBackground:
                    return SkinElementPath("taiko-slider"); // might not do
                case SkinElements.TaikoSliderFailBackground:
                    return SkinElementPath("taiko-slider-fail"); // might not do
                case SkinElements.TaikoButtonsUI:
                    return SkinElementPath("taiko-bar-left");
                case SkinElements.TaikoInnerButton:
                    return SkinElementPath("taiko-drum-inner");
                case SkinElements.TaikoOuterButton:
                    return SkinElementPath("taiko-drum-outer");
                case SkinElements.TaikoPlayBar:
                    return SkinElementPath("taiko-bar-right");
                case SkinElements.TaikoPlayBarGlow:
                    return SkinElementPath("taiko-bar-right-glow");
                case SkinElements.TaikoBarLine:
                    return SkinElementPath("taiko-barline");// might not do
                case SkinElements.TaikoRollMiddle:
                    return SkinElementPath("taiko-roll-middle");
                case SkinElements.TaikoRollEnd:
                    return SkinElementPath("taiko-roll-end");
                case SkinElements.TaikoSpinnerWarning:
                    return SkinElementPath("spinner-warning");
                case SkinElements.CatchFruitCatcherIdle:
                    return AnimatableSkinElementPath("fruit-catcher-idle");
                case SkinElements.CatchFruitCatcherFail:
                    return AnimatableSkinElementPath("fruit-catcher-fail");
                case SkinElements.CatchFruitCatcherKiai:
                    return AnimatableSkinElementPath("fruit-catcher-kiai");
                case SkinElements.CatchFruitPear:
                    return SkinElementPath("fruit-pear");
                case SkinElements.CatchFruitPearOverlay:
                    return SkinElementPath("fruit-pear-overlay");
                case SkinElements.CatchFruitGrapes:
                    return SkinElementPath("fruit-grapes");
                case SkinElements.CatchFruitGrapesOverlay:
                    return SkinElementPath("fruit-grapes-overlay");
                case SkinElements.CatchFruitApple:
                    return SkinElementPath("fruit-apple");
                case SkinElements.CatchFruitAppleOverlay:
                    return SkinElementPath("fruit-apple-overlay");
                case SkinElements.CatchFruitOrange:
                    return SkinElementPath("fruit-orange");
                case SkinElements.CatchFruitOrangeOverlay:
                    return SkinElementPath("fruit-orange-overlay");
                case SkinElements.CatchFruitBananas:
                    return SkinElementPath("fruit-bananas");
                case SkinElements.CatchFruitBananasOverlay:
                    return SkinElementPath("fruit-bananas-overlay");
                case SkinElements.CatchFruitDrop:
                    return SkinElementPath("fruit-drop");
                case SkinElements.CatchFruitDropOverlay:
                    return SkinElementPath("fruit-drop-overlay");
                case SkinElements.Lighting:
                    return SkinElementPath("lighting"); // might not do
                default:
                    throw new Exception("Skin element does not exist");
            }
        }

        public static void ApplyNotelockColourEffect(HitObject hitObject)
        {
            if (hitObject is HitCircle hc)
            {
                HitCircle.Circle(hc).Source = HitCirclesColoured.Last();
            }
            else if (hitObject is Slider s)
            {
                Slider.HeadHitCircle(s).Source = HitCirclesColoured.Last();
            }
        }

        public static void GetHitObjectsRGBValues()
        {
            List<Color> colours = SkinIniProperties.GetComboColours();
            if (colours.Count == 0)
            {
                return;
            }

            int index = 0;
            foreach (HitObjectData hitObjectData in MainWindow.map.HitObjects!)
            {
                if (hitObjectData.ComboNumber == 1)
                {
                    index++;
                    if (index >= colours.Count - 1)
                    {
                        index = 0;
                    }
                }

                hitObjectData.RGBValue = colours[index];
            }
        }

        unsafe private static BitmapSource GetColouredTaikoCircle(SkinElements skinElement)
        {
            if (SkinElementsDictionary[skinElement] != null)
            {
                return SkinElementsDictionary[skinElement];
            }

            BitmapSource circle = new BitmapImage(new Uri(GetElementPath(skinElement)));
            WriteableBitmap colouredCircle = new WriteableBitmap(circle);

            IntPtr pBackBuffer = colouredCircle.BackBuffer;
            byte* pBuff = (byte*)pBackBuffer.ToPointer();

            int backBufferStride = colouredCircle.BackBufferStride;

            int pixelX;
            int pixelY;
            int pixelIndex;

            byte a;
            byte b;
            byte g;
            byte r;

            for (int x = 0; x < colouredCircle.PixelHeight; x++)
            {
                for (int y = 0; y < colouredCircle.PixelWidth; y++)
                {
                    pixelX = 4 * x;
                    pixelY = y * backBufferStride;
                    pixelIndex = pixelX + pixelY;

                    a = pBuff[pixelIndex + 3];
                    if (a == 0)
                    {
                        continue;
                    }

                    b = pBuff[pixelIndex];
                    g = pBuff[pixelIndex + 1];
                    r = pBuff[pixelIndex + 2];

                    // Tinted red for "Don"(235, 69, 44) as circle skin element wiki says
                    if (skinElement == SkinElements.TaikoHitCircleDon || skinElement == SkinElements.TaikoHitCircleBigDon)
                    {
                        pBuff[pixelIndex + 0] = (byte)(b - (b - 44));
                        pBuff[pixelIndex + 1] = (byte)(g - (g - 69));
                        pBuff[pixelIndex + 2] = (byte)(r - (r - 235));
                    }
                    else // Tinted blue for "Katsu"(68, 141, 171) as circle skin element wiki says
                    {
                        pBuff[pixelIndex + 0] = (byte)(b - (b - 171));
                        pBuff[pixelIndex + 1] = (byte)(g - (g - 141));
                        pBuff[pixelIndex + 2] = (byte)(r - (r - 68));
                    }
                }
            }

            SkinElementsDictionary[skinElement] = colouredCircle;
            return SkinElementsDictionary[skinElement];
        }

        // old colouring functions https://github.com/ravinyan/osuReplayAnalyzer/blob/9d73d6f2580b8e5402dab6e3ae35e8090d997c7a/ReplayAnalyzer/HitObjects/HitObject.cs
        private static WriteableBitmap GetColouredHitCircle(int comboColourIndex)
        {
            // fun fact: base implementation i took from internet took 3.5ms on average, mine takes 15-20 ticks... smh noobs

            // skip white colour coz if skin element is already coloured (like in -Nekoha Shizuku -(Suminoze) skin)
            // then it will just become white or just be some ugly abomination... there might be more cases like this
            // but i cant care enough to download 500 skins and check if there are more cases like that...
            List<Color> colours = SkinIniProperties.GetComboColours();
            if (colours.Count == 0 || colours[comboColourIndex] == Color.FromArgb(255, 255, 255))
            {
                if (IsCircleColourSaved == false)
                {
                    IsCircleColourSaved = true;
                    HitCirclesColoured = new WriteableBitmap[2]; // 1 is base circle, 2 is notelock effect colour

                    HitCirclesColoured[0] = new WriteableBitmap(SkinElementsDictionary[SkinElements.HitCircle]);
                    RecolourHitCircle(1, colours);
                }

                return HitCirclesColoured[0];
            }

            if (IsCircleColourSaved == true)
            {
                return HitCirclesColoured[comboColourIndex];
            }

            IsCircleColourSaved = true;
            // + 1 here coz we reserving last index as notelock colour effect
            HitCirclesColoured = new WriteableBitmap[SkinIniProperties.GetComboColours().Count + 1];
            for (int i = 0; i < colours.Count + 1; i++)
            {
                RecolourHitCircle(i, colours);
            }

            return HitCirclesColoured[comboColourIndex];
        }

        unsafe private static void RecolourHitCircle(int colourIndex, List<Color> colours)
        {
            HitCirclesColoured[colourIndex] = new WriteableBitmap(SkinElementsDictionary[SkinElements.HitCircle]);

            // i guess this is memory buffer of the WHOLE image
            IntPtr pBackBuffer = HitCirclesColoured[colourIndex].BackBuffer;
            // pointers are basically arrays so it creates array of all colour data in packs of 4 (BGRA format)
            byte* pBuff = (byte*)pBackBuffer.ToPointer();

            int backBufferStride = HitCirclesColoured[colourIndex].BackBufferStride;

            int pixelX;
            int pixelY;
            int pixelIndex;

            byte a;
            byte b;
            byte g;
            byte r;

            for (int x = 0; x < HitCirclesColoured[colourIndex].PixelHeight; x++)
            {
                for (int y = 0; y < HitCirclesColoured[colourIndex].PixelWidth; y++)
                {
                    pixelX = 4 * x;                 // 4 * x (y * buff) is the boundle of BGRA on this specific X/Y pixel
                    pixelY = y * backBufferStride;  // back buffer stride is memory size of single column of the image
                    pixelIndex = pixelX + pixelY;

                    a = pBuff[pixelIndex + 3];      // < to colours we add up to +3 to get B(0) G(1) R(2) A(3) values
                    if (a == 0)
                    {
                        continue;                   // we skip invisible pixels to speed up the recolouring
                    }

                    // get current colours of hit object
                    b = pBuff[pixelIndex];
                    g = pBuff[pixelIndex + 1];
                    r = pBuff[pixelIndex + 2];

                    // apply new colours to hit object based on skin colours and based on how strong the white colour is (thanks google AI for once you werent useless)
                    // based on this formula: White - (White - Colour) (all pixels are white before they are changed)
                    // i found multiple different formulas for this but they use multiplication and division which is slow
                    // also after i found my formula i couldnt find anything close to that so... it would suck if i refreshed my browser lol
                    // another formula just in case: (byte)(colour.R + (255 - colour.R) * (b / 255)) i think it does same thing just slower

                    // THIS COLOURING IS LIGHTER THAN WHAT OSU HAS
                    // i like it this way but if needed just uncomment * 0.85 and change it to darken the colours (lower = darker)
                    if (colourIndex < colours.Count)
                    {
                        pBuff[pixelIndex + 0] = (byte)((b - (b - colours[colourIndex].B))); //* 0.85);
                        pBuff[pixelIndex + 1] = (byte)((g - (g - colours[colourIndex].G))); //* 0.85);
                        pBuff[pixelIndex + 2] = (byte)((r - (r - colours[colourIndex].R))); //* 0.85);
                    }
                    else if (colourIndex == colours.Count)
                    {// notelock colour
                        pBuff[pixelIndex + 0] = 0;
                        pBuff[pixelIndex + 1] = 0;
                        pBuff[pixelIndex + 2] = 255;
                    }
                }
            }
        }

        private static string SkinElementPath(string skinElement)
        {
            // base example hitcircle
            // priority hitcircle@2x > no hd
            string fullPath = $"{SkinPath()}\\{skinElement}";
            if (File.Exists($"{fullPath}@2x.png"))
            {
                return $"{fullPath}@2x.png";
            }
            else if (File.Exists($"{fullPath}.png"))
            {
                return $"{fullPath}.png";
            }
            else
            {
                if (File.Exists($"{DefaultSkinFolderPath}\\{skinElement}@2x.png"))
                {
                    return $"{DefaultSkinFolderPath}\\{skinElement}@2x.png";
                }
                else
                {
                    return $"{DefaultSkinFolderPath}\\{skinElement}.png";
                }        
            }
        }

        // special function for judgements coz it can have animated skin elements (but no animations)
        private static string AnimatableSkinElementPath(string skinElement)
        {
            // base example  hit300
            // priority -0@2x > the non hd > hit300@2x > non
            string fullPath = $"{SkinPath()}\\{skinElement}";
            if (File.Exists($"{fullPath}-0@2x.png"))
            {
                return $"{fullPath}-0@2x.png";
            }
            else if (File.Exists($"{fullPath}-0.png"))
            {
                return $"{fullPath}-0.png";
            }
            else if (File.Exists($"{fullPath}@2x.png"))
            {
                return $"{fullPath}@2x.png";
            }
            else if (File.Exists($"{fullPath}.png"))
            {
                return $"{fullPath}.png";
            }
            else
            {
                return $"{DefaultSkinFolderPath}\\{skinElement}-0@2x.png";
            }
        }

        // that is a chonker enum
        public enum SkinElements
        {
            // misc used by multiple gamemodes
            ApproachCircle,         // osu! osu!taiko
            SliderTick,             // osu! osu!taiko
            SpinnerCircle,          // osu! osu!taiko
            SpinnerApproachCircle,  // osu! osu!taiko
            Lighting,               // osu! osu!taiko osu!catch

            // std
            Cursor,
            HitCircle,
            HitCircleOverlay,
            ComboNumber0,
            ComboNumber1,
            ComboNumber2,
            ComboNumber3,
            ComboNumber4,
            ComboNumber5,
            ComboNumber6,
            ComboNumber7,
            ComboNumber8,
            ComboNumber9,
            ReverseArrow,
            SliderBall,
            SliderBallCircle,
            Hit300,
            Hit100,
            Hit50 ,
            Hit0 ,
            SliderEndMiss,
            SliderTickMiss,
            SpinnerBackground,

            // mania
            ManiaHit0,
            ManiaHit50,
            ManiaHit100,
            ManiaHit200,
            ManiaHit300,
            ManiaHit320,
            ManiaKey1Idle,
            ManiaKey1Pressed,
            ManiaKey2Idle,
            ManiaKey2Pressed,
            ManiaKey3Idle,
            ManiaKey3Pressed,
            ManiaNote1,
            ManiaNote2,
            ManiaNote3,
            ManiaLongNoteHead1,
            ManiaLongNoteHead2,
            ManiaLongNoteHead3,
            ManiaLongNoteBody1,
            ManiaLongNoteBody2,
            ManiaLongNoteBody3,
            ManiaLongNoteTail1,
            ManiaLongNoteTail2,
            ManiaLongNoteTail3,
            ManiaStageLeft,
            ManiaStageRight,
            ManiaStageBottom,
            ManiaStageLight,
            ManiaStageJudgementLine,
            ManiaHitLightning,
            ManiaHitLightningHold,

            // taiko
            TaikoHit0,
            TaikoHit100,
            TaikoHit300,
            TaikoHitCircleBigDon,
            TaikoHitCircleBigKat,
            TaikoHitCircleOverlayBig,
            TaikoHitCircleDon,
            TaikoHitCircleKat,
            TaikoHitCircleOverlay,
            //TaikoApproachCircle,
            TaikoGlow,
            TaikoButtonsUI,
            TaikoInnerButton,
            TaikoOuterButton,
            TaikoPlayBar,
            TaikoPlayBarGlow,
            TaikoRollMiddle,
            TaikoRollEnd,
            //TaikoSliderScorePoint,
            TaikoSpinnerWarning,
            //TaikoSpinner,
            //TaikoSpinnerApproachCircle,
            //TaikoLighting,              // unknown if i do his
            TaikoSliderBackground,      // unknown if i do his
            TaikoSliderFailBackground,  // unknown if i do his
            TaikoBarLine,               // unknown if i do his

            // catch
            CatchFruitCatcherIdle,
            CatchFruitCatcherFail,
            CatchFruitCatcherKiai,
            CatchFruitPear,
            CatchFruitPearOverlay,
            CatchFruitGrapes,
            CatchFruitGrapesOverlay,
            CatchFruitApple,
            CatchFruitAppleOverlay,
            CatchFruitOrange,
            CatchFruitOrangeOverlay,
            CatchFruitBananas,
            CatchFruitBananasOverlay,
            CatchFruitDrop,
            CatchFruitDropOverlay,
            //CatchLighting,            // unknown if i do his
        }
    }
}

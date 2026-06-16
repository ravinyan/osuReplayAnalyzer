using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayAnalyzer.HitObjects;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace ReplayAnalyzer.GameplaySkin
{
    public static class SkinElement
    {
        private static string CurrentSkinFolderPath = "";
        private static string DefaultSkinFolderPath = $"{AppContext.BaseDirectory}\\Skins\\Komori - PeguLian II (PwV)";

        // well... i dont see better way to do this
        // this is special thing for hit circle, rest is just cached BitmapSource
        private static bool IsSaved = false;
        private static BitmapSource HitCircleSource = null!;
        private static WriteableBitmap[] HitCirclesColoured = Array.Empty<WriteableBitmap>();

        // to be memory efficient it would be better to put EVERYTHING here in an array (4 arrays for each gamemode)
        // but it would also make everything pain in the ass
        // i guess dictionary of SkinElements key and BitmapSource value would be good? lets see i guess
        // dictionary or touple in list both will work and list might be better but that wont be a problem to change
        // nvm this takes <3k bytes no need for a list lmao

        private static BitmapSource ApproachCircle = null!; // osu! osu!taiko
        private static BitmapSource SliderTick = null!; // osu! osu!taiko
        private static BitmapSource SpinnerApproachCircle = null!; // osu! osu!taiko
        private static BitmapSource SpinnerCircle = null!; // osu! osu!taiko
        private static BitmapSource Lighting = null!; // osu! osu!taiko osu!catch

        private static Dictionary<SkinElements, BitmapSource> SkinElementsBitmaps = new Dictionary<SkinElements, BitmapSource>()
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
            { SkinElements.ManiaHoldNoteHead1,         null! },
            { SkinElements.ManiaHoldNoteHead2,         null! },
            { SkinElements.ManiaHoldNoteHead3,         null! },
            { SkinElements.ManiaHoldNoteBody1,         null! },
            { SkinElements.ManiaHoldNoteBody2,         null! },
            { SkinElements.ManiaHoldNoteBody3,         null! },
            { SkinElements.ManiaHoldNoteTail1,         null! },
            { SkinElements.ManiaHoldNoteTail2,         null! },
            { SkinElements.ManiaHoldNoteTail3,         null! },
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
            { SkinElements.TaikoHitCircleBig,          null! },
            { SkinElements.TaikoHitCircleOverlayBig,   null! },
            { SkinElements.TaikoHitCircle,             null! },
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

        // osu!
        private static BitmapSource   Cursor                = null!;
        private static BitmapSource[] ComboNumbers          = Array.Empty<BitmapSource>(); // from 0 to 9
        private static BitmapSource   HitCircleOverlay      = null!;
        private static BitmapSource   ReverseArrow          = null!;
        private static BitmapSource   SliderBall            = null!;
        private static BitmapSource   SliderBallCircle      = null!;
        private static BitmapSource   SpinnerBackground     = null!;
        private static BitmapSource   Hit300                = null!;
        private static BitmapSource   Hit100                = null!;
        private static BitmapSource   Hit50                 = null!;
        private static BitmapSource   Hit0                  = null!;
        private static BitmapSource   SliderEndMiss         = null!;
        private static BitmapSource   SliderTickMiss        = null!;
                                                            
        // osu!mania 1=white 2=pink 3=yellow key colours    
        private static BitmapSource ManiaHit0               = null!;
        private static BitmapSource ManiaHit50              = null!;
        private static BitmapSource ManiaHit100             = null!;
        private static BitmapSource ManiaHit200             = null!;
        private static BitmapSource ManiaHit300             = null!;
        private static BitmapSource ManiaHit320             = null!;
        private static BitmapSource ManiaKey1Idle           = null!; // white
        private static BitmapSource ManiaKey1Pressed        = null!;
        private static BitmapSource ManiaKey2Idle           = null!; // pink
        private static BitmapSource ManiaKey2Pressed        = null!;
        private static BitmapSource ManiaKey3Idle           = null!; // yellow
        private static BitmapSource ManiaKey3Pressed        = null!;
        private static BitmapSource ManiaNote1              = null!; // white they have animations? why? im not doing that lmao
        private static BitmapSource ManiaNote2              = null!; // pink  also same format as animatable judgements
        private static BitmapSource ManiaNote3              = null!; // yellow
        private static BitmapSource ManiaHoldNoteHead1      = null!;
        private static BitmapSource ManiaHoldNoteHead2      = null!;
        private static BitmapSource ManiaHoldNoteHead3      = null!;
        private static BitmapSource ManiaHoldNoteBody1      = null!;
        private static BitmapSource ManiaHoldNoteBody2      = null!;
        private static BitmapSource ManiaHoldNoteBody3      = null!;
        private static BitmapSource ManiaHoldNoteTail1      = null!; // these are optional, default is Head skin element
        private static BitmapSource ManiaHoldNoteTail2      = null!; // these are optional, default is Head skin element
        private static BitmapSource ManiaHoldNoteTail3      = null!; // these are optional, default is Head skin element
        private static BitmapSource ManiaStageLeft          = null!;
        private static BitmapSource ManiaStageRight         = null!;
        private static BitmapSource ManiaStageBottom        = null!;
        private static BitmapSource ManiaStageLight         = null!;
        private static BitmapSource ManiaStageJudgementLine = null!;
        private static BitmapSource ManiaHitLightning       = null!;
        private static BitmapSource ManiaHitLightningHold   = null!;

        /* Below is the default note image layout for each column, by key count.
        
        Keycount	Col 1	Col 2	Col 3	Col 4	Col 5	Col 6	Col 7	Col 8	Col 9
        1K	S								
        2K	1	1							
        3K	1	S	1						
        4K	1	2	2	1					
        5K	1	2	S	2	1				
        6K	1	2	1	1	2	1			
        7K	1	2	1	S	1	2	1		
        8K	1	2	1	2	2	1	2	1	
        9K	1	2	1	2	S	2	1	2	1
        */

        // osu!taiko (im not doing pippidon animations or even just pippidon)
        private static BitmapSource TaikoHit0                  = null!;
        private static BitmapSource TaikoHit100                = null!;
        private static BitmapSource TaikoHit300                = null!;
        private static BitmapSource TaikoHitCircleBig          = null!;
        private static BitmapSource TaikoHitCircleOverlayBig   = null!;
        private static BitmapSource TaikoHitCircle             = null!;
        private static BitmapSource TaikoHitCircleOverlay      = null!;
        private static BitmapSource TaikoGlow                  = null!;
        private static BitmapSource TaikoButtonsUI             = null!;    
        private static BitmapSource TaikoInnerButton           = null!; // here positions are varying by skin version idk what that means yet
        private static BitmapSource TaikoOuterButton           = null!; // here positions are varying by skin version idk what that means yet
        private static BitmapSource TaikoPlayBar               = null!;
        private static BitmapSource TaikoPlayBarGlow           = null!;
        private static BitmapSource TaikoRollMiddle            = null!; // idk wat dat is
        private static BitmapSource TaikoRollEnd               = null!; // idk wat dat is
        private static BitmapSource TaikoSpinnerWarning        = null!;
        // not sure if i want this                           
        private static BitmapSource TaikoSliderBackground      = null!;
        private static BitmapSource TaikoSliderFailBackground  = null!;
        private static BitmapSource TaikoBarLine               = null!;

        // osu!catch
        private static BitmapSource CatchFruitCatcherIdle    = null!;
        private static BitmapSource CatchFruitCatcherFail    = null!;
        private static BitmapSource CatchFruitCatcherKiai    = null!;
        private static BitmapSource CatchFruitPear           = null!;
        private static BitmapSource CatchFruitPearOverlay    = null!;
        private static BitmapSource CatchFruitGrapes         = null!;
        private static BitmapSource CatchFruitGrapesOverlay  = null!;
        private static BitmapSource CatchFruitApple          = null!;
        private static BitmapSource CatchFruitAppleOverlay   = null!;
        private static BitmapSource CatchFruitOrange         = null!;
        private static BitmapSource CatchFruitOrangeOverlay  = null!;
        private static BitmapSource CatchFruitBananas        = null!;
        private static BitmapSource CatchFruitBananasOverlay = null!;
        private static BitmapSource CatchFruitDrop           = null!;
        private static BitmapSource CatchFruitDropOverlay    = null!;

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

            Cursor = null!; // reset cached cursor before applying new skin
            CursorSkin.Apply();

            // clear everything
            foreach (SkinElements element in SkinElementsBitmaps.Keys)
            {
                SkinElementsBitmaps[element] = null!;
            }
            // reset everything to save new skin elements
            IsSaved               = false;
            //HitCircleSource       = null!;
            //HitCirclesColoured    = new WriteableBitmap[SkinIniProperties.GetComboColours().Count + 1];
            //ComboNumbers          = Array.Empty<BitmapImage>();
            //HitCircleOverlay      = null!;
            //ApproachCircle        = null!;
            //ReverseArrow          = null!;
            //SliderBall            = null!;
            //SliderBallCircle      = null!;
            //SliderTick            = null!;
            //SpinnerApproachCircle = null!;
            //SpinnerBackground     = null!;
            //SpinnerCircle         = null!;
            //Hit300                = null!;
            //Hit100                = null!;
            //Hit50                 = null!;
            //Hit0                  = null!;
            //SliderEndMiss         = null!;
            //SliderTickMiss        = null!;
        }
        
        public static string SkinPath()
        {
            return CurrentSkinFolderPath;
        }

        public static BitmapSource GetElement(SkinElements skinElement, string index = "0")
        {
            // ? idk if this will work
            if (SkinElementsBitmaps[skinElement] == null)
            {
                SkinElementsBitmaps[skinElement] = new BitmapImage(new Uri(GetElementPath(skinElement, index))); 
            }

            // special case coz coloured hit circles are made in a bit different way... change it soon tho
            if (skinElement == SkinElements.HitCircle)
            {
                return GetColouredHitCircle(int.Parse(index));
            }

            return SkinElementsBitmaps[skinElement];
        }

        // 99.99% i wont implement it but just in case i feel like doing it
        // spinner-rpm.png | spinner-clear.png | spinner-spin.png | spinner-metre.png and some more https://osu.ppy.sh/wiki/en/Skinning/osu%21
        /// <summary>
        /// 
        /// </summary>
        /// <param name="skinElement"></param>
        /// <param name="index">this is used to put index for ComboNumber index and HitCircle colour index</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static BitmapSource WorseGetElement(SkinElements skinElement, string index = "0")
        {
            // i dont give a fuck what is anyone opinion for this i like it this way
            switch (skinElement)
            {
                case SkinElements.Cursor:
                    if (Cursor == null)
                    {
                        Cursor = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return Cursor;
                case SkinElements.ApproachCircle:
                    if (ApproachCircle == null)
                    {
                        ApproachCircle = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ApproachCircle;
                case SkinElements.HitCircle:
                    if (HitCircleSource == null)
                    {
                        HitCircleSource = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return GetColouredHitCircle(int.Parse(index));
                case SkinElements.HitCircleOverlay:
                    if (HitCircleOverlay == null)
                    {
                        HitCircleOverlay = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return HitCircleOverlay;
                case SkinElements.ComboNumber0:
                    if (ComboNumbers.Length == 0)
                    {
                        ComboNumbers = new BitmapSource[10];
                        for (int i = 0; i < 10; i++) // combo numbers are from 0 to 9
                        {
                            ComboNumbers![i] = new BitmapImage(new Uri(GetElementPath(skinElement, $"{i}")));
                        }
                    }
                    return ComboNumbers![int.Parse(index)];
                case SkinElements.ReverseArrow:
                    if (ReverseArrow == null)
                    {
                        ReverseArrow = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ReverseArrow;
                case SkinElements.SliderBall:
                    if (SliderBall == null)
                    {
                        SliderBall = new BitmapImage(new Uri(GetElementPath(skinElement))); // this one can be animated but will leave it like this unless its a problem
                    }
                    return SliderBall;
                case SkinElements.SliderBallCircle:
                    if (SliderBallCircle == null)
                    {
                        SliderBallCircle = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return SliderBallCircle;
                case SkinElements.Hit300:
                    if (Hit300 == null)
                    {
                        Hit300 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return Hit300;
                case SkinElements.Hit100:
                    if (Hit100 == null)
                    {
                        Hit100 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return Hit100;
                case SkinElements.Hit50:
                    if (Hit50 == null)
                    {
                        Hit50 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return Hit50;
                case SkinElements.Hit0:
                    if (Hit0 == null)
                    {
                        Hit0 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return Hit0;
                case SkinElements.SliderEndMiss:
                    if (SliderEndMiss == null)
                    {
                        SliderEndMiss = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return SliderEndMiss;
                case SkinElements.SliderTickMiss:
                    if (SliderTickMiss == null)
                    {
                        SliderTickMiss = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return SliderTickMiss;
                case SkinElements.SliderTick:
                    if (SliderTick == null)
                    {
                        SliderTick = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return SliderTick;
                case SkinElements.SpinnerApproachCircle:
                    if (SpinnerApproachCircle == null)
                    {
                        SpinnerApproachCircle = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return SpinnerApproachCircle;
                case SkinElements.SpinnerBackground:
                    if (SpinnerBackground == null)
                    {
                        SpinnerBackground = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return SpinnerBackground;
                case SkinElements.SpinnerCircle:
                    if (SpinnerCircle == null)
                    {
                        SpinnerCircle = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return SpinnerCircle;
                case SkinElements.ManiaHit0:
                    if (ManiaHit0 == null)
                    {
                        ManiaHit0 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaHit0;
                case SkinElements.ManiaHit50:
                    if (ManiaHit50 == null)
                    {
                        ManiaHit50 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaHit50;
                case SkinElements.ManiaHit100:
                    if (ManiaHit100 == null)
                    {
                        ManiaHit100 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaHit100;
                case SkinElements.ManiaHit200:
                    if (ManiaHit200 == null)
                    {
                        ManiaHit200 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaHit200;
                case SkinElements.ManiaHit300:
                    if (ManiaHit300 == null)
                    {
                        ManiaHit300 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaHit300;
                case SkinElements.ManiaHit320:
                    if (ManiaHit320 == null)
                    {
                        ManiaHit320 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaHit320;
                case SkinElements.ManiaKey1Idle:
                    if (ManiaKey1Idle == null)
                    {
                        ManiaKey1Idle = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaKey1Idle;
                case SkinElements.ManiaKey2Idle:
                    if (ManiaKey2Idle == null)
                    {
                        ManiaKey2Idle = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaKey2Idle;
                case SkinElements.ManiaKey3Idle:
                    if (ManiaKey3Idle == null)
                    {
                        ManiaKey3Idle = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaKey3Idle;
                case SkinElements.ManiaKey1Pressed:
                    if (ManiaKey1Pressed == null)
                    {
                        ManiaKey1Pressed = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaKey1Pressed;
                case SkinElements.ManiaKey2Pressed:
                    if (ManiaKey2Pressed == null)
                    {
                        ManiaKey2Pressed = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaKey2Pressed;
                case SkinElements.ManiaKey3Pressed:
                    if (ManiaKey3Pressed == null)
                    {
                        ManiaKey3Pressed = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaKey3Pressed;
                case SkinElements.ManiaNote1:
                    if (ManiaNote1 == null)
                    {
                        ManiaNote1 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaNote1;
                case SkinElements.ManiaNote2:
                    if (ManiaNote2 == null)
                    {
                        ManiaNote2 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaNote2;
                case SkinElements.ManiaNote3:
                    if (ManiaNote3 == null)
                    {
                        ManiaNote3 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaNote3;
                case SkinElements.ManiaHoldNoteHead1:
                    if (ManiaHoldNoteHead1 == null)
                    {
                        ManiaHoldNoteHead1 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaHoldNoteHead1;
                case SkinElements.ManiaHoldNoteHead2:
                    if (ManiaHoldNoteHead2 == null)
                    {
                        ManiaHoldNoteHead2 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaHoldNoteHead2;
                case SkinElements.ManiaHoldNoteHead3:
                    if (ManiaHoldNoteHead3 == null)
                    {
                        ManiaHoldNoteHead3 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaHoldNoteHead3;
                case SkinElements.ManiaHoldNoteBody1:
                    if (ManiaHoldNoteBody1 == null)
                    {
                        ManiaHoldNoteBody1 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaHoldNoteBody1;
                case SkinElements.ManiaHoldNoteBody2:
                    if (ManiaHoldNoteBody2 == null)
                    {
                        ManiaHoldNoteBody2 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaHoldNoteBody2;
                case SkinElements.ManiaHoldNoteBody3:
                    if (ManiaHoldNoteBody3 == null)
                    {
                        ManiaHoldNoteBody3 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaHoldNoteBody3;
                case SkinElements.ManiaHoldNoteTail1:
                    if (ManiaHoldNoteTail1 == null)
                    {
                        ManiaHoldNoteTail1 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaHoldNoteTail1;
                case SkinElements.ManiaHoldNoteTail2:
                    if (ManiaHoldNoteTail2 == null)
                    {
                        ManiaHoldNoteTail2 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaHoldNoteTail2;
                case SkinElements.ManiaHoldNoteTail3:
                    if (ManiaHoldNoteTail3 == null)
                    {
                        ManiaHoldNoteTail3 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaHoldNoteTail3;
                case SkinElements.ManiaStageLeft:
                    if (ManiaStageLeft == null)
                    {
                        ManiaStageLeft = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaStageLeft;
                case SkinElements.ManiaStageRight:
                    if (ManiaStageRight == null)
                    {
                        ManiaStageRight = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaStageRight;
                case SkinElements.ManiaStageBottom:
                    if (ManiaStageBottom == null)
                    {
                        ManiaStageBottom = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaStageBottom;
                case SkinElements.ManiaStageLight:
                    if (ManiaStageLight == null)
                    {
                        ManiaStageLight = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaStageLight;
                case SkinElements.ManiaStageJudgementLine:
                    if (ManiaStageJudgementLine == null)
                    {
                        ManiaStageJudgementLine = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaStageJudgementLine;
                case SkinElements.ManiaHitLightning:
                    if (ManiaHitLightning == null)
                    {
                        ManiaHitLightning = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaHitLightning;
                case SkinElements.ManiaHitLightningHold:
                    if (ManiaHitLightningHold == null)
                    {
                        ManiaHitLightningHold = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return ManiaHitLightningHold;
                case SkinElements.TaikoHit0:
                    if (TaikoHit0 == null)
                    {
                        TaikoHit0 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoHit0;
                case SkinElements.TaikoHit100:
                    if (TaikoHit100 == null)
                    {
                        TaikoHit100 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoHit100;
                case SkinElements.TaikoHit300:
                    if (TaikoHit300 == null)
                    {
                        TaikoHit300 = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoHit300;
                case SkinElements.TaikoHitCircleBig:
                    if (TaikoHitCircleBig == null)
                    {
                        TaikoHitCircleBig = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoHitCircleBig;
                case SkinElements.TaikoHitCircleOverlayBig:
                    if (TaikoHitCircleOverlayBig == null)
                    {
                        TaikoHitCircleOverlayBig = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoHitCircleOverlayBig;
                case SkinElements.TaikoHitCircle:
                    if (TaikoHitCircle == null)
                    {
                        TaikoHitCircle = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoHitCircle;
                case SkinElements.TaikoHitCircleOverlay:
                    if (TaikoHitCircleOverlay == null)
                    {
                        TaikoHitCircleOverlay = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoHitCircleOverlay;
                case SkinElements.TaikoGlow:
                    if (TaikoGlow == null)
                    {
                        TaikoGlow = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoGlow;
                case SkinElements.TaikoSliderBackground: // might not do
                    if (TaikoSliderBackground == null)
                    {
                        TaikoSliderBackground = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoSliderBackground;
                case SkinElements.TaikoSliderFailBackground: // might not do
                    if (TaikoSliderFailBackground == null)
                    {
                        TaikoSliderFailBackground = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoSliderFailBackground;
                case SkinElements.TaikoButtonsUI:
                    if (TaikoButtonsUI == null)
                    {
                        TaikoButtonsUI = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoButtonsUI;
                case SkinElements.TaikoInnerButton:
                    if (TaikoInnerButton == null)
                    {
                        TaikoInnerButton = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoInnerButton;
                case SkinElements.TaikoOuterButton:
                    if (TaikoOuterButton == null)
                    {
                        TaikoOuterButton = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoOuterButton;
                case SkinElements.TaikoPlayBar:
                    if (TaikoPlayBar == null)
                    {
                        TaikoPlayBar = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoPlayBar;
                case SkinElements.TaikoPlayBarGlow:
                    if (TaikoPlayBarGlow == null)
                    {
                        TaikoPlayBarGlow = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoPlayBarGlow;
                case SkinElements.TaikoBarLine: // might not do
                    if (TaikoBarLine == null)
                    {
                        TaikoBarLine = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoBarLine;
                case SkinElements.TaikoRollMiddle:
                    if (TaikoRollMiddle == null)
                    {
                        TaikoRollMiddle = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoRollMiddle;
                case SkinElements.TaikoRollEnd:
                    if (TaikoRollEnd == null)
                    {
                        TaikoRollEnd = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoRollEnd;
                case SkinElements.TaikoSpinnerWarning:
                    if (TaikoSpinnerWarning == null)
                    {
                        TaikoSpinnerWarning = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return TaikoSpinnerWarning;
                case SkinElements.CatchFruitCatcherIdle:
                    if (CatchFruitCatcherIdle == null)
                    {
                        CatchFruitCatcherIdle = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return CatchFruitCatcherIdle;
                case SkinElements.CatchFruitCatcherFail:
                    if (CatchFruitCatcherFail == null)
                    {
                        CatchFruitCatcherFail = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return CatchFruitCatcherFail;
                case SkinElements.CatchFruitCatcherKiai:
                    if (CatchFruitCatcherKiai == null)
                    {
                        CatchFruitCatcherKiai = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return CatchFruitCatcherKiai;
                case SkinElements.CatchFruitPear:
                    if (CatchFruitPear == null)
                    {
                        CatchFruitPear = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return CatchFruitPear;
                case SkinElements.CatchFruitPearOverlay:
                    if (CatchFruitPearOverlay == null)
                    {
                        CatchFruitPearOverlay = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return CatchFruitPearOverlay;
                case SkinElements.CatchFruitGrapes:
                    if (CatchFruitGrapes == null)
                    {
                        CatchFruitGrapes = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return CatchFruitGrapes;
                case SkinElements.CatchFruitGrapesOverlay:
                    if (CatchFruitGrapesOverlay == null)
                    {
                        CatchFruitGrapesOverlay = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return CatchFruitGrapesOverlay;
                case SkinElements.CatchFruitApple:
                    if (CatchFruitApple == null)
                    {
                        CatchFruitApple = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return CatchFruitApple;
                case SkinElements.CatchFruitAppleOverlay:
                    if (CatchFruitAppleOverlay == null)
                    {
                        CatchFruitAppleOverlay = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return CatchFruitAppleOverlay;
                case SkinElements.CatchFruitOrange:
                    if (CatchFruitOrange == null)
                    {
                        CatchFruitOrange = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return CatchFruitOrange;
                case SkinElements.CatchFruitOrangeOverlay:
                    if (CatchFruitOrangeOverlay == null)
                    {
                        CatchFruitOrangeOverlay = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return CatchFruitOrangeOverlay;
                case SkinElements.CatchFruitBananas:
                    if (CatchFruitBananas == null)
                    {
                        CatchFruitBananas = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return CatchFruitBananas;
                case SkinElements.CatchFruitBananasOverlay:
                    if (CatchFruitBananasOverlay == null)
                    {
                        CatchFruitBananasOverlay = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return CatchFruitBananasOverlay;
                case SkinElements.CatchFruitDrop:
                    if (CatchFruitDrop == null)
                    {
                        CatchFruitDrop = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return CatchFruitDrop;
                case SkinElements.CatchFruitDropOverlay:
                    if (CatchFruitDropOverlay == null)
                    {
                        CatchFruitDropOverlay = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return CatchFruitDropOverlay;
                case SkinElements.Lighting: // might not do
                    if (Lighting == null)
                    {
                        Lighting = new BitmapImage(new Uri(GetElementPath(skinElement)));
                    }
                    return Lighting;
                default:
                    throw new Exception("Skin element does not exist");
            }
        }

        public static string GetElementPath(SkinElements skinElement, string index = "0")
        {
            // i dont give a fuck what is anyone opinion for this i like it this way
            // some elements like TaikoApporachCircle are the same as ApproachCircle so they will be grouped together 
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
                case SkinElements.ManiaHoldNoteHead1:
                    return AnimatableSkinElementPath("mania-note1H");
                case SkinElements.ManiaHoldNoteHead2:
                    return AnimatableSkinElementPath("mania-note2H");
                case SkinElements.ManiaHoldNoteHead3:
                    return AnimatableSkinElementPath("mania-noteSH");
                case SkinElements.ManiaHoldNoteBody1:
                    return AnimatableSkinElementPath("mania-note1L");
                case SkinElements.ManiaHoldNoteBody2:
                    return AnimatableSkinElementPath("mania-note2L");
                case SkinElements.ManiaHoldNoteBody3:
                    return AnimatableSkinElementPath("mania-noteSL");
                case SkinElements.ManiaHoldNoteTail1:
                    return AnimatableSkinElementPath("mania-note1T");
                case SkinElements.ManiaHoldNoteTail2:
                    return AnimatableSkinElementPath("mania-note2T");
                case SkinElements.ManiaHoldNoteTail3:
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
                case SkinElements.TaikoHitCircleBig:
                    return SkinElementPath("taikobigcircle");
                case SkinElements.TaikoHitCircleOverlayBig:
                    return AnimatableSkinElementPath("taikobigcircleoverlay");
                case SkinElements.TaikoHitCircle:
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
                if (IsSaved == false)
                {
                    IsSaved = true;
                    HitCirclesColoured = new WriteableBitmap[2]; // 1 is base circle, 2 is notelock effect colour

                    HitCirclesColoured[0] = new WriteableBitmap(HitCircleSource);
                    RecolourHitCircle(1, colours);
                }

                return HitCirclesColoured[0];
            }

            if (IsSaved == true)
            {
                return HitCirclesColoured[comboColourIndex];
            }

            IsSaved = true;
            // + 1 here coz we reserving last index as notelock colour effect
            HitCirclesColoured = new WriteableBitmap[SkinIniProperties.GetComboColours().Count + 1];
            for (int i = 0; i < colours.Count + 1; i++)
            {
                RecolourHitCircle(i, colours);
            }

            return HitCirclesColoured[comboColourIndex];
        }

        unsafe private static void RecolourHitCircle(int i, List<Color> colours)
        {
            HitCirclesColoured[i] = new WriteableBitmap(SkinElementsBitmaps[SkinElements.HitCircle]);

            // i guess this is memory buffer of the WHOLE image
            IntPtr pBackBuffer = HitCirclesColoured[i].BackBuffer;
            // pointers are basically arrays so it creates array of all colour data in packs of 4 (BGRA format)
            byte* pBuff = (byte*)pBackBuffer.ToPointer();

            int backBufferStride = HitCirclesColoured[i].BackBufferStride;

            int pixelX;
            int pixelY;
            int pixelIndex;

            byte a;
            byte b;
            byte g;
            byte r;

            for (int x = 0; x < HitCirclesColoured[i].PixelHeight; x++)
            {
                for (int y = 0; y < HitCirclesColoured[i].PixelWidth; y++)
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
                    if (i < colours.Count)
                    {
                        pBuff[pixelIndex + 0] = (byte)((b - (b - colours[i].B))); //* 0.85);
                        pBuff[pixelIndex + 1] = (byte)((g - (g - colours[i].G))); //* 0.85);
                        pBuff[pixelIndex + 2] = (byte)((r - (r - colours[i].R))); //* 0.85);
                    }
                    else if (i == colours.Count)
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
            ManiaHoldNoteHead1,
            ManiaHoldNoteHead2,
            ManiaHoldNoteHead3,
            ManiaHoldNoteBody1,
            ManiaHoldNoteBody2,
            ManiaHoldNoteBody3,
            ManiaHoldNoteTail1,
            ManiaHoldNoteTail2,
            ManiaHoldNoteTail3,
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
            TaikoHitCircleBig,
            TaikoHitCircleOverlayBig,
            TaikoHitCircle,
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

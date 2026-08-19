## osu! Replay Analyzer

Application for analyzing replays for all game modes in rhythm game [osu!](https://osu.ppy.sh/)

Made by [me](https://osu.ppy.sh/users/11774957) with help from [osu!lazer](https://github.com/ppy/osu) code for some specific math calculations (thank u peppy).

<img width="1357" height="784" alt="ReplayAnalyzer_alkQdiB9wp" src="https://github.com/user-attachments/assets/892c3ae7-c6b9-4f91-8a30-e83821c8f7f2" />

[Replay](https://osu.ppy.sh/scores/1140922988) by [sriracha rice](https://osu.ppy.sh/users/11922986/osu) on [this map](https://osu.ppy.sh/beatmapsets/1336619#osu/2769000)

Skins used in replay analyzer:
- [Komori - PeguLian II (PwV) - osu!std game mode](https://www.reddit.com/r/OsuSkins/comments/eizg4d/osunewyear_skin_pegulian_iipwvhappy_new/)
- [Ralsei dark 1.2 (Corne2Plum3) - All game modes](https://osu.ppy.sh/community/forums/topics/1629393?n=1)


## Features

- Compatibility with osu! and osu!lazer
- Watching replay frame by frame
- Jumping to any point in replay
- Shows Hit Markers on click, Frame Markers and Cursor Path
- Toggleable Hidden mod (even if replay doesn't have Hidden coz why not)
- Getting replay simply by pressing F2 on replay screen in game on any kind of map
- Rate change slider from 0.25x to 2x speed
- UR bar where late hits are on the left and early on the right
- Jumping to closest misses to the Left or Right of current time in replay
- Toggleable Key Overlay that shows length of clicks
- Judgement Timeline for x100, x50 and misses with toggles to enable/disable them separately
- osu!lazer mod compatibility with mods and their settings that change gameplay in any way, like Difficulty Adjust (excluding Random, Target Practice and Fun mods)
- Skin can be changed using Skins folder in Replay Analyzer or by using some other external folder that contains skin folders
- Toggleable Hit Map that shows precise position of clicks on or near circles
- In osu!catch Catcher shows precise position on the platter where object was caught/missed
- In osu!mania judgements are shown separately for each column
- In osu!mania and osu!taiko Scroll Velocity can be adjusted in settings
- And probably more SoonTM...

## Download

Go to the latest release [here](https://github.com/ravinyan/osuReplayAnalyzer/releases) and download win-x64 zip folder, sadly it's windows only (blame WPF). 

Folder can be moved freely anywhere but don't delete/move or modify files inside it since it can lead to application not working.

## Random fun fact(s)

This application was supposed to be a simple parser for osu! replay/beatmap files, but i was bored and curious so it evolved into this replay analyzer... oops?

Tried to make this application pretty lightweight for current (horrible) standards, it might not be but i tried a little bit at least. Tested on stream map that is currently playing on default (not lowest) app settings:
- RAM - <75MB on 5min long replay
- CPU - 3-4% on i5-13450HX
- GPU - average of ~5% on RTX 4060

![ShawzinBot Logo](https://github.com/ianespana/ShawzinBot/blob/master/ShawzinBot/Resources/Shawzin.png)

ShawzinBot is a program which converts a MIDI input or file to a series of key presses for the Shawzin. Any MIDI input works with this script (a MIDI keyboard, a virtual MIDI channel, etc), as well as [Standard MIDI Files (SMF)](https://www.midi.org/specifications/category/smf-specifications).

# Virus Scan
You can check the virus scan for the precompiled version [here](https://www.virustotal.com/gui/file/3cb941d7918c21f79d749aba8434b8ef55a8eadc024277c94502e3f89f4e40df/detection).

# Download
You can always get the latest version of ShawzinBot [here](https://github.com/ianespana/ShawzinBot/releases/latest).

# About

### What's the ShawzinBot?

ShawzinBot is a third party software that can read MIDI files (.mid) and play their content in Warframe. It simulates key presses, and that's how the Shawzin is played.

### Is it safe?

ShawzinBot does not in any way interfere with gameplay, nor does it inject new code or modify existing one. It also does not tamper with memory in any way, thus is can't be catalogued as a cheat and should be safe to use.

### How does it work?

ShawzinBot is built on the C# [DryWetMIDI midi library by melanchall](https://github.com/melanchall/drywetmidi). This library loads a MIDI file and plays it. Every note event is intercepted by ShawzinBot and depending on note's ID, a different combination of keys is pressed to play the sound. ShawzinBot also allows the usage of external MIDI devices (keyboard, synthesizers, etc).

# Getting Started
Using ShawzinBot is quite easy. As long as you don't separate any of it's components, it can sit anywhere in your computer. No installation is required, all you need to do is double click ShawzinBot.exe and you'll be good to go! Please note, **you MUST match the the game's scale with the one displayed by the bot!**

## Usage
Once the program is running you'll see the following window:

![ShawzinBot Overview](https://github.com/ianespana/ShawzinBot/blob/master/ShawzinBot/Resources/Overview.png)

On the top left corner there's a button you can click to open and load MIDI files. Once a file is open, all you have to do is click play and tab back into game. ShawzinBot will not mess with your keyboard unless you're tabbed into Warframe!

To use an external MIDI device, all you have to do is select if from the dropdown menu and start playing. If you do not see your device, just click the reload button to the right of the dropdown.

The program defaults to playing the first MIDI track found inside the file. If you want to play a different track or tracks, all you have to do is open the "MIDI Tracks" dropdown and select the tracks there. Simple.

There are 3 settings you can change at this moment:
* Whammy - This will enable the whammy. Default off.
* Transpose notes - This setting will attempt to transpose notes that are unplayable. Default on.
* Play MIDI through speakers - This will play the MIDI file through your main sound device. Useful for testing songs. Default off.

# Special Thanks
* [@lilggamegenius](https://github.com/lilggamegenius) - For helping with figuring out the key presses in game.
* [/u/T2k5](https://www.reddit.com/user/T2k5/) - For making the original full keyboard script.

# Notes
* Multiple keys at the same time don't really work correctly due to Shawzin limitations, you can offset notes by a tiny bit to fix this issue.

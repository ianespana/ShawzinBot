;
; ShawzinBot.ahk
; Convert a MIDI input to a multiple keypresses for the Shawzin
;
; Made by Heracles421
; https://github.com/ianespana/ShawzinBot
;

#include MidiLib.ahk
#include ShawzinLib.ahk

midi := new Midi()
midi.OpenMidiIn( 0 )

shawzin := new Shawzin()
Return

MidiNoteOn:
	midiEvent := midi.MidiIn()
	noteNumber := midiEvent.noteNumber
	noteVelocity := midiEvent.velocity
	
	if (noteNumber != 0)
	{
		shawzin.PlayNote( noteNumber, noteVelocity )
	}
	Return
	
Numpad0::
	Reload
	Return
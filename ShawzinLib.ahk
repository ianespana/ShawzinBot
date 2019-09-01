;
; ShawzinLib.ahk
; Convert a MIDI note to a Shawzin keypress
;
; Made by Heracles421
; https://github.com/ianespana/ShawzinBot
;
; Original full keyboard script made by T2k5
;

shawzinNotes := {48:[0,0,1,0], 49:[0,0,2,0], 50:[0,0,3,0], 51:[0,1,1,0], 52:[0,1,2,0], 53:[0,1,3,0], 54:[0,2,1,0], 55:[0,2,2,0], 56:[0,2,3,0], 57:[0,3,1,0], 58:[0,3,2,0], 59:[0,3,3,0], 60:[7,1,3,0], 61:[4,2,1,0], 62:[7,2,1,0], 63:[1,2,2,0], 64:[7,2,2,0], 65:[1,2,3,0], 66:[1,3,1,0], 67:[7,2,3,0], 68:[7,3,1,1], 69:[7,3,1,0], 70:[1,3,3,0], 71:[4,3,2,1], 72:[4,3,2,0], 73:[4,3,3,0], 74:[7,3,3,0], 75:[6,3,3,0]}
shawzinFrets := {1:"n", 2:"l", 3:"m"}
scaleSize := 8
activeScale := 0
vibratoActive := false

; Shawzin class interface
Class Shawzin
{
	PlayNote( noteNumber, noteVelocity )
	{
		if ( noteVelocity != 0 )
		{
			__SendNote( noteNumber, noteVelocity )
		}
	}
}

__SetScale( scaleIndex )
{
	global activeScale
	global scaleSize
	
	scaleDifference := 0
	if( scaleIndex < activeScale )
	{
		scaleDifference := scaleSize - ( activeScale - scaleIndex )
	}
	else if ( scaleIndex > activeScale )
	{
		scaleDifference := scaleIndex - activeScale
	}
	Loop, %scaleDifference% 
	{
		SendInput {TAB}
	}
	activeScale := scaleIndex
}

__SendNote( noteNumber, noteVelocity ){
	global shawzinNotes
	global shawzinFrets
	global vibratoActive
	
	noteVelocity := Floor(( noteVelocity + 1 ) / 32)
	
	activeNote := shawzinNotes[noteNumber]
	
	if ( activeNote != "" )
	{
		if ( vibratoActive )
		{
			SendInput, {Space up}
			vibratoActive := false
		}
		
		__SetScale( activeNote[1] )
		fretPress := shawzinFrets[activeNote[2]]
		
		if( activeNote[2] > 0 ){	
			SendInput, {%fretPress% down}
		}
		if( activeNote[4] ){
			SendInput, {Space down}
			vibratoActive := true
		}
		
		Loop, %noteVelocity%{
			SendInput, % activeNote[3]
		}
		
		if( activeNote[2] > 0 )
		{
			SendInput, {%fretPress% up}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.Win32;
using ShawzinBot.Models;

namespace ShawzinBot.ViewModels
{
    public class MainViewModel : Screen
    {
        #region Private Variables

        private string _songName;
        private double _songSlider;
        private double _maximumTime = 1;
        private string _currentTime = "0:00";
        private string _totalTime = "0:00";
        private string _playPauseIcon = "Play";

        private BindableCollection<MidiInputModel> _midiInputs = new BindableCollection<MidiInputModel>();
        private MidiInputModel _selectedMidiInput;

        private bool _enableVibrato = true;
        private bool _transposeNotes = true;
        private bool _playThroughSpeakers;

        #endregion

        #region Public Variables

        public MidiFile midiFile;
        public Playback playback;

        #endregion

        #region Constructor

        public MainViewModel()
        {
            MidiInputs.Add(new MidiInputModel("None"));
            SelectedMidiInput = MidiInputs[0];
        }

        #endregion

        #region Properties

        public string SongName
        {
            get => _songName;
            set
            {
                _songName = value;
                NotifyOfPropertyChange(() => SongName);
            }
        }

        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                NotifyOfPropertyChange(() => CurrentTime);
            }
        }

        public string TotalTime
        {
            get => _totalTime;
            set
            {
                _totalTime = value;
                NotifyOfPropertyChange(() => TotalTime);
            }
        }

        public double SongSlider
        {
            get => _songSlider;
            set
            {
                _songSlider = value;
                NotifyOfPropertyChange(() => SongSlider);
            }
        }

        public double MaximumTime
        {
            get => _maximumTime;
            set
            {
                _maximumTime = value;
                NotifyOfPropertyChange(() => MaximumTime);
            }
        }

        public string PlayPauseIcon
        {
            get => _playPauseIcon;
            set
            {
                _playPauseIcon = value;
                NotifyOfPropertyChange(() => PlayPauseIcon);
            }
        }

        public BindableCollection<MidiInputModel> MidiInputs
        {
            get => _midiInputs;
            set
            {
                _midiInputs = value;
                NotifyOfPropertyChange(() => MidiInputs);
            }
        }

        public MidiInputModel SelectedMidiInput
        {
            get => _selectedMidiInput;
            set
            {
                _selectedMidiInput = value;
                NotifyOfPropertyChange(() => SelectedMidiInput);
            }
        }

        public bool EnableVibrato
        {
            get => _enableVibrato;
            set
            {
                _enableVibrato = value;
                NotifyOfPropertyChange(() => EnableVibrato);
            }
        }

        public bool TransposeNotes
        {
            get => _transposeNotes;
            set
            {
                _transposeNotes = value;
                NotifyOfPropertyChange(() => TransposeNotes);
            }
        }

        public bool PlayThroughSpeakers
        {
            get => _playThroughSpeakers;
            set
            {
                _playThroughSpeakers = value;
                NotifyOfPropertyChange(() => PlayThroughSpeakers);
            }
        }

        #endregion

        #region Methods

        public void OpenFile()
        {
            var openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "MIDI file|*.mid;*.midi"; // Filter to only midi files
            if (openFileDialog.ShowDialog() != true) return;

            CloseFile();
            midiFile = MidiFile.Read(openFileDialog.FileName);
            SongName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);

            var tempoMap = midiFile.GetTempoMap();
            TimeSpan midiFileDuration = midiFile.GetTimedEvents()
                                            .LastOrDefault(e => e.Event is NoteOffEvent)
                                            ?.TimeAs<MetricTimeSpan>(tempoMap) ?? new MetricTimeSpan();

            TotalTime = midiFileDuration.ToString("m\\:ss");
            MaximumTime = midiFileDuration.TotalSeconds;
            SongSlider = 0;
            CurrentTime = "0:00";
            playback = midiFile.GetPlayback(OutputDevice.GetById(0));
            playback.Finished += (s, e) =>
            {
                CloseFile();
            };
			playback.NoteCallback += (data, time, length, playbackTime)=>{ 	// Call function for every note.
				ActionManager.SendNote(data.NoteNumber);					// Probably won't sound good unless monophonic
				return data;
			};

            playback.ClockTick += OnTick;

            playback.EventPlayed += OnNoteEvent;
        }

        public void CloseFile()
        {
            if (playback != null)
            {
                playback.Stop();
                playback.OutputDevice.Dispose();
                playback.Dispose();
            }

            playback = null;
            midiFile = null;

            PlayPauseIcon = "Play";
            SongName = "";
            TotalTime = "0:00";
            CurrentTime = "0:00";
            MaximumTime = 1;
        }

        public void PlayPause()
        {
            if (midiFile == null || MaximumTime == 0d) return;
            if (playback.IsRunning)
            {
                PlayPauseIcon = "Play";
                playback.Stop();
            }
            else
            {
                PlayPauseIcon = "Pause";
                var device = OutputDevice.GetById(0);
                if (!PlayThroughSpeakers && playback.OutputDevice != null)
                {
                    playback.OutputDevice.Dispose();
                    playback.OutputDevice = null;
                }
                else if (PlayThroughSpeakers && (playback.OutputDevice == null || playback.OutputDevice.ProductIdentifier != device.ProductIdentifier))
                {
                    playback.OutputDevice = OutputDevice.GetById(0);
                }
				ActionManager.warframeWindow = ActionManager.FindWindow("WarframePublicEvolutionGfxD3D11", null); 	//Attempt to focus window, not working yet.
				ActionManager.BringWindowToFront(ActionManager.warframeWindow ); 														//Still requires the user to click first
                playback.Start();
            }
        }

        #endregion

        #region EventHandlers

        public void OnTick(object sender, ClockTickArgs e)
        {
            SongSlider = e.Time.TotalSeconds;
            CurrentTime = e.Time.ToString("m\\:ss");
        }

        public void OnNoteEvent(object sender, MidiEventPlayedEventArgs e)
        {
            if (e.Event.EventType == MidiEventType.NoteOn)
            {
                //e.Note.Octave
            }
        }

        #endregion
    }
}

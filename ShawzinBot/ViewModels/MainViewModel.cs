using System;
using System.IO;
using System.Linq;
using System.Timers;
using Caliburn.Micro;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.Win32;
using ShawzinBot.Models;
using InputDevice = Melanchall.DryWetMidi.Devices.InputDevice;

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
        private string _scale = "Scale: Chromatic";
        
        private BindableCollection<MidiInputModel> _midiInputs = new BindableCollection<MidiInputModel>();
        private BindableCollection<MidiTrackModel> _midiTracks = new BindableCollection<MidiTrackModel>();
        private MidiInputModel _selectedMidiInput;

        private bool _enableVibrato = true;
        private bool _transposeNotes = true;
        private bool _playThroughSpeakers;
        private bool _ignoreSliderChange;

        private string[] ScaleArray = {
            "Chromatic",
            "Hexatonic",
            "Major",
            "Minor",
            "Hirajoshi",
            "Phrygian",
            "Yo",
            "Pentatonic Minor",
            "Pentatonic Major"
        };

        private TrackChunk metaTrack;
        private TrackChunk firstTrack;

        private Timer playTimer;

        #endregion

        #region Public Variables

        public MidiFile midiFile;
        public TempoMap tempoMap;
        public Playback playback;
        public InputDevice inputDevice;

        public static bool reloadPlayback;

        #endregion

        #region Constructor

        public MainViewModel()
        {
            MidiInputs.Add(new MidiInputModel("None"));

            foreach (var device in InputDevice.GetAll())
            {
                MidiInputs.Add(new MidiInputModel(device.Name));
            }

            SelectedMidiInput = MidiInputs[0];

            EnableVibrato = Properties.Settings.Default.EnableVibrato;
            TransposeNotes = Properties.Settings.Default.TransposeNotes;
            PlayThroughSpeakers = Properties.Settings.Default.PlayThroughSpeakers;
            PlaybackCurrentTimeWatcher.Instance.PollingInterval = TimeSpan.FromSeconds(1);
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
                if (!_ignoreSliderChange && playback != null)
                {
                    if (playback.IsRunning)
                    {
                        playback.Stop();
                        PlayPauseIcon = "Play";
                    }
                    TimeSpan time = TimeSpan.FromSeconds(_songSlider);

                    CurrentTime = time.ToString("m\\:ss");
                    playback.MoveToTime((MetricTimeSpan) time);
                }
                _ignoreSliderChange = false;
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

        public BindableCollection<MidiTrackModel> MidiTracks
        {
            get => _midiTracks;
            set
            {
                _midiTracks = value;
                NotifyOfPropertyChange(() => MidiTracks);
            }
        }

        public MidiInputModel SelectedMidiInput
        {
            get => _selectedMidiInput;
            set
            {
                _selectedMidiInput = value;
                inputDevice?.Dispose();

                if (value?.DeviceName != null && value.DeviceName != "None")
                {
                    inputDevice = InputDevice.GetByName(value.DeviceName);
                    inputDevice.EventReceived += OnNoteEvent;
                    inputDevice.StartEventsListening();
                    ActionManager.OnSongPlay();
                }

                NotifyOfPropertyChange(() => SelectedMidiInput);
            }
        }

        public bool EnableVibrato
        {
            get => _enableVibrato;
            set
            {
                _enableVibrato = value;
                Properties.Settings.Default.EnableVibrato = value;
                Properties.Settings.Default.Save();
                NotifyOfPropertyChange(() => EnableVibrato);
            }
        }

        public bool TransposeNotes
        {
            get => _transposeNotes;
            set
            {
                _transposeNotes = value;
                Properties.Settings.Default.TransposeNotes = value;
                Properties.Settings.Default.Save();
                NotifyOfPropertyChange(() => TransposeNotes);
            }
        }

        public bool PlayThroughSpeakers
        {
            get => _playThroughSpeakers;
            set
            {
                _playThroughSpeakers = value;
                Properties.Settings.Default.PlayThroughSpeakers = value;
                Properties.Settings.Default.Save();
                NotifyOfPropertyChange(() => PlayThroughSpeakers);

                if (playback != null)
                {
                    if (!_playThroughSpeakers && playback.OutputDevice != null)
                    {
                        playback.OutputDevice.Dispose();
                        playback.OutputDevice = null;
                    }
                    else if (_playThroughSpeakers && (playback.OutputDevice == null))
                    {
                        var list = OutputDevice.GetAll();
                        if (list.Count() >= 1)
                        {
                            playback.OutputDevice = list.FirstOrDefault();
                        }
                    }
                }
            }
        }

        public string Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                NotifyOfPropertyChange(() => Scale);
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
            MidiTracks.Clear();

            midiFile = MidiFile.Read(openFileDialog.FileName);
            SongName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);

            tempoMap = midiFile.GetTempoMap();

            TimeSpan midiFileDuration = midiFile.GetDuration<MetricTimeSpan>();
            TotalTime = midiFileDuration.ToString("m\\:ss");
            MaximumTime = midiFileDuration.TotalSeconds;
            UpdateSlider(0);
            CurrentTime = "0:00";
            metaTrack = midiFile.GetTrackChunks().FirstOrDefault();
            midiFile.Chunks.Remove(metaTrack);
            firstTrack = midiFile.GetTrackChunks().FirstOrDefault();
            midiFile.Chunks.Remove(firstTrack);
            MidiTracks.Add(new MidiTrackModel(firstTrack, true));

            foreach (TrackChunk track in midiFile.GetTrackChunks())
            {
                MidiTracks.Add(new MidiTrackModel(track));
            }
        }

        public void CloseFile()
        {
            if (playback != null)
            {
                playback.Stop();
                playback.OutputDevice?.Dispose();

                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback);

                playback?.Dispose();
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
            if (playback == null || reloadPlayback)
            {
                ITimeSpan playTime = new MidiTimeSpan();
                if (playback != null)
                {
                    playback.Stop();
                    playTime = playback.GetCurrentTime(TimeSpanType.Midi);
                    playback.OutputDevice?.Dispose();
                    playback = null;
                    PlayPauseIcon = "Play";
                }

                midiFile.Chunks.Clear();
                midiFile.Chunks.Add(metaTrack);

                foreach (MidiTrackModel trackModel in MidiTracks)
                {
                    if (trackModel.IsChecked)
                    {
                        midiFile.Chunks.Add(trackModel.Track);
                    }
                }

                playback = midiFile.GetPlayback();
                playback.MoveToTime(playTime);
                playback.Finished += (s, e) =>
                {
                    CloseFile();
                };

                PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback, TimeSpanType.Metric);
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += OnTick;
                PlaybackCurrentTimeWatcher.Instance.Start();

                playback.EventPlayed += OnNoteEvent;
                reloadPlayback = false;
            }

            if (playback.IsRunning)
            {
                PlayPauseIcon = "Play";
                playback.Stop();
            }
            else
            {
                PlayPauseIcon = "Pause";                

                ActionManager.OnSongPlay();
                playTimer = new Timer();
                playTimer.Interval = 100;
                playTimer.Elapsed += new ElapsedEventHandler(PlayTimerElapsed);
                playTimer.AutoReset = false;
                playTimer.Start();
            }
            UpdateScale(ActionManager.activeScale);
        }

        private void PlayTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (ActionManager.IsWindowFocused("Warframe") || PlayThroughSpeakers)
            {
                playback.Start();
                playTimer.Stop();
            }
            else
            {
                playTimer.Stop();
                playTimer.Start();
            }
        }

        public void Previous()
        {
            if (playback != null)
            {
                playback.MoveToStart();
                UpdateSlider(0);
                CurrentTime = "0:00";
            }
        }

        public void Next()
        {
            CloseFile();
        }

        public void RefreshDevices()
        {
            MidiInputs.Clear();
            MidiInputs.Add(new MidiInputModel("None"));

            foreach (var device in InputDevice.GetAll())
            {
                MidiInputs.Add(new MidiInputModel(device.Name));
            }

            SelectedMidiInput = MidiInputs[0];
        }

        public void UpdateScale(int scaleIndex) 
        {
            Scale = "Scale: " + ScaleArray[scaleIndex];
        }

        #endregion

        #region EventHandlers

        public void OnTick(object sender, PlaybackCurrentTimeChangedEventArgs e)
        {
            foreach (var playbackTime in e.Times)
            {
                TimeSpan time = (MetricTimeSpan) playbackTime.Time;

                UpdateSlider(time.TotalSeconds);
                CurrentTime = time.ToString("m\\:ss");
            }
        }

        public void OnNoteEvent(object sender, MidiEventPlayedEventArgs e)
        {
            switch (e.Event.EventType)
            {
                case MidiEventType.SetTempo:
                    var tempo = e.Event as SetTempoEvent;
                    playback.Speed = tempo.MicrosecondsPerQuarterNote;
                    return;
                case MidiEventType.NoteOn:
                    var note = e.Event as NoteOnEvent;
                    if (note != null && note.Velocity <= 0) return;

                    //Check if the user has tabbed out of warframe, and stop playback to avoid Scale issues
                    if (!(ActionManager.PlayNote(note, EnableVibrato, TransposeNotes) || PlayThroughSpeakers)) PlayPause();
                    return;
                default:
                    return;
            }
        }

        public void OnNoteEvent(object sender, MidiEventReceivedEventArgs e)
        {
            if (e.Event.EventType != MidiEventType.NoteOn) return;

            var note = e.Event as NoteOnEvent;
            if (note != null && note.Velocity <= 0) return;

            ActionManager.PlayNote(note, EnableVibrato, TransposeNotes);
        }

        private void UpdateSlider(double value)
        {
            _ignoreSliderChange = true;
            SongSlider = value;
        }

        #endregion
    }
}

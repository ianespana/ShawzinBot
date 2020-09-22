using Melanchall.DryWetMidi.Smf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace ShawzinBot.Models
{
    public class MidiTrackModel
    {
        private bool _isChecked;

        public string TrackName { get; private set; }
        public TrackChunk Track { get; private set; }
        public bool IsChecked {
            get => _isChecked;
            set
            {
                _isChecked = value;
                ViewModels.MainViewModel.reloadPlayback = true;
            }
        }

        public MidiTrackModel(TrackChunk track)
        {
            Track = track;
            TrackName = track.Events.OfType<SequenceTrackNameEvent>().FirstOrDefault()?.Text;
        }

        public MidiTrackModel(TrackChunk track, bool isChecked)
        {
            Track = track;
            TrackName = track.Events.OfType<SequenceTrackNameEvent>().FirstOrDefault()?.Text;
            IsChecked = isChecked;
        }
    }
}

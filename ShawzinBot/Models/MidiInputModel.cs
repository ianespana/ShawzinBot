using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ShawzinBot.Models
{
    public class MidiInputModel
    {
        public string DeviceName { get; private set; }

        public MidiInputModel(string deviceName)
        {
            DeviceName = deviceName;
        }
    }
}

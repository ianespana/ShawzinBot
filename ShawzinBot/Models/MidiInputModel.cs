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

namespace ShawzinBot.Models
{
    public class MidiSpeedModel
    {
        public string SpeedName { get; private set; }
        public double Speed { get; private set; }

        public MidiSpeedModel(string speedName, double speed)
        {
            SpeedName = speedName;
            Speed = speed;
        }
    }
}

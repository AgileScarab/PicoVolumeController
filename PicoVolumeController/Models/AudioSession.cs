using CoreAudio;

namespace PicoVolumeController.Models
{
    public class AudioSession
    {
        public string DisplayName { get; set; }
        public AudioSessionControl2 SessionControl2 { get; set; }
        public AudioSession(string displayName, AudioSessionControl2 sessionControl2)
        {
            DisplayName = displayName;
            SessionControl2 = sessionControl2;
        }
    }
}

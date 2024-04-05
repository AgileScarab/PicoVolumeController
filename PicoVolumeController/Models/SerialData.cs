using System.ComponentModel;

namespace PicoVolumeController.Models
{
    public enum EncoderAction
    {
        Increase,
        Decrease,
        Mute
    }
    public class SerialData
    {
        public int Encoder { get; set; }
        public EncoderAction Action { get; set; }


        public SerialData(int encoder, EncoderAction action)
        {
            Encoder = encoder;
            Action = action;
        }
    }
}

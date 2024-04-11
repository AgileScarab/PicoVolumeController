using PicoVolumeController.Models;
using System.IO.Ports;

namespace PicoVolumeController.Services
{
    public class SerialPortService
    {
        private SerialPort? _serialPort;
        public event EventHandler<Models.SerialData>? DataReceived;
        public void Initialize(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate)
            {
                RtsEnable = true,
                DtrEnable = true
            };
            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.Open();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string data = sp.ReadExisting();
            Models.SerialData? serialData = ProcessData(data);
            if (serialData != null)
            {
                OnDataReceived(serialData);
            }
        }

        private static Models.SerialData? ProcessData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return null;
            data = data.Trim().Replace("\r", string.Empty).Replace("\n", string.Empty); //Clean the string
            int encoder = 0;
            int value = 0;

            EncoderAction action = EncoderAction.Decrease;
            try
            {
                encoder = Convert.ToInt32(data.Split(':')[0]);
                value = Convert.ToInt32(data.Split(':')[1]);
            }
            catch { }//data is sometimes empty eventhough we check if its null or empty, how? i do not know

            switch (value)
            {
                case 0:
                    action = EncoderAction.Decrease;
                    break;
                case 1:
                    action = EncoderAction.Increase;
                    break;
                case 2:
                    action = EncoderAction.Mute;
                    break;
            }
            if (encoder == 0)
                return null;

            return new Models.SerialData(encoder, action);
        }

        private void OnDataReceived(Models.SerialData data)
        {
            DataReceived?.Invoke(this, data);
        }

        public void Close()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        public void Dispose()
        {
            Close();
            _serialPort?.Dispose();
        }
    }
}


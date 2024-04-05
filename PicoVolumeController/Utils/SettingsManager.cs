using PicoVolumeController.Models;
using System.Text.Json;


namespace PicoVolumeController.Utils
{
    public class SettingsManager
    {
        private readonly string _path = Path.GetDirectoryName(Application.ExecutablePath)!;
        private string settingsText;
        public Settings Settings { get; private set; }

        public SettingsManager(string fileName)
        {
            _path = Path.Combine(_path, fileName);

            if (!File.Exists(_path))
            {
                throw new FileNotFoundException();
            }
            settingsText = File.ReadAllText($"{_path}");
            Settings = JsonSerializer.Deserialize<Settings>(settingsText) ?? throw new InvalidOperationException("Settings did not match json format");
        }
    }
}

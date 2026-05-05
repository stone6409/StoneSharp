// UserConfigurationManager.cs
using System.Text.Json;
using System.Threading.Tasks;

namespace StoneSharp.Core.Settings
{
    public class UserConfigurationManager
    {
        private readonly string _userConfigPath;
        private UserSettings _userSettings;

        public UserConfigurationManager()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _userConfigPath = Path.Combine(appDataPath, "CodeAI", "user-settings.json");

            // 确保目录存在
            Directory.CreateDirectory(Path.GetDirectoryName(_userConfigPath));

            // 加载用户设置
            LoadUserSettings();
        }

        public AiModelSettings GetAiModelSettings()
        {
            return _userSettings.AiModelSettings;
        }

        public void UpdateAiModelSettings(AiModelSettings settings)
        {
            _userSettings.AiModelSettings = settings;
            SaveUserSettings();
        }

        public async Task UpdateAiModelSettingsAsync(AiModelSettings settings)
        {
            _userSettings.AiModelSettings = settings;
            await SaveUserSettingsAsync();
        }

        private void LoadUserSettings()
        {
            if (File.Exists(_userConfigPath))
            {
                var json = File.ReadAllText(_userConfigPath);
                _userSettings = JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
            }
            else
            {
                _userSettings = new UserSettings();
            }
        }

        private async Task LoadUserSettingsAsync()
        {
            if (File.Exists(_userConfigPath))
            {
                var json = await File.ReadAllTextAsync(_userConfigPath);
                _userSettings = JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
            }
            else
            {
                _userSettings = new UserSettings();
            }
        }

        private void SaveUserSettings()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_userSettings, options);
            File.WriteAllText(_userConfigPath, json);
        }

        private async Task SaveUserSettingsAsync()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_userSettings, options);
            await File.WriteAllTextAsync(_userConfigPath, json);
        }
    }

    public class UserSettings
    {
        public AiModelSettings AiModelSettings { get; set; } = new AiModelSettings();
        public ChatSettings ChatSettings { get; set; } = new ChatSettings();
    }
}
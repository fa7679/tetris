using System.IO;
using System.Security.Cryptography;
using System.Text;
using CDR_AI_Assistant.Models;
using Newtonsoft.Json;

namespace CDR_AI_Assistant.Services;

public class ConfigManager
{
    private readonly string _configPath;
    private readonly string _logPath;
    private AppConfig? _cachedConfig;

    public ConfigManager()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var pluginFolder = Path.Combine(appData, "CDR_AI_Assistant");
        Directory.CreateDirectory(pluginFolder);

        _configPath = Path.Combine(pluginFolder, "config.json");
        _logPath = Path.Combine(pluginFolder, "plugin.log");
    }

    public AppConfig Load()
    {
        if (_cachedConfig != null)
            return _cachedConfig;

        if (File.Exists(_configPath))
        {
            try
            {
                var json = File.ReadAllText(_configPath);
                _cachedConfig = JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
            }
            catch (Exception ex)
            {
                LogError($"配置文件读取失败: {ex.Message}");
                _cachedConfig = new AppConfig();
            }
        }
        else
        {
            _cachedConfig = new AppConfig();
        }

        return _cachedConfig;
    }

    public void Save(AppConfig config)
    {
        try
        {
            // 加密 OpenAI API Key
            if (!string.IsNullOrEmpty(config.ApiKey) && string.IsNullOrEmpty(config.EncryptedApiKey))
            {
                config.EncryptedApiKey = Encrypt(config.ApiKey);
            }
            config.ApiKey = null;

            // 加密 DeepL API Key
            if (!string.IsNullOrEmpty(config.DeepLApiKey) && string.IsNullOrEmpty(config.DeepLApiKeyEncrypted))
            {
                config.DeepLApiKeyEncrypted = Encrypt(config.DeepLApiKey);
            }
            config.DeepLApiKey = null;

            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(_configPath, json);
            _cachedConfig = config;
        }
        catch (Exception ex)
        {
            LogError($"配置保存失败: {ex.Message}");
            throw;
        }
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        var data = Encoding.UTF8.GetBytes(plainText);
        var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        try
        {
            var data = Convert.FromBase64String(cipherText);
            var decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            return string.Empty;
        }
    }

    public void Log(string message)
    {
        try
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            File.AppendAllText(_logPath, logEntry + Environment.NewLine);
        }
        catch { }
    }

    public void LogError(string message)
    {
        Log($"[ERROR] {message}");
    }
}
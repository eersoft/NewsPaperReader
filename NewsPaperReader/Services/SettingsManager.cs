using System.IO;
using System.Text.Json;
using NewsPaperReader.Models;

namespace NewsPaperReader.Services
{
    /// <summary>
    /// 设置管理器
    /// </summary>
    public class SettingsManager
    {
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "esNewsPaperReader",
            "settings.json"
        );

        /// <summary>
        /// 读取设置
        /// </summary>
        /// <returns>应用程序设置</returns>
        public static AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    // 如果设置文件不存在，返回默认设置
                    return new AppSettings();
                }
            }
            catch
            {
                // 如果读取失败，返回默认设置
                return new AppSettings();
            }
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="settings">应用程序设置</param>
        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                // 确保目录存在
                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 序列化并保存设置
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch
            {
                // 忽略保存失败的情况
            }
        }
    }
}

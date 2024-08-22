using Hermes.Common;
using Hermes.Models;
using System.IO;
using System.Text.Json;
using System;
using System.Globalization;
using System.Threading;
using Hermes.Common.Extensions;
using Hermes.Types;

namespace Hermes.Repositories;

public class SettingsRepository : ISettingsRepository
{
    public event Action<Settings>? SettingsChanged;
    public string Path { get; init; }
    public string FileName { get; init; } = "settings.json";

    public virtual Settings Settings
    {
        get => _settings;
        private set
        {
            this.SettingsChanged?.Invoke(value);
            _settings = value;
        }
    }

    private readonly AesEncryptor _aesEncryptor;
    private Settings _settings = new();

    public SettingsRepository(AesEncryptor aesEncryptor)
    {
        this._aesEncryptor = aesEncryptor;
        this.Path = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Name!,
            FileName);
    }

    public void Save(Settings settings)
    {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path) ?? string.Empty);
        var jsonData = JsonSerializer.Serialize(settings);
        var encryptedData = _aesEncryptor.Encrypt(jsonData);
        using var fileStream = File.Create(Path);
        using var writer = new StreamWriter(fileStream);
        writer.Write(encryptedData);
        this.Settings = settings;
    }

    public Settings Load()
    {
        this.Settings = this.Read();
        SetLanguage(Settings.Language);
        return this.Settings;
    }

    public Settings Read()
    {
        Settings? settings = null;
        if (File.Exists(Path))
        {
            using var fileStream = File.OpenRead(Path);
            using var reader = new StreamReader(fileStream);
            var encryptedData = reader.ReadToEnd();
            var jsonData = _aesEncryptor.Decrypt(encryptedData);
            settings = JsonSerializer.Deserialize<Settings>(jsonData);
        }

        return settings ?? new Settings();
    }

    public static void SetLanguage(LanguageType language)
    {
        try
        {
            var info = new CultureInfo(language.GetDescription())
            {
                NumberFormat = CultureInfo.CreateSpecificCulture(language.GetDescription()).NumberFormat
            };
            Thread.CurrentThread.CurrentUICulture = info;
            Thread.CurrentThread.CurrentCulture = info;
        }
        catch (Exception)
        {
            // ignored
        }
    }
}
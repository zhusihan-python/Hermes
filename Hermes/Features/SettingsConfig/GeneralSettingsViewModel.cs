using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core.Models;
using ConfigFactory.Core;
using Hermes.Language;
using ConfigFactory.Core.Attributes;
using Hermes.Types;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using System;

namespace Hermes.Features.SettingsConfig;

public partial class GeneralSettingsViewModel : ConfigModule<GeneralSettingsViewModel>
{
    // Use a 32-byte key for AES-256 and a 16-byte IV
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // 32 bytes
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("1234567890123456"); // 16 bytes


    //public LogfileType LogfileType { get; set; } = LogfileType.TriDefault; // TODO
    //public int WatchDelayMilliseconds { get; set; } = 500;


    [ObservableProperty]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        Filter = "Folder:*.*",
        InstanceBrowserKey = "some-browser-field-key")]
    [property: Config(
        Header = "Input Path",
        Description = "Put Input Path",
        Category = "General",
        Group = "Common")]
    public string _InputPath = @$"{Directory.GetCurrentDirectory()}\Input";

    [ObservableProperty]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        Filter = "Folder:*.*",
        InstanceBrowserKey = "some-browser-field-key")]
    [property: Config(
        Header = "Backup Input Path",
        Description = "Put Backup Input Path",
        Category = "General",
        Group = "Common")]
    public string _BackupPath = @$"{Directory.GetCurrentDirectory()}\BackupInput";

    [ObservableProperty]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        Filter = "Folder:*.*",
        InstanceBrowserKey = "some-browser-field-key")]
    [property: Config(
        Header = "SFC Path",
        Description = "Put SFC Path",
        Category = "General",
        Group = "Common")]
    public string _SfcPath = @$"{Directory.GetCurrentDirectory()}\SFC";

    [ObservableProperty]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFolder,
        Filter = "Folder:*.*",
        InstanceBrowserKey = "some-browser-field-key")]
    [property: Config(
        Header = "Backup SFC Path",
        Description = "Put Backup SFC Path",
        Category = "General",
        Group = "Common")]
    public string _BackupSfcPath = @$"{Directory.GetCurrentDirectory()}\BackupTraceability";

    [ObservableProperty]
    [property: Config(
        Header = "Autostar",
        Description = "Put Autostar",
        Category = "General",
        Group = "Common")]
    private bool _AutostartUutProcessor = false;

    [ObservableProperty]
    [property: DropdownConfig(
        RuntimeItemsSourceMethodName = "SelectLanguage")]
    [property: Config(
        Header = "Language",
        Description = "Language Station",
        Category = "General",
        Group = "Common")]
    private string _Language = "ES";

    [ObservableProperty]
    [property: DropdownConfig(
        RuntimeItemsSourceMethodName = "SelectStation")]
    [property: Config(
        Header = "Station_",
        Description = "Put Station_",
        Category = "General",
        Group = "Station")]
    private string _Station = "SPIBOT";

    [ObservableProperty]
    [property: DropdownConfig(
        RuntimeItemsSourceMethodName = "SelectMachine")]
    [property: Config(
        Header = "Machine_",
        Description = "Put Machine_",
        Category = "General",
        Group = "Station")]
    private string _Machine = "SPI";

    [ObservableProperty]
    [property: DropdownConfig(
        RuntimeItemsSourceMethodName = "SelectLine")]
    [property: Config(
        Header = "Line_",
        Description = "Put Line_",
        Category = "General",
        Group = "Station")]
    private string _Line = "AG01";

    [ObservableProperty]
    [property: DropdownConfig(
        RuntimeItemsSourceMethodName = "SfcExtension")]
    [property: Config(
        Header = "File Extension",
        Description = "Put File SFC Extension",
        Category = "General",
        Group = "Station")]
    private string _SfcResponseExtension = ".log";

    [ObservableProperty]
    [property: DropdownConfig(
        RuntimeItemsSourceMethodName = "FileExtension")]
    [property: Config(
        Header = "SFC File Extension",
        Description = "Put File Extension",
        Category = "General",
        Group = "Station")]
    private string _InputFileExtension = ".3dx";

    [ObservableProperty]
    [property: Config(
        Header = "Send Repair File",
        Description = "Put Send Repair File",
        Category = "General",
        Group = "Station")]
    private bool _SendRepairFile = true;

    [ObservableProperty]
    [property: Config(
        Header = "Repair Boards",
        Description = "Put Repair Boards",
        Category = "General",
        Group = "Station")]
    private bool _RepairBoards = false;

    [ObservableProperty]
    [property: NumericConfig(
        Minimum = 1,
        Maximum = 12,
        Increment = 1)]
    [property: Config(
        Header = "Keep Logs For Months",
        Description = "Put Keep Logs For Months",
        Category = "General",
        Group = "Logs & Time")]
    private int _MonthsToKeepLogs = 3;

    [ObservableProperty]
    [property: NumericConfig(
        Minimum = 7,
        Maximum = 40,
        Increment = 1)]
    [property: Config(
        Header = "Sfc Timeout",
        Description = "Put Sfc Timeout",
        Category = "General",
        Group = "Logs & Time")]
    private int _SfcTimeoutSeconds = 8;

    [ObservableProperty]
    [property: NumericConfig(
        Minimum = 3,
        Maximum = 30,
        Increment = 1)]
    [property: Config(
        Header = "Window Timeout",
        Description = "Put Window Timeout",
        Category = "General",
        Group = "Logs & Time")]
    private int _UutSuccessWindowTimeoutSeconds = 5;

    public static ObservableCollection<string> SelectLanguage()
    {
        return new()
        {
            { "EN" },
            { "ES" },
        };
    }

    public static ObservableCollection<string> SelectMachine()
    {
        return new()
        {
            { "SPI" },
            { "AOI" },
            { "AXI" },
            { "MAGIC" },
            { "LABELING" },
        };
    }

    public static ObservableCollection<string> SelectStation()
    {
        return new()
        {
            { "SPIBOT" },
            { "SPITOP" },
            { "AOI1" },
            { "AOI2" },
            { "AOI3" },
            { "AOI4" },
            { "AXI" },
            { "PTH" },
            { "LABELING" },
        };
    }

    public static ObservableCollection<string> SelectLine()
    {
        return new()
        {
            { "AG01" },
            { "AG02" },
            { "AG03" },
            { "AG04" },
            { "AG05" },
            { "AG06" },
            { "AG07" },
        };
    }

    public static ObservableCollection<string> SfcExtension()
    {
        return new()
        {
            { ".log" },
            { ".ret" },
        };
    }

    public static ObservableCollection<string> FileExtension()
    {
        return new()
        {
            { ".3dx" },
            { ".rle" },
            { ".info" },
            { ".res" },
            { ".txt" },
        };
    }

    partial void OnSendRepairFileChanged(bool value)
    {
        Validate(() => SendRepairFile, value => { return value is true; });
    }

    partial void OnRepairBoardsChanged(bool value)
    {
        Validate(() => RepairBoards, value => { return value is true; });
    }

    partial void OnAutostartUutProcessorChanged(bool value)
    {
        Validate(() => AutostartUutProcessor, value => { return value is true; });
    }

    partial void OnMachineChanged(string? value)
    {
        switch (value)
        {
            case "AXI":
                SfcResponseExtension = ".ret";
                InputFileExtension = ".rle";
                SendRepairFile = true;
                break;
            case "SPI":
                SfcResponseExtension = ".log";
                InputFileExtension = ".3dx";
                SendRepairFile = false;
                break;
            case "AOI":
                SfcResponseExtension = ".log";
                InputFileExtension = ".3dx";
                SendRepairFile = true;
                break;
            case "MAGIC":
                SfcResponseExtension = ".log";
                InputFileExtension = ".txt";
                SendRepairFile = true;
                break;
            case "LABELING":
                SfcResponseExtension = ".log";
                InputFileExtension = ".res";
                SendRepairFile = true;
                break;
            default:
                SfcResponseExtension = ".log";
                InputFileExtension = ".3dx";
                SendRepairFile = true;
                break;
        }
    }

    partial void OnStationChanged(string? value)
    {
        switch (value)
        {
            case "SPIBOT":
                Machine = "SPI";
                SendRepairFile = false;
                break;
            case "SPITOP":
                Machine = "SPI";
                SendRepairFile = false;
                break;
            case "AOI1":
                Machine = "AOI";
                SendRepairFile = true;
                break;
            case "AOI2":
                Machine = "AOI";
                SendRepairFile = true;
                break;
            case "AOI3":
                Machine = "AOI";
                SendRepairFile = true;
                break;
            case "AOI4":
                Machine = "AOI";
                SendRepairFile = true;
                break;
            case "AXI":
                Machine = "AXI";
                SendRepairFile = true;
                break;
            case "PTH":
                Machine = "MAGIC";
                SendRepairFile = true;
                break;
            case "LABELING":
                Machine = "LABELING";
                SendRepairFile = true;
                break;
            default:
                Machine = "AOI";
                SendRepairFile = true;
                break;
        }
    }

    public override string Translate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        return Resources.ResourceManager?.GetString(input, Resources.Culture) ?? input;
    }

    partial void OnInputPathChanged(string value)
    {
        if (ValidatePath(value) && ValidatePath(SfcPath))
        {
            AutostartUutProcessor = true;
        }
        else
        {
            AutostartUutProcessor = false;
        }
    }

    partial void OnSfcPathChanged(string value)
    {
        if (ValidatePath(value) && ValidatePath(InputPath))
        {
            AutostartUutProcessor = true;
        }
        else
        {
            AutostartUutProcessor = false;
        }
    }

    // Método para validar rutas
    private bool ValidatePath(string path)
    {
        return Directory.Exists(path);
    }

    public GeneralSettingsViewModel()
    {
        InitializeEventHandlers();
    }

    public override void Load(ref GeneralSettingsViewModel module)
    {
        Console.WriteLine("Cargando datos...");
        try
        {
            if (File.Exists(module.LocalPath))
            {
                using FileStream fileStream = File.OpenRead(module.LocalPath);
                using var reader = new StreamReader(fileStream);
                var encryptedData = reader.ReadToEnd();
                var jsonData = Decrypt(encryptedData);

                if (string.IsNullOrEmpty(jsonData))
                {
                    throw new JsonException("Decrypted data is empty or null.");
                }

                module = JsonSerializer.Deserialize<GeneralSettingsViewModel>(jsonData);
            }

            foreach (KeyValuePair<string, ConfigProperty> property2 in module.Properties)
            {
                property2.Deconstruct(out var key, out var value);
                value.Deconstruct(out PropertyInfo property, out ConfigAttribute _);
                string text = key;
                PropertyInfo propertyInfo = property;
                typeof(GeneralSettingsViewModel)
                    .GetMethod("On" + text + "Changed", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(module, new object[1] { propertyInfo.GetValue(module) });
            }

            Console.WriteLine("Datos cargados");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings: {ex.Message}");
            throw;
        }
    }

    public override void Save()
    {
        try
        {
            if (this.OnSavingHandler())
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LocalPath));
                using FileStream fileStream = File.Create(LocalPath);
                using var writer = new StreamWriter(fileStream);
                var jsonData = JsonSerializer.Serialize(this);
                var encryptedData = Encrypt(jsonData);
                writer.Write(encryptedData);
                this.OnSaveHandler();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving settings: {ex.Message}");
            throw;
        }
    }

    private static string Encrypt(string plainText)
    {
        try
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream ms = new();
            using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
            using StreamWriter sw = new(cs);
            sw.Write(plainText);
            sw.Close();

            return Convert.ToBase64String(ms.ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Encryption error: {ex.Message}");
            throw;
        }
    }

    private static string Decrypt(string cipherText)
    {
        try
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream ms = new(Convert.FromBase64String(cipherText));
            using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
            using StreamReader sr = new(cs);
            return sr.ReadToEnd();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Decryption error: {ex.Message}");
            throw;
        }
    }

    private bool OnSavingHandler()
    {
        Console.WriteLine("Datos guardados en progreso...");
        return true;
    }

    private void OnSaveHandler()
    {
        Console.WriteLine("Se guardaron los datos");
    }

    private void InitializeEventHandlers()
    {
        var onSavingEvent =
            typeof(ConfigModule<GeneralSettingsViewModel>).GetField("OnSaving",
                BindingFlags.Instance | BindingFlags.NonPublic);
        var onSaveEvent =
            typeof(ConfigModule<GeneralSettingsViewModel>).GetField("OnSave", BindingFlags.Instance | BindingFlags.NonPublic);

        if (onSavingEvent != null)
        {
            var onSavingDelegate = (Func<bool>)onSavingEvent.GetValue(this);
            onSavingDelegate += OnSavingHandler;
            onSavingEvent.SetValue(this, onSavingDelegate);
        }

        if (onSaveEvent != null)
        {
            var onSaveDelegate = (Action)onSaveEvent.GetValue(this);
            onSaveDelegate += OnSaveHandler;
            onSaveEvent.SetValue(this, onSaveDelegate);
        }
    }
}
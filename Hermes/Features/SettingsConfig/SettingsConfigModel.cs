using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core.Attributes;
using Hermes.Common.Extensions;
using Hermes.Common;
using Hermes.Repositories;
using Hermes.Types;
#pragma warning disable CS0657 // Not a valid attribute location for this declaration

namespace Hermes.Features.SettingsConfig;

public partial class SettingsConfigModel(
    ILogger logger,
    SettingsRepository settingsRepository)
    : BaseConfigModel<SettingsConfigModel>(logger, settingsRepository)
{
    #region General

    [ObservableProperty]
    [property: DropdownConfig(RuntimeItemsSourceMethodName = "Languages")]
    [property: Config(Header = "Language", Description = "Language Station",
        Category = "General", Group = "Common")]
    private LanguageType _language = LanguageType.En;

    public static LanguageType[] Languages => EnumExtensions.GetValues<LanguageType>();

    [ObservableProperty]
    [property: NumericConfig(Minimum = 100, Maximum = 2000, Increment = 1)]
    [property: Config(Header = "Wait Delay Ms", Description = "General delay while waiting for actions to happen",
        Category = "General", Group = "Common")]
    private int _waitDelayMilliseconds = 100;

    [ObservableProperty]
    [property: DropdownConfig(RuntimeItemsSourceMethodName = "StationType")]
    [property: Config(Header = "Station", Description = "Station type",
        Category = "General", Group = "Station")]
    private StationType _station = StationType.SpiBottom;

    public static StationType[] StationTypes => EnumExtensions.GetValues<StationType>();

    [ObservableProperty]
    [property: DropdownConfig(RuntimeItemsSourceMethodName = "MachineType")]
    [property: Config(Header = "Machine type", Description = "Machine type",
        Category = "General", Group = "Station")]
    private MachineType _machine = MachineType.Spi;

    public static MachineType[] MachineTypes => EnumExtensions.GetValues<MachineType>();

    [ObservableProperty]
    [property: DropdownConfig(RuntimeItemsSourceMethodName = "LineType")]
    [property: Config(Header = "Line name", Description = "Line name",
        Category = "General", Group = "Station")]
    private LineType _line = LineType.Ag01;

    public static LineType[] LineTypes => EnumExtensions.GetValues<LineType>();

    [ObservableProperty]
    [property: DropdownConfig(RuntimeItemsSourceMethodName = "LogfileTypes")]
    [property: Config(Header = "Logfile type", Description = "Logfile type",
        Category = "General", Group = "Station")]
    private LogfileType _logfileType = LogfileType.TriDefault;

    public static LogfileType[] LogfileTypes => EnumExtensions.GetValues<LogfileType>();

    [ObservableProperty]
    [property: DropdownConfig(RuntimeItemsSourceMethodName = "FileExtensions")]
    [property: Config(Header = "Sfc response extension", Description = "Sfc response extension",
        Category = "General", Group = "Station")]
    private FileExtension _sfcResponseExtension = FileExtension.Ret;

    [ObservableProperty]
    [property: DropdownConfig(RuntimeItemsSourceMethodName = "FileExtensions")]
    [property: Config(Header = "Input file extension", Description = "Input file extension",
        Category = "General", Group = "Station")]
    private FileExtension _inputFileExtension = FileExtension.Ret;

    [ObservableProperty]
    [property: Config(Header = "Send Repair File", Description = "Put Send Repair File",
        Category = "General", Group = "Station")]
    private bool _sendRepairFile = true;

    #endregion

    #region UutProcessor

    [ObservableProperty]
    [property: Config(Header = "Autostart UUT Processor", Description = "Put Autostart",
        Category = "Uut processor", Group = "Common")]
    private bool _autostartUutProcessor;

    [ObservableProperty]
    [property: NumericConfig(Minimum = 0, Maximum = 5, Increment = 1)]
    [property: Config(Header = "Max retries", Description = "Max sfc send retries when failure",
        Category = "Uut processor", Group = "Common")]
    private int _maxSfcRetries = 3;

    [ObservableProperty]
    [property: BrowserConfig(BrowserMode = BrowserMode.OpenFolder, Filter = "Folder:*.*")]
    [property: Config(Header = "Input Path", Description = "Put Input Path",
        Category = "Uut processor", Group = "Paths")]
    private string _inputPath = string.Empty;

    [ObservableProperty]
    [property: BrowserConfig(BrowserMode = BrowserMode.OpenFolder, Filter = "Folder:*.*")]
    [property: Config(Header = "Backup Path", Description = "Put Backup Path",
        Category = "Uut processor", Group = "Paths")]
    private string _backupPath = string.Empty;

    [ObservableProperty]
    [property: BrowserConfig(BrowserMode = BrowserMode.OpenFolder, Filter = "Folder:*.*")]
    [property: Config(Header = "Sfc Path", Description = "Put Sfc Path",
        Category = "Uut processor", Group = "Paths")]
    private string _sfcPath = string.Empty;

    public static FileExtension[] FileExtensions => EnumExtensions.GetValues<FileExtension>();

    [ObservableProperty]
    [property: NumericConfig(Minimum = 3, Maximum = 30, Increment = 1)]
    [property: Config(Header = "Sfc Timeout", Description = "Sfc Timeout",
        Category = "Uut processor", Group = "Time")]
    private int _sfcTimeoutSeconds = 10;

    [ObservableProperty]
    [property: NumericConfig(Minimum = 3, Maximum = 30, Increment = 1)]
    [property: Config(Header = "Success window timeout", Description = "Close success window whe this time is over",
        Category = "Uut processor", Group = "Time")]
    private int _uutSuccessWindowTimeoutSeconds = 5;

    #endregion

    partial void OnMachineChanged(MachineType value)
    {
        switch (value)
        {
            case MachineType.Axi:
                LogfileType = LogfileType.VitroxDefault;
                SfcResponseExtension = FileExtension.Ret;
                InputFileExtension = FileExtension.Rle;
                SendRepairFile = true;
                break;
            case MachineType.Spi:
                LogfileType = LogfileType.TriDefault;
                SfcResponseExtension = FileExtension.Log;
                InputFileExtension = FileExtension._3dx;
                SendRepairFile = false;
                break;
            case MachineType.Aoi:
                LogfileType = LogfileType.TriDefault;
                SfcResponseExtension = FileExtension.Log;
                InputFileExtension = FileExtension._3dx;
                SendRepairFile = true;
                break;
            case MachineType.Magic:
                LogfileType = LogfileType.MagicDefault;
                SfcResponseExtension = FileExtension.Log;
                InputFileExtension = FileExtension.Txt;
                SendRepairFile = true;
                break;
            case MachineType.Labeling:
                LogfileType = LogfileType.Default;
                SfcResponseExtension = FileExtension.Log;
                InputFileExtension = FileExtension.Res;
                SendRepairFile = true;
                break;
            default:
                LogfileType = LogfileType.Default;
                SfcResponseExtension = FileExtension.Log;
                InputFileExtension = FileExtension._3dx;
                SendRepairFile = true;
                break;
        }
    }


    partial void OnStationChanged(StationType value)
    {
        switch (value)
        {
            case StationType.SpiBottom:
            case StationType.SpiTop:
                Machine = MachineType.Spi;
                SendRepairFile = false;
                break;
            case StationType.Axi:
                Machine = MachineType.Axi;
                SendRepairFile = true;
                break;
            case StationType.Pth:
                Machine = MachineType.Magic;
                SendRepairFile = true;
                break;
            case StationType.Labeling:
                Machine = MachineType.Labeling;
                SendRepairFile = true;
                break;
            case StationType.Aoi1:
            case StationType.Aoi2:
            case StationType.Aoi3:
            case StationType.Aoi4:
            default:
                Machine = MachineType.Aoi;
                SendRepairFile = true;
                break;
        }
    }
}
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
    ISettingsRepository settingsRepository)
    : BaseConfigModel<SettingsConfigModel>(logger, settingsRepository)
{
    #region General

    [ObservableProperty]
    [property: DropdownConfig(RuntimeItemsSourceMethodName = "Languages")]
    [property: Config(
        Header = "c_settings_header_language",
        Description = "c_settings_description_language",
        Category = "c_settings_category_general",
        Group = "c_settings_group_common")]
    private LanguageType _language = LanguageType.En;

    public static LanguageType[] Languages => EnumExtensions.GetValues<LanguageType>();

    [ObservableProperty]
    [property: NumericConfig(Minimum = 100, Maximum = 2000, Increment = 1)]
    [property: Config(
        Header = "c_settings_header_wait_delay_ms",
        Description = "c_settings_description_wait_delay_ms",
        Category = "c_settings_category_general",
        Group = "c_settings_group_common")]
    private int _waitDelayMilliseconds = 100;

    [ObservableProperty]
    [property: DropdownConfig(RuntimeItemsSourceMethodName = "LineType")]
    [property: Config(
        Header = "c_settings_header_line_name",
        Description = "c_settings_header_line_name",
        Category = "c_settings_category_general",
        Group = "c_settings_group_station")]
    private LineType _line = LineType.Ag01;

    public static LineType[] LineTypes => EnumExtensions.GetValues<LineType>();

    [ObservableProperty]
    [property: DropdownConfig(RuntimeItemsSourceMethodName = "StationType")]
    [property: Config(
        Header = "c_settings_header_station",
        Description = "c_settings_header_station",
        Category = "c_settings_category_general",
        Group = "c_settings_group_station")]
    private StationType _station = StationType.SpiBottom;

    [ObservableProperty]
    [property: Config(
        Header = "c_settings_header_station_id",
        Description = "c_settings_header_station_id",
        Category = "c_settings_category_general",
        Group = "c_settings_group_station")]
    private string _stationId = "";

    public static StationType[] StationTypes => EnumExtensions.GetValues<StationType>();

    [ObservableProperty]
    [property: DropdownConfig(RuntimeItemsSourceMethodName = "MachineType")]
    [property: Config(
        Header = "c_settings_header_machine_type",
        Description = "c_settings_header_machine_type",
        Category = "c_settings_category_general",
        Group = "c_settings_group_station")]
    private MachineType _machine = MachineType.Spi;

    public static MachineType[] MachineTypes => EnumExtensions.GetValues<MachineType>();

    [ObservableProperty]
    [property: DropdownConfig(RuntimeItemsSourceMethodName = "LogfileTypes")]
    [property: Config(
        Header = "c_settings_header_logfile_type",
        Description = "c_settings_header_logfile_type",
        Category = "c_settings_category_general",
        Group = "c_settings_group_station")]
    private LogfileType _logfileType = LogfileType.TriDefault;

    public static LogfileType[] LogfileTypes => EnumExtensions.GetValues<LogfileType>();

    [ObservableProperty]
    [property: DropdownConfig(RuntimeItemsSourceMethodName = "FileExtensions")]
    [property: Config(
        Header = "c_settings_header_sfc_response_ext",
        Description = "c_settings_header_sfc_response_ext",
        Category = "c_settings_category_general",
        Group = "c_settings_group_station")]
    private FileExtension _sfcResponseExtension = FileExtension.Ret;

    [ObservableProperty]
    [property: DropdownConfig(RuntimeItemsSourceMethodName = "FileExtensions")]
    [property: Config(
        Header = "c_settings_header_input_file_ext",
        Description = "c_settings_header_input_file_ext",
        Category = "c_settings_category_general",
        Group = "c_settings_group_station")]
    private FileExtension _inputFileExtension = FileExtension.Ret;

    [ObservableProperty]
    [property: Config(
        Header = "c_settings_header_send_repair_file",
        Description = "c_settings_header_send_repair_file",
        Category = "c_settings_category_general",
        Group = "c_settings_group_station")]
    private bool _sendRepairFile = true;

    #endregion

    #region UutProcessor

    [ObservableProperty]
    [property: Config(
        Header = "c_settings_header_autostart_uut_processor",
        Description = "c_settings_header_autostart_uut_processor",
        Category = "c_settings_category_uut_processor",
        Group = "c_settings_group_common")]
    private bool _autostartUutProcessor;

    [ObservableProperty]
    [property: NumericConfig(Minimum = 0, Maximum = 5, Increment = 1)]
    [property: Config(
        Header = "c_settings_header_max_retries",
        Description = "c_settings_description_max_retries",
        Category = "c_settings_category_uut_processor",
        Group = "c_settings_group_common")]
    private int _maxSfcRetries = 3;

    [ObservableProperty]
    [property: BrowserConfig(BrowserMode = BrowserMode.OpenFolder, Filter = "Folder:*.*")]
    [property: Config(
        Header = "c_settings_header_input_path",
        Description = "c_settings_header_input_path",
        Category = "c_settings_category_uut_processor",
        Group = "c_settings_group_paths")]
    private string _inputPath = string.Empty;

    [ObservableProperty]
    [property: BrowserConfig(BrowserMode = BrowserMode.OpenFolder, Filter = "Folder:*.*")]
    [property: Config(
        Header = "c_settings_header_backup_path",
        Description = "c_settings_header_backup_path",
        Category = "c_settings_category_uut_processor",
        Group = "c_settings_group_paths")]
    private string _backupPath = string.Empty;

    [ObservableProperty]
    [property: BrowserConfig(BrowserMode = BrowserMode.OpenFolder, Filter = "Folder:*.*")]
    [property: Config(
        Header = "c_settings_header_sfc_path",
        Description = "c_settings_header_sfc_path",
        Category = "c_settings_category_uut_processor",
        Group = "c_settings_group_paths")]
    private string _sfcPath = string.Empty;

    public static FileExtension[] FileExtensions => EnumExtensions.GetValues<FileExtension>();

    [ObservableProperty]
    [property: NumericConfig(Minimum = 3, Maximum = 30, Increment = 1)]
    [property: Config(
        Header = "c_settings_header_sfc_timeout",
        Description = "c_settings_header_sfc_timeout",
        Category = "c_settings_category_uut_processor",
        Group = "c_settings_group_time")]
    private int _sfcTimeoutSeconds = 10;

    [ObservableProperty]
    [property: NumericConfig(Minimum = 3, Maximum = 30, Increment = 1)]
    [property: Config(
        Header = "c_settings_header_success_window_timeout",
        Description = "c_settings_description_success_window_timeout",
        Category = "c_settings_category_uut_processor",
        Group = "c_settings_group_time")]
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
                LogfileType = LogfileType.LabelingMachineDefault;
                SfcResponseExtension = FileExtension.Log;
                InputFileExtension = FileExtension.Res;
                SendRepairFile = true;
                break;
            case MachineType.ScreenPrinter:
                LogfileType = LogfileType.GkgDefault;
                SfcResponseExtension = FileExtension.Log;
                InputFileExtension = FileExtension._3dx;
                SendRepairFile = false;
                break;
            default:
                LogfileType = LogfileType.Default;
                SfcResponseExtension = FileExtension.Res;
                InputFileExtension = FileExtension.Txt;
                SendRepairFile = true;
                break;
        }
    }

    partial void OnStationChanged(StationType value)
    {
        switch (value)
        {
            case StationType.Labeling:
                Machine = MachineType.None;
                SendRepairFile = false;
                AutostartUutProcessor = false;
                break;
            case StationType.LabelingMachine:
                Machine = MachineType.Labeling;
                SendRepairFile = false;
                AutostartUutProcessor = true;
                break;
            case StationType.SpiBottom:
            case StationType.SpiTop:
                Machine = MachineType.Spi;
                SendRepairFile = false;
                AutostartUutProcessor = true;
                break;
            case StationType.Axi:
                Machine = MachineType.Axi;
                SendRepairFile = true;
                AutostartUutProcessor = true;
                break;
            case StationType.Pth:
                Machine = MachineType.Magic;
                SendRepairFile = true;
                AutostartUutProcessor = true;
                break;
            case StationType.ScreenPrinterBottom:
            case StationType.ScreenPrinterTop:
                Machine = MachineType.ScreenPrinter;
                SendRepairFile = false;
                AutostartUutProcessor = true;
                break;
            case StationType.Aoi1:
            case StationType.Aoi2:
            case StationType.Aoi3:
            case StationType.Aoi4:
            default:
                Machine = MachineType.Aoi;
                SendRepairFile = true;
                AutostartUutProcessor = true;
                break;
        }

        this.CalcStationId();
    }

    partial void OnLineChanged(LineType value)
    {
        this.CalcStationId();
    }

    private void CalcStationId()
    {
        var id = Station.GetDescription();
        StationId = !string.IsNullOrEmpty(id) ? $"{(int)Line + 1}1{id}" : string.Empty;
    }
}
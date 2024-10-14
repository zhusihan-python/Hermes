using Hermes.Types;

namespace Hermes.Models;

public class Settings
{
    #region General

    public LanguageType Language { get; set; } = LanguageType.En;
    public int WaitDelayMilliseconds { get; set; } = 100;
    public string DatabaseServer { get; set; } = "10.12.204.48";
    public StationType Station { get; set; } = StationType.SpiBottom;
    public MachineType Machine { get; set; } = MachineType.Spi;
    public string StationId { get; set; } = "";
    public LineType Line { get; set; } = LineType.Ag01;
    public LogfileType LogfileType { get; set; } = LogfileType.TriDefault;
    public FileExtension SfcResponseExtension { get; set; } = FileExtension.Ret;
    public FileExtension InputFileExtension { get; set; } = FileExtension._3dx;
    public bool SendRepairFile { get; set; } = true;

    #endregion

    #region UutProcessor

    public bool AutostartUutProcessor { get; set; } = true;
    public string InputPath { get; set; } = @".\Input";
    public string BackupPath { get; set; } = @".\Backup";
    public string SfcPath { get; set; } = @".\Sfc";
    public int SfcTimeoutSeconds { get; set; } = 10;
    public int UutSuccessWindowTimeoutSeconds { get; set; } = 5;
    public int MaxSfcRetries { get; set; } = 3;
    public string GkgTunnelComPort { get; set; } = "COM50";
    public string ScannerComPort { get; set; } = "COM40";
    public string AdditionalOkSfcResponse { get; set; } = "";

    #endregion
}
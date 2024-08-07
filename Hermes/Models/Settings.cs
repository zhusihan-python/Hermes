using Hermes.Types;

namespace Hermes.Models;

public class Settings
{
    public LogfileType LogfileType { get; set; } = LogfileType.TriDefault; // TODO
    public FileExtension SfcResponseExtension { get; set; } = FileExtension.Ret;
    public bool AutostartUutProcessor { get; set; } = true;
    public int SfcTimeoutSeconds { get; set; } = 10;
    public int WaitDelayMilliseconds { get; set; } = 100;
    public int UutSuccessWindowTimeoutSeconds { get; set; } = 5;
    public string BackupPath { get; set; } = @"C:\Users\Foxconn\Documents\Tpm\HermesLogfiles\Backup"; // TODO
    public FileExtension InputFileExtension { get; set; } = FileExtension._3dx; // TODO
    public string InputPath { get; set; } = @"C:\Users\Foxconn\Documents\Tpm\HermesLogfiles\Input"; // TODO
    public string SfcPath { get; set; } = @"C:\Users\Foxconn\Documents\Tpm\HermesLogfiles\Sfc"; // TODO
    public int MaxSfcRetries { get; set; } = 3;
}
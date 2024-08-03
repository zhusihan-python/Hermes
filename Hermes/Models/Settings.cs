using Hermes.Types;

namespace Hermes.Models;

public class Settings
{
    public LogfileType LogfileType { get; set; } = LogfileType.TriDefault; // TODO
    public FileExtension SfcResponseExtension { get; set; } = FileExtension.Ret;
    public bool AutostartUutProcessor { get; set; } = true;
    public int SfcTimeoutSeconds { get; set; } = 2;
    public int WaitDelayMilliseconds { get; set; } = 100;
    public int UutSuccessWindowTimeoutSeconds { get; set; } = 5;
    public string BackupPath { get; set; } = @"C:\Users\david\Documents\Tmp\Hermes\Backup"; // TODO
    public FileExtension InputFileExtension { get; set; } = FileExtension._3dx; // TODO
    public string InputPath { get; set; } = @"C:\Users\david\Documents\Tmp\Hermes\Input"; // TODO
    public string SfcPath { get; set; } = @"C:\Users\david\Documents\Tmp\Hermes\Sfc"; // TODO
    public int MaxSfcRetries { get; set; } = 3;
}
using Hermes.Types;

namespace Hermes.Models;

public class Settings
{
    public LogfileType LogfileType { get; set; } = LogfileType.TriDefault; // TODO
    public SfcResponseExtension SfcResponseExtension { get; set; } = SfcResponseExtension.RET;
    public bool AutostartUutProcessor { get; set; } = true;
    public int SfcTimeoutSeconds { get; set; } = 2;
    public int SfcWaitDelaySeconds { get; set; } = 1;
    public int UutSuccessWindowTimeoutSeconds { get; set; } = 5;
    public int WatchLogfilesDelayMilliseconds { get; set; } = 500;
    public string BackupPath { get; set; } = @"C:\Users\Foxconn\Documents\Tpm\HermesLogfiles\Backup"; // TODO
    public string InputFileExtension { get; set; } = @".3dx"; // TODO
    public string InputPath { get; set; } = @"C:\Users\Foxconn\Documents\Tpm\HermesLogfiles\Input"; // TODO
    public string SfcPath { get; set; } = @"C:\Users\Foxconn\Documents\Tpm\HermesLogfiles\Sfc"; // TODO
}
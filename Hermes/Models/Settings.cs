using Hermes.Types;

namespace Hermes.Models;

public class Settings
{
    private static Settings? _instance;

    public static Settings Instance => _instance ??= new Settings();

    public string InputPath { get; set; } = @"C:\Users\Foxconn\Documents\Tpm\HermesLogfiles\Input"; // TODO
    public string InputFilter { get; set; } = @"*.*"; // TODO
    public LogfileType LogfileType { get; set; } = LogfileType.TriDefault; // TODO
    public string SfcPath { get; set; } = @"C:\Users\Foxconn\Documents\Tpm\HermesLogfiles\Input\Sfc"; // TODO
    public string BackupPath { get; set; } = @"C:\Users\Foxconn\Documents\Tpm\HermesLogfiles\Input\Backup"; // TODO
    public int WatchLogfilesDelayMilliseconds { get; set; } = 500;
    public int SfcTimeoutSeconds { get; set; } = 5;
    public SfcResponseExtension SfcResponseExtension { get; set; } = SfcResponseExtension.RET;
    public int MinActionsLength { get; set; } = 10;
    public int SuccessWindowTimeout { get; set; } = 5;
    public int SfcWaitDelaySeconds { get; set; } = 1;
}
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace Hermes.Features.AdminTools;

public partial class SystemSetTabViewModel: ViewModelBase
{
    public string SealMediumHeader { get; set; }
    public string SealMediumDescription { get; set; }
    public string SealMediumValue { get; set; }
    public string BakeTempHeader { get; set; }
    public string BakeTempDescription { get; set; }
    public string BakeTempValue { get; set; }
    public string BakeDurationHeader { get; set; }
    public string BakeDurationDescription { get; set; }
    public string BakeDurationValue { get; set; }
    public string SuckerOnePressureHeader { get; set; }
    public string SuckerOneDescription { get; set; }
    public string SuckerOnePressureValue { get; set; }
    public string SuckerTwoPressureHeader { get; set; }
    public string SuckerTwoDescription { get; set; }
    public string SuckerTwoPressureValue { get; set; }
    public string GasTankPressureHeader { get; set; }
    public string GasTankPressureDescription { get; set; }
    public string GasTankPressureValue { get; set; }
    public string BackupIntervalHeader { get; set; }
    public string BackupIntervalDescription { get; set; }
    public string BackupIntervalValue { get; set; }
    public string BackupCountHeader { get; set; }
    public string BackupDescription { get; set; }
    public string BackupCountValue { get; set; }
    [ObservableProperty]
    private string defaultDir = System.IO.Path.Combine(AppContext.BaseDirectory, "Backups");
    [ObservableProperty]
    private string hisInterface = "http://192.168.0.1";
    [ObservableProperty]
    private string pisInterface = "http://192.168.0.2";

    public SystemSetTabViewModel()
    {
        SealMediumHeader = "封片剂添加量";
        SealMediumDescription = "封片剂添加量 单位μL";
        SealMediumValue = "120";

        BakeTempHeader = "烘干温度";
        BakeTempDescription = "封片烘干温度";
        BakeTempValue = "80";

        BakeDurationHeader = "烘干时长";
        BakeDurationDescription = "封片烘干时长 单位分钟";
        BakeDurationValue = "2";

        SuckerOnePressureHeader = "吸盘1压力";
        SuckerOneDescription = "吸盘1压力 -100到0 Kpa";
        SuckerOnePressureValue = "-80";

        SuckerTwoPressureHeader = "吸盘2压力";
        SuckerTwoDescription = "吸盘2压力 -100到0 Kpa";
        SuckerTwoPressureValue = "-80";

        GasTankPressureHeader = "气罐压力";
        GasTankPressureDescription = "气罐压力 0到1000 Kpa";
        GasTankPressureValue = "200";

        BackupIntervalHeader = "备份间隔";
        BackupIntervalDescription = "备份间隔天数";
        BackupIntervalValue = "3";

        BackupCountHeader = "备份数目";
        BackupDescription = "最多保存备份数目";
        BackupCountValue = "4";
    }

    [RelayCommand]
    public async Task OpenFolderAsync(Control control)
    {
        if (control == null)
            return;

        var folder = TopLevel.GetTopLevel(control)?.StorageProvider
            .OpenFolderPickerAsync(new FolderPickerOpenOptions()
            {
                AllowMultiple = false
            });

        if (folder?.Result.Count >= 1)
        {
            DefaultDir = folder.Result[0].Path.LocalPath;
        }
    }
}

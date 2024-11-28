namespace Hermes.Features.Bender;

public partial class OtherTabViewModel: ViewModelBase
{
    public string BakeTempHeader { get; set; }
    public string BakeTempDescription { get; set; }
    public string BakeTempValue { get; set; }
    public string BakeDurationHeader { get; set; }
    public string BakeDurationDescription { get; set; }
    public string BakeDurationValue { get; set; }
    public string SuckerOnePressureHeader {  get; set; }
    public string SuckerOneDescription { get; set; }
    public string SuckerOnePressureValue { get; set; }
    public string SuckerTwoPressureHeader { get; set; }
    public string SuckerTwoDescription { get; set; }
    public string SuckerTwoPressureValue { get; set; }
    public string GasTankPressureHeader { get; set; }
    public string GasTankPressureDescription { get; set; }
    public string GasTankPressureValue { get; set; }

    public OtherTabViewModel()
    {
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
    }
}

//public class PointToBackgroundConverter : IValueConverter
//{
//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        if (value is bool IsPointerOver)
//        {
//            return IsPointerOver ? Brushes.OrangeRed : Brushes.PaleGreen;
//        }
//        return Brushes.LightGray;
//    }

//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        throw new NotImplementedException();
//    }
//}

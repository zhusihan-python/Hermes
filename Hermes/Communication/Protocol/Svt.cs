namespace Hermes.Communication.Protocol;

/// <summary>
///     Defines constants related to the Modbus protocol.
/// </summary>
internal static class Svt
{
    // frame head 0x3C 0x28 frame tail 0x29 0x3E
    //帧头定义
    public static readonly byte[] FullHead = { 0x3C, 0x28 };
    //帧尾定义
    public static readonly byte[] FullTail = { 0x29, 0x3E };
    public const byte InsertStartByte = 0x82; //!0x3C
    public const byte InsertEndByte = 0x83; //!0x3E
    public const byte StartByte = 0x3C;  // "<"
    public const byte StartByte1 = 0x28;  // "("
    public const byte EndByte = 0x29;  // ")"
    public const byte EndByte1 = 0x3E;  // ">"

    // supported function codes
    public const ushort DiagnosticsReturnQueryData = 0;
    public const ushort DeviceId = 0x0020;
    public const ushort BaudRate = 0x0022;
    public const ushort Diagnostics = 0x0026;
    public const ushort BakeParams = 0x0338;
    public const ushort ScanTrigger = 0x033A;
    public const ushort ScanResult = 0x033B;
    public const ushort HeartBeat = 0x033F;
    public const ushort SystemStatus = 0x0340;
    public const ushort FlowAction = 0x0341;

    public const int MaximumDiscreteRequestResponseSize = 65535;
    public const int MaximumRegisterRequestResponseSize = 4095;

    // supported extend codes
    public const byte Read = 0x55;
    public const byte Write = 0x66;
    public const byte ReadSuccess = 0xAA;
    public const byte WriteScueess = 0x88;
    public const byte WriteFailed = 0x99;

    // modbus slave exception offset that is added to the function code, to flag an exception
    public const byte ExceptionOffset = 128;

    // modbus slave exception codes
    public const byte IllegalFunction = 1;
    public const byte IllegalDataAddress = 2;
    public const byte Acknowledge = 5;
    public const byte SlaveDeviceBusy = 6;

    // default setting for number of retries for IO operations
    public const int DefaultRetries = 3;

    // default number of milliseconds to wait after encountering an ACKNOWLEGE or SLAVE DEVIC BUSY slave exception response.
    public const int DefaultWaitToRetryMilliseconds = 250;

    // default setting for IO timeouts in milliseconds
    public const int DefaultTimeout = 1000;

    // used by the ASCII tranport to indicate end of message
    // public const string NewLine = ")>";
    public const string NewLine = "293E";
}

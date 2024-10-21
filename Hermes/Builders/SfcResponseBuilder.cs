using System;
using System.Threading.Tasks;
using Hermes.Models;
using Hermes.Types;

namespace Hermes.Builders;

public class SfcResponseBuilder
{
    private const string PassContent = "OK\n{UuTSerialNumber}";
    private const string FailContent = "FAIL";
    private const string WrongStation = "GO-ICT\n{UuTSerialNumber}";
    private const string UnknownContent = "UNKNOWN";
    private const string TimeoutContent = "Timeout";

    private string _serialNumber = "1A26TEST";

    private string Content { get; set; } = UnknownContent;

    public SfcResponse Build()
    {
        return new SfcResponse(this.GetContent());
    }

    public string GetContent()
    {
        return this.Content.Replace("{UuTSerialNumber}", _serialNumber);
    }

    public SfcResponse BuildTimeout()
    {
        return SfcResponse.BuildTimeout();
    }

    public SfcResponseBuilder SetOkSfcResponse()
    {
        this.Content = PassContent;
        return this;
    }

    public SfcResponseBuilder SetFailContent()
    {
        this.Content = FailContent;
        return this;
    }

    public SfcResponseBuilder SetWrongStation()
    {
        this.Content = WrongStation;
        return this;
    }
    
    public SfcResponseBuilder SetScanError()
    {
        this.Content = SfcResponse.ScanError;
        return this;
    }

    public SfcResponseBuilder SetFailContent(string message)
    {
        if (string.IsNullOrEmpty(message)) return this;
        this.Content = FailContent + " - " + message;
        return this;
    }

    public SfcResponseBuilder SetUnknownContent()
    {
        this.Content = UnknownContent;
        return this;
    }

    public SfcResponseBuilder SetTimeoutContent()
    {
        this.Content = SfcResponse.TimeoutText;
        return this;
    }

    public SfcResponseBuilder SerialNumber(string serialNumber)
    {
        _serialNumber = serialNumber;
        return this;
    }

    public SfcResponseBuilder Clone()
    {
        return new SfcResponseBuilder();
    }
}
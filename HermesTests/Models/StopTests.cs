using Hermes.Builders;
using Hermes.Models;
using Hermes.Types;

namespace HermesTests.Models;

public class StopTests
{
    private readonly SfcResponseBuilder _sfcResponseBuilder;

    public StopTests(SfcResponseBuilder sfcResponseBuilder)
    {
        this._sfcResponseBuilder = sfcResponseBuilder;
    }

    [Fact]
    public void IsMachineStop_TypeIsMachine_ReturnsTrue()
    {
        var stop = new Stop
        {
            Type = StopType.Machine
        };
        Assert.True(stop.IsMachineStop);
    }

    [Fact]
    public void IsMachineStop_TypeNotMachine_ReturnsFalse()
    {
        var stop = new Stop
        {
            Type = StopType.Line
        };
        Assert.False(stop.IsMachineStop);
    }

    [Fact]
    public void SerialNumber_ValidSfcResponse_ReturnsNotNullString()
    {
        var sfcResponse = this._sfcResponseBuilder.Build();
        var stop = new Stop(StopType.Machine, sfcResponse);
        Assert.False(string.IsNullOrWhiteSpace(stop.SerialNumber));
    }

    [Fact]
    public void Message_FailSfcResponse_ReturnsSfcResponseDetails()
    {
        var sfcResponse = this._sfcResponseBuilder
            .SetFailContent()
            .Build();
        var stop = new Stop(StopType.Machine, sfcResponse);
        Assert.Equal(sfcResponse.Content, stop.Message);
    }

    [Fact]
    public void Message_SuccessSfcResponse_ReturnsStopType()
    {
        var sfcResponse = this._sfcResponseBuilder
            .SetOkContent()
            .Build();
        var stopType = StopType.Machine;
        var stop = new Stop(stopType, sfcResponse);
        Assert.Contains(stopType.ToString().ToUpper(), stop.Message);
    }

    [Fact]
    public void Details_SuccessSfcResponse_ReturnsSfcResponseDetails()
    {
        var sfcResponse = this._sfcResponseBuilder
            .SetOkContent()
            .Build();
        var stop = new Stop(StopType.Machine, sfcResponse);
        Assert.Equal(sfcResponse.Details, stop.Details);
    }

    [Fact]
    public void Details_SfcResponseNull_ReturnsEmptyString()
    {
        var stop = new Stop(StopType.Machine, SfcResponse.Null);
        Assert.True(string.IsNullOrEmpty(stop.Details));
    }
}
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
        var serialNumber = "1234";
        var stop = new Stop(StopType.Machine) { SerialNumber = serialNumber };
        Assert.Equal(serialNumber, stop.SerialNumber);
    }

    [Fact]
    public void Message_SuccessSfcResponse_ReturnsStopType()
    {
        var stopType = StopType.Machine;
        var stop = new Stop(stopType);
        Assert.Contains(stopType.ToString(), stop.Message, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void Details_Called_ReturnsDetails()
    {
        var details = "details";
        var stop = new Stop(StopType.Machine) { Details = details };
        Assert.Equal(details, stop.Details);
    }
}
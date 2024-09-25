using Hermes.Common.Parsers;

namespace HermesTests.Common.Parsers;

public class PackageParserTests
{
    private readonly PackageParser _sut;
    private const string HhPartNumber = "0101L6100-000-G-DT";
    private const string Quantity = "10";
    private const string SupplierPartNumber = "0101L6100-000-G-DT";
    private const string DateCode = "0824";
    private const string Lot = "D1108";
    private const string Pkgid = "VVG0010028CN240220199D1108";
    private const string Vendor = "DTI";

    private const string ScannedCode =
        $"P{HhPartNumber},Q{Quantity},M{SupplierPartNumber},D{DateCode},L{Lot},S{Pkgid},{Vendor}";

    private const string AlternativeScannedCode =
        $"L{Lot},P{HhPartNumber},Q{Quantity},M{SupplierPartNumber},D{DateCode},S{Pkgid},{Vendor}";

    private const string WithoutLotScannedCode =
        $"P{HhPartNumber},Q{Quantity},M{SupplierPartNumber},D{DateCode},S{Pkgid},{Vendor}";

    public PackageParserTests()
    {
        this._sut = new PackageParser();
    }

    [Fact]
    public void Parse_ValidCode_ParsesPackage()
    {
        var result = this._sut.Parse(ScannedCode);
        Assert.Equal(HhPartNumber, result.HhPartNumber);
        Assert.Equal(Quantity, result.Quantity.ToString());
        Assert.Equal(SupplierPartNumber, result.SupplierPartNumber);
        Assert.Equal(DateCode, result.DateCode);
        Assert.Equal(Lot, result.Lot);
        Assert.Equal(Pkgid, result.Id);
        Assert.Equal(Vendor, result.Vendor);
    }

    [Fact]
    public void Parse_AlternativeCode_DoesNotParsesVendor()
    {
        var result = this._sut.Parse(AlternativeScannedCode);
        Assert.Empty(result.Vendor);
    }

    [Fact]
    public void Parse_WithoutLotCode_DoesNotParsesLot()
    {
        var result = this._sut.Parse(WithoutLotScannedCode);
        Assert.Empty(result.Lot);
    }
}
using System.Text.RegularExpressions;
using Hermes.Models;

namespace Hermes.Common.Parsers;

public class PackageParser
{
    private const string RegexBase = @"{1}([\w-\._]+),";
    private static readonly Regex HhPartNumberRgx = new(@",P([\w-]+)");
    private static readonly Regex QuantityRgx = new(@",Q([\d]+)");
    private static readonly Regex SupplierPartNumberRgx = new(@"M([\w-]+)");
    private static readonly Regex DateCodeRgx = new(@",D([\d]+)");
    private static readonly Regex LotRgx = new(@",L([\w-]+)");
    private static readonly Regex PkgidRgx = new(@",S([\w-]+)");
    private static readonly Regex VendorRgx = new(@",{1}([\w-\._]+)$");

    private static readonly Regex WithVendorRgx =
        new($@"P{RegexBase}Q{RegexBase}M{RegexBase}D{RegexBase}L{RegexBase}S{RegexBase}[\w-\._]+$");

    public Package Parse(string code)
    {
        if (!code.StartsWith(","))
        {
            code = $",{code}";
        }

        var package = new Package
        {
            HhPartNumber = HhPartNumberRgx.Match(code).Groups[1].Value,
            Quantity = int.Parse(QuantityRgx.Match(code).Groups[1].Value),
            SupplierPartNumber = SupplierPartNumberRgx.Match(code).Groups[1].Value,
            DateCode = DateCodeRgx.Match(code).Groups[1].Value,
            Lot = LotRgx.Match(code).Groups[1].Value,
            Id = PkgidRgx.Match(code).Groups[1].Value
        };
        if (WithVendorRgx.IsMatch(code))
        {
            package.Vendor = VendorRgx.Match(code).Groups[1].Value;
        }

        return package;
    }
}
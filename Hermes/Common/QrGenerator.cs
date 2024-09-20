using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using QRCoder.Core;

namespace Hermes.Common;

public class QrGenerator
{
    public async Task<Bitmap> GenerateAvaloniaBitmap(string code, int width)
    {
        using MemoryStream stream = this.Generate(code);
        return await Task.Run(() => Bitmap.DecodeToWidth(stream, width));
    }

    public MemoryStream Generate(string code)
    {
        using QRCodeGenerator qrGenerator = new QRCodeGenerator();
        using QRCodeData qrCodeData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
        using PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
        return new MemoryStream(qrCode.GetGraphic(20));
    }
}
using Application.Common.Interfaces;
using QRCoder;

namespace Infrastructure.Services;

public class AttendanceQrCodeService : IAttendanceQrCodeService
{
    public byte[] GeneratePng(string content)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }
}

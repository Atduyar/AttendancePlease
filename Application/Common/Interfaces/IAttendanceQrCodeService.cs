namespace Application.Common.Interfaces;

public interface IAttendanceQrCodeService
{
    byte[] GeneratePng(string content);
}

namespace Application.Common;

public static class GpsHelper
{
    private const double EarthRadiusMeters = 6371000d;

    public static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var latitudeDelta = ToRadians(lat2 - lat1);
        var longitudeDelta = ToRadians(lon2 - lon1);
        var startLatitude = ToRadians(lat1);
        var endLatitude = ToRadians(lat2);

        var sinLatitude = Math.Sin(latitudeDelta / 2d);
        var sinLongitude = Math.Sin(longitudeDelta / 2d);

        var a = (sinLatitude * sinLatitude)
            + Math.Cos(startLatitude) * Math.Cos(endLatitude) * (sinLongitude * sinLongitude);
        var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));

        return EarthRadiusMeters * c;
    }

    private static double ToRadians(double degrees) => degrees * (Math.PI / 180d);
}

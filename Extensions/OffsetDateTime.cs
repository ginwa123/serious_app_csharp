namespace App.Extensions;


public static class DateTimeOffsetExtensions
{
    // Extension method to convert DateTimeOffset to a string
    public static string ToDefaultString(this DateTimeOffset dateTimeOffset, string format = "yyyy-MM-ddTHH:mm:sszzz")
    {
        // Return the formatted string representation of the DateTimeOffset
        return dateTimeOffset.ToString(format);
    }
}
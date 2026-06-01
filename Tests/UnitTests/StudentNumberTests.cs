using Application.Features.Enrollments;

namespace UnitTests;

public class StudentNumberTests
{
    [Fact]
    public void Normalize_TrimsSurroundingWhitespace()
    {
        Assert.Equal("12345", StudentNumber.Normalize("  12345  "));
    }

    [Theory]
    [InlineData("190304@student.ius.edu.ba", "190304")]
    [InlineData("  190304@STUDENT.IUS.EDU.BA  ", "190304")]
    public void FromStudentEmail_ReturnsDigits_ForValidStudentEmail(string email, string expected)
    {
        Assert.Equal(expected, StudentNumber.FromStudentEmail(email));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromStudentEmail_ReturnsNull_ForMissingEmail(string? email)
    {
        Assert.Null(StudentNumber.FromStudentEmail(email));
    }

    [Theory]
    [InlineData("john.doe@ius.edu.ba")]              // wrong domain
    [InlineData("staff@gmail.com")]                  // unrelated domain
    [InlineData("abc123@student.ius.edu.ba")]        // local part not all digits
    [InlineData("@student.ius.edu.ba")]              // empty local part
    public void FromStudentEmail_ReturnsNull_ForNonStudentOrNonNumericLocalPart(string email)
    {
        Assert.Null(StudentNumber.FromStudentEmail(email));
    }
}

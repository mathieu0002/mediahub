using FluentAssertions;
using FluentValidation.TestHelper;
using MediaHub.Application.DTOs.Requests;
using MediaHub.Application.Validators;

namespace MediaHub.UnitTests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void ValidRequest_HasNoErrors()
    {
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Username = "validuser",
            Password = "Test1234"
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    public void InvalidEmail_HasError(string email)
    {
        var request = new RegisterRequest
        {
            Email = email,
            Username = "user",
            Password = "Test1234"
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(r => r.Email);
    }

    [Theory]
    [InlineData("ab")]                  // trop court
    [InlineData("user with spaces")]    // espace
    [InlineData("user!chars")]          // caractère interdit
    public void InvalidUsername_HasError(string username)
    {
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Username = username,
            Password = "Test1234"
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(r => r.Username);
    }

    [Theory]
    [InlineData("short")]           // trop court
    [InlineData("alllowercase1")]   // pas de majuscule
    [InlineData("ALLUPPERCASE1")]   // pas de minuscule
    [InlineData("NoDigitsHere")]    // pas de chiffre
    public void WeakPassword_HasError(string password)
    {
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Username = "user",
            Password = password
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(r => r.Password);
    }
}
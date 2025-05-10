using Modules.Users.Application.Authentication.Register;

namespace Application.UnitTests.Users.TestData;

public sealed class RegisterUserCommandInvalidData : TheoryData<RegisterUserCommand>
{
    public RegisterUserCommandInvalidData()
    {
        // All empty
        Add(new RegisterUserCommand(string.Empty, string.Empty, string.Empty));

        // Empty email
        Add(new RegisterUserCommand(string.Empty, "validUsername", "ValidPassword123!"));

        // Empty username
        Add(new RegisterUserCommand("user@example.com", string.Empty, "ValidPassword123!"));

        // Empty password
        Add(new RegisterUserCommand("user@example.com", "validUsername", string.Empty));

        // Invalid email format
        Add(new RegisterUserCommand("invalid-email", "validUsername", "ValidPassword123!"));
        Add(new RegisterUserCommand("user@.com", "validUsername", "ValidPassword123!"));
        Add(new RegisterUserCommand("userexample.com", "validUsername", "ValidPassword123!"));

        // Whitespace only
        Add(new RegisterUserCommand("   ", "validUsername", "ValidPassword123!"));
        Add(new RegisterUserCommand("user@example.com", "   ", "ValidPassword123!"));
        Add(new RegisterUserCommand("user@example.com", "validUsername", "   "));

        // Short password
        Add(new RegisterUserCommand("user@example.com", "validUsername", "123"));

        // Very long username
        Add(new RegisterUserCommand("user@example.com", new string('a', 300), "ValidPassword123!"));

        // Password without required characters (assuming policy requires uppercase/lowercase/digits/symbols)
        Add(new RegisterUserCommand("user@example.com", "validUsername", "alllowercase"));
        Add(new RegisterUserCommand("user@example.com", "validUsername", "ALLUPPERCASE"));
        Add(new RegisterUserCommand("user@example.com", "validUsername", "12345678"));
        Add(new RegisterUserCommand("user@example.com", "validUsername", "NoSymbols123"));
    }
}

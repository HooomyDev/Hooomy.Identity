namespace Hooome.Identity.Services;

public class PassGenService : IPassGenService
{
    public string Generate(int length = 12)
    {
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string special = "!@#$%^&*()_-+=<>?";

        var random = new Random();
        var password = new char[length];

        password[0] = lowercase[random.Next(lowercase.Length)];
        password[1] = uppercase[random.Next(uppercase.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = special[random.Next(special.Length)];

        var allChars = lowercase + uppercase + digits + special;
        for (int i = 4; i < length; i++)
        {
            password[i] = allChars[random.Next(allChars.Length)];
        }

        return new string(password.OrderBy(x => random.Next()).ToArray());
    }
}

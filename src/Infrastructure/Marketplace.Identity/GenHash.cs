using System;
using Marketplace.Identity.Services;

class Program
{
    static void Main()
    {
        var hasher = new Argon2idPasswordHasher();
        var hash = "$argon2id$v=19$m=65536,t=3,p=1$t3spA9wh4NUB1wk5kT9ejw$WIU+dzsDyvQ2XZcKoeWI3KMXvsMTCfQtZ1DrlWd8P4w";
        Console.WriteLine("Is Valid: " + hasher.VerifyPassword("Admin@123456", hash));
    }
}

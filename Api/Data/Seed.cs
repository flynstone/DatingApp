using Api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Api.Data
{
    public class Seed
    {
        public static async Task SeedUsers(AppDbContext context)
        {
            // Do nothing if there is already data in the database.
            if (await context.Users.AnyAsync()) return;

            // Store the returned json file inside variable.
            var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");

            // Convert the json data file and store it inside varible.
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            // Loop through the list of users, create a generic password for all users.
            // This is only for testing purposes.
            foreach(var user in users)
            {
                using var hmac = new HMACSHA512();

                user.UserName = user.UserName.ToLower();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
                user.PasswordSalt = hmac.Key;

                context.Users.Add(user);
            }

            await context.SaveChangesAsync();
        }
    }
}

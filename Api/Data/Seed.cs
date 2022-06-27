using Api.Entities;

namespace Api.Data
{
    public class Seed
    {
        public static async Task SeedUserData(AppDbContext context)
        {
            // If no users exist in the database, create some.
            if (!context.Users.Any())
            {
                var users = new List<AppUser>
                {
                    // Create new user.
                    new AppUser
                    {
                        Id = 1,
                        UserName = "Bob"
                    },
                    new AppUser
                    {
                        Id = 2,
                        UserName = "Tom"
                    },
                    new AppUser
                    {
                        Id = 3,
                        UserName = "Jane"
                    },
                };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }
        }
    }
}

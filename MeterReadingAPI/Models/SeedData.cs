using Microsoft.EntityFrameworkCore;

namespace MeterReadingAPI.Models
{

    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                if (context.Accounts.Any())
                {
                    return; // DB has been seeded
                }

                var accounts = File.ReadAllLines("path_to_Test_accounts.csv")
                    .Skip(1)
                    .Select(line => line.Split(','))
                    .Select(columns => new Account
                    {
                        AccountId = int.Parse(columns[0]),
                        FirstName = columns[1],
                        LastName = columns[2]
                    }).ToArray();

                context.Accounts.AddRange(accounts);
                context.SaveChanges();
            }
        }
    }


}

using Poliak_UI_WT.Domain.Entities;

namespace Poliak_UI_WT.API.Data
{
    public class DbInitializer
    {

        static readonly UriBuilder BasePhoneUri = new("https", "localhost", 7002, "Images\\MemoryPhones");

        public static async Task SeedData(WebApplication app)
        {
            var getPhoneImageUri = (string imageName) =>
                new Uri(Path.Combine(BasePhoneUri.ToString(), imageName)).ToString();

            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!context.Categories.Any() && !context.Phones.Any())
            {
                var iosCat = new Category() { Name = "iOS", NormalizedName = "ios" };
                var androidCat = new Category() { Name = "Android", NormalizedName = "android" };

                Phone s24Black = new()
                {
                    PhoneId = 1,
                    Name = "Samsung",
                    Model = "S24 Black",
                    Price = 3200,
                    Image = getPhoneImageUri("s24black.jpg"),
                    Category = androidCat
                };

                Phone s24Gold = new()
                {
                    PhoneId = 2,
                    Name = "Samsung",
                    Model = "S24 Gold",
                    Price = 3100,
                    Image = getPhoneImageUri("s24gold.jpg"),
                    Category = androidCat
                };

                Phone s24ultrablack = new()
                {
                    PhoneId = 3,
                    Name = "Samsung",
                    Model = "S24 Ultra Black",
                    Price = 4300,
                    Image = getPhoneImageUri("s24ultrablack.jpg"),
                    Category = androidCat
                };

                Phone s24ultragold = new()
                {
                    PhoneId = 4,
                    Name = "Samsung",
                    Model = "S24 Ultra Gold",
                    Price = 3200,
                    Image = getPhoneImageUri("s24ultragold.jpg"),
                    Category = androidCat
                };

                Phone ip15problack = new()
                {
                    PhoneId = 5,
                    Name = "iPhone",
                    Model = "15 pro black",
                    Price = 2900,
                    Image = getPhoneImageUri("iphone15problack.jpg"),
                    Category = iosCat
                };

                Phone ip15prodeserttitan = new()
                {
                    PhoneId = 6,
                    Name = "iPhone",
                    Model = "15 pro desert titanium",
                    Price = 3100,
                    Image = getPhoneImageUri("iphone15prodeserttitan.jpg"),
                    Category = iosCat
                };

                Phone ip15prorose = new()
                {
                    PhoneId = 7,
                    Name = "iPhone",
                    Model = "15 pro rose",
                    Price = 3100,
                    Image = getPhoneImageUri("iphone15prorose.jpg"),
                    Category = iosCat
                };
                Phone ip15protitan = new()
                {
                    PhoneId = 8,
                    Name = "iPhone",
                    Model = "15 pro titanium",
                    Price = 3200,
                    Image = getPhoneImageUri("iphone15protitan.jpg"),
                    Category = iosCat
                };

                List<Phone> newPhones = new() {s24Black, s24Gold, s24ultrablack, s24ultragold, ip15problack,
                ip15prodeserttitan, ip15prorose, ip15protitan };

                await context.AddRangeAsync(newPhones);
                await context.SaveChangesAsync();
            };

        }

    }

}

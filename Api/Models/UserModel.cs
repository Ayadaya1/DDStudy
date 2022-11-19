namespace Api.Models
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTimeOffset BirthDate { get; set; }

        public int subscriberCount { get; set; }
        public int subscriptionCount { get; set; }

    }
}

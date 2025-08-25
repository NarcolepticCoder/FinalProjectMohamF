namespace Data.Entities
{
    public class Users
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ExternalId { get; set; } = default!;
        public string Email { get; set; } = default!;

        public Guid RoleId { get; set; }
        public Roles Role { get; set; } = default!;

        public ICollection<SecurityEvents> AuthoredEvents { get; set; } = new List<SecurityEvents>();
        public ICollection<SecurityEvents> AffectedEvents { get; set; } = new List<SecurityEvents>();
    }
}
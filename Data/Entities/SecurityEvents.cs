namespace Data.Entities
{
    public class SecurityEvents
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string EventType { get; set; } = default!; // LoginSuccess, Logout, RoleAssigned
        public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }

        public Guid AuthorUserId { get; set; }
        public User AuthorUser { get; set; } = default!;

        public Guid AffectedUserId { get; set; }
        public User AffectedUser { get; set; } = default!;
    }
}
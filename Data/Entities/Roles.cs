using Data.Entities;

public class Roles
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public string? Description { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<RoleClaims> RoleClaim { get; set; } = new List<RoleClaims>();
    }
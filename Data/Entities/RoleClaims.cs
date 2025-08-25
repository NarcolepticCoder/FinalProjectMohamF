using Data.Entities;

public class RoleClaims
    {
        public Guid RoleId { get; set; }
        public Roles Role { get; set; } = default!;

        public Guid ClaimId { get; set; }
        public Claims Claim { get; set; } = default!;
    }
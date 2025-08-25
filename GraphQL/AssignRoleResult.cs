namespace GraphQL
{
    public class AssignRoleResult  // DTO for GraphQL output
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public string FromRole { get; set; } = string.Empty;
        public string ToRole { get; set; } = string.Empty;
    }


}
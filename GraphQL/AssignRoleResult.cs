using Data.Entities;

namespace GraphQL
{
    public class AssignRoleResult  // DTO for GraphQL output
    {
        
        public User AuthorUser { get; set; } = default!;
        public User AffectedUser { get; set; } = default!;
        public string FromRole { get; set; } = string.Empty;
        public string ToRole { get; set; } = string.Empty;
    }


}
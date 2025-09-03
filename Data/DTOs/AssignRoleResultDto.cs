using Data.Entities;

namespace Data.Dtos
{
    public class AssignRoleResultDto  // DTO for GraphQL output
    {
        
        public User AuthorUser { get; set; } = default!;
        public User AffectedUser { get; set; } = default!;
        public string FromRole { get; set; } = string.Empty;
        public string ToRole { get; set; } = string.Empty;
    }


}
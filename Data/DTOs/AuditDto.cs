namespace Data.Dtos
{
    public class AuditDto
    {
        public string Email { get; set; } = null!;
        public string ExternalId { get; set; } = null!;
        public string? Provider { get; set; }


    }
}
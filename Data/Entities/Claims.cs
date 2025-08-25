namespace Data.Entities
{
    public class Claims
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Type { get; set; } = default!;
        public string Value { get; set; } = default!;

        public ICollection<RoleClaims> RoleClaim { get; set; } = new List<RoleClaims>();
    }
}
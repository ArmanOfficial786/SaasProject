

namespace UserManagement.Domain.Entities
{
    public class Tenant
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Subdomain { get; set; }
        public string? ProductCode { get; set; }  // "HRM", "SCHOOL"
        public string Status { get; set; } = "Active";
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
    }
}

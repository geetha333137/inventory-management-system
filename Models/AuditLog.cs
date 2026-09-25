using System.ComponentModel.DataAnnotations;

namespace InvertoryManagementSystem.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(80)]
        public string EntityName { get; set; } = string.Empty;

        public int? EntityId { get; set; }

        [StringLength(150)]
        public string? PerformedBy { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}

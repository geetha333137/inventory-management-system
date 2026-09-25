using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace InvertoryManagementSystem.Models
{
    public class Asset
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Asset code is required.")]
        [Display(Name = "Asset Code")]
        [StringLength(50)]
        public string AssetCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Asset name is required.")]
        [Display(Name = "Asset Name")]
        [StringLength(150)]
        public string AssetName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [ValidateNever]
        public Category? Category { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Serial Number")]
        [StringLength(100)]
        public string? SerialNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Purchase Date")]
        public DateTime? PurchaseDate { get; set; }

        [Range(0, 999999999, ErrorMessage = "Purchase cost must be a valid amount.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Purchase Cost")]
        public decimal? PurchaseCost { get; set; }

        [StringLength(150)]
        public string? Location { get; set; }

        [StringLength(150)]
        public string? Department { get; set; }

        [Display(Name = "Assigned To")]
        [StringLength(150)]
        public string? AssignedTo { get; set; }

        public int? AssignedToUserId { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(50)]
        public string Status { get; set; } = AssetStatuses.Available;

        public int? CreatedBy { get; set; }

        [Display(Name = "Created By")]
        [StringLength(150)]
        public string? CreatedByName { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        public int? UpdatedBy { get; set; }

        [Display(Name = "Last Modified By")]
        [StringLength(150)]
        public string? UpdatedByName { get; set; }

        [Display(Name = "Last Modified Date")]
        public DateTime? UpdatedDate { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace OrganisationProfile.OrganisationProfile.Models
{
    public class Organisation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Organisation Name")]
        public string OrganisationName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Organisation Code")]
        public string OrganisationCode { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Url]
        [StringLength(250)]
        public string? Website { get; set; }

        [StringLength(300)]
        public string? LogoPath { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
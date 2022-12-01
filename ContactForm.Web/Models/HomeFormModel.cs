using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebTestPlaywright.Models
{
    public class HomeFormModel
    {
        [Required]
        [StringLength(100,MinimumLength = 3)]
        [DisplayName("First name")]
        public string? FirstName { get; set; }

        [Required]
        [DisplayName("Last name")]
        [StringLength(100, MinimumLength = 3)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        [DisplayName("Email address")]
        public string? EmailAddress { get; set; }

        [Required]
        [DisplayName("Birth date")]
        public DateTime? BirthDate { get; set; }
    }
}

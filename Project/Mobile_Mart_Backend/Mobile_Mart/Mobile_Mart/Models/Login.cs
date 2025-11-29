using System.ComponentModel.DataAnnotations;

namespace Mobile_Mart.Models
{
    public class Login
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace AkademikTakvimWebSitesi.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email alanı gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi girin.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı gereklidir.")]
        public string Sifre { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;

namespace VERA.Application.DTOs.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Ad zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad 2-100 karakter arasında olmalıdır.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Soyad 2-100 karakter arasında olmalıdır.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(30)]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
    public string Password { get; set; } = string.Empty;
}
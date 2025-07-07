using System;
using System.ComponentModel.DataAnnotations;
public class LoginDto
{
    [Required]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
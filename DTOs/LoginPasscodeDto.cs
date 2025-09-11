using System;
using System.ComponentModel.DataAnnotations;
public class LoginPasscodeDto
{
    [Required]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    public string Passcode { get; set; } = null!;
}
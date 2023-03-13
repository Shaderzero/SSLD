using System.ComponentModel.DataAnnotations;

namespace SSLD.Tools;

public class ChangePasswordUser
{
    [Required(ErrorMessage = "Old password is required")]
    [DataType(DataType.Password)]
    public string OldPassword { get; set; }

    [Required(ErrorMessage = "New password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "New password is required")]
    [DataType(DataType.Password)]
    public string RepeatPassword { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace Sebezhko.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Не указан логин")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Не указан логин")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using SWork.Data.DTO.AuthDTO;

namespace SWork.Data.DTO.AuthDTO
{
    public class LoginResponseDTO
    {
        public UserDTO? User { get; set; }


        public IEnumerable<string> Role { get; set; } = new List<string>();
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;

    }
}

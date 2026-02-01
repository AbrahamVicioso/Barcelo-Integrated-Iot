using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usuarios.ExternalService.Utils
{
    public class AuthenticationApiOptions
    {
        public const string SectionName = "ExternalServices:Authentication";
        public string BaseUrl { get; set; } = default!;
    }
}

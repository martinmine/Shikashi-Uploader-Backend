using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShikashiAPI.ViewModels
{
    public class APIKeyViewModel
    {
        public string Key { get; set; }

        public long ExpirationTime { get; set; }
    }
}

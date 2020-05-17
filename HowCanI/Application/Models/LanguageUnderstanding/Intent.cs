using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HowCanI.Application.Models.LanguageUnderstanding
{
    public class Intent
    {
        public string Name { get; set; }

        public List<string> Properties { get; set; }
    }
}

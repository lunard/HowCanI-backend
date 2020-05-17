using HowCanI.Application.Models.LanguageUnderstanding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HowCanI.Application.Services.Interfaces
{
    public interface ILanguageUnderstanding
    {
        Task<Intent> GetIntent(string sentence);
    }
}

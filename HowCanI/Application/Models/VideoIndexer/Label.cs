using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HowCanI.Application.Models.VideoIndexer
{
    public class Label
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public List<LabelAppearance> Appearances { get; set; }
    }

    public  class LabelAppearance
    {
        public decimal StartSeconds { get; set; }
        public decimal EndSeconds { get; set; }
    }
}

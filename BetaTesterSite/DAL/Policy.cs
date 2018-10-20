using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetaTesterSite.DAL
{
    public class Policy
    {
        public int PolicyId { get; set; }
        public string Name { get; set; }
        public string NiceName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string ExternalCode { get; set; }
    }
}

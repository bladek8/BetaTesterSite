using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetaTesterSite.DAL
{
    public class PhaseRate
    {
        public int PhaseRateId { get; set; }
        public int PhaseId { get; set; }
        public int UserId { get; set; }
        public int Rate { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetaTesterSite.DAL
{
    public class PolicyRole
    {
        public int PolicyRoleId { get; set; }
        public int PolicyId { get; set; }
        public int RoleId { get; set; }
    }
}

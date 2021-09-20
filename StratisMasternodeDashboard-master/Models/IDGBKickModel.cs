using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stratis.FederatedSidechains.AdminDashboard.Models
{
    public class IDGBKickModel
    {
        public bool IsValidKey { get; set; }
        public bool IsResponseSuccess { get; set; }
        public string Message { get; set; }
       
    }
}

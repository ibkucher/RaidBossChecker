using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidBossChecker
{
    public class RaidBoss
    {
        public string Name { get; set; }
        public DateTime TimeKilled { get; set; }
       
        public DateTime RespawnStart { get; set; }
        public DateTime RespawnEnd { get; set; }
    }
}

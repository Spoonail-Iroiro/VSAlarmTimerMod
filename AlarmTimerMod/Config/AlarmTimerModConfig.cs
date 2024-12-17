using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmTimerMod.Config {
    record class AlarmTimerModConfig {
        public string rootCommandName { get; set; } = "tm";
        public double masterVolume { get; set; } = 1;
    }
}

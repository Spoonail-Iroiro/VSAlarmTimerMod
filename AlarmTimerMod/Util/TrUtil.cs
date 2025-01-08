using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Config;

namespace AlarmTimerMod.Util {
    internal class TrUtil {
        public static string Tr(string translationKey) {
            return Lang.Get("alarmtimermod:" + translationKey);
        }
    }
}

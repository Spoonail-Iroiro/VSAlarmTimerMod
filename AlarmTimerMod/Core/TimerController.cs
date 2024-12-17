using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace AlarmTimerMod.Core {
    internal class TimerController : TimerControllerBase {
        ICoreClientAPI capi;

        public TimerController(ICoreClientAPI capi) : base() {
            this.capi = capi;
        }

        protected override void RegisterTimerToSystem(int seconds, Action actionOnTime) {
            capi.Event.RegisterCallback((dt) => {
                actionOnTime();
            }, seconds * 1000);
        }
        protected override void NotifyToSystem(TimerReservation reservation) {
            capi.ShowChatMessage($"<strong>[Alarm] {reservation.message}</strong>");

            var modSystem = capi.ModLoader.GetModSystem<AlarmTimerModSystem>();
            if (modSystem != null) {
                capi.World.PlaySoundFor(
                    new AssetLocation("alarmtimermod:sounds/maou_se_jingle04"),
                    capi.World.Player,
                    randomizePitch: false,
                    volume: (float)modSystem.Config.masterVolume
                );
            }

        }
    }
}

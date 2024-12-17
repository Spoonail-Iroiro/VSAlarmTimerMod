using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmTimerMod.Core {
    record class TimerReservation(string id, int seconds, string message, bool isValid = true);

    abstract class TimerControllerBase {
        public TimerPresetAccessor PresetAccessor { get; set; }

        public Dictionary<string, TimerReservation> CurrentTimerReservations { get; private set; } = new();

        public LimitedStack<string> AddedTimerIdHistory { get; set; } = new(5);

        public void StartTimer(int seconds, string message) {
            var id = Guid.NewGuid().ToString();
            CurrentTimerReservations[id] = new TimerReservation(id, seconds, message);
            AddedTimerIdHistory.Push(id);
            RegisterTimerToSystem(seconds, () => OnTime(id));
        }

        public void StartTimer(string presetName) {
            if (PresetAccessor == null) throw new InvalidOperationException("No timer presets loaded");
            var preset = PresetAccessor.TimerPresetData.TimerPresetEntries.GetValueOrDefault(presetName);
            if (preset == null) throw new ArgumentException($"Preset '{presetName}' not found");

            StartTimer(preset.Seconds, preset.Message);
        }

        void OnTime(string id) {
            var reservation = CurrentTimerReservations.GetValueOrDefault(id);
            if (reservation == null || !reservation.isValid) {
                CurrentTimerReservations.Remove(id);
                return;
            }

            NotifyToSystem(reservation);
            CurrentTimerReservations.Remove(id);
        }

        abstract protected void RegisterTimerToSystem(int seconds, Action actionOnTime);

        abstract protected void NotifyToSystem(TimerReservation reservation);

        public TimerReservation CancelTimer(string id) {
            var reservation = CurrentTimerReservations.GetValueOrDefault(id);
            var isRemoved = CurrentTimerReservations.Remove(id);

            return isRemoved ? reservation : null;
        }

        public TimerReservation CancelLastTimer() {
            var lastId = AddedTimerIdHistory.PopOrNull();
            if (lastId == null) return null;

            return CancelTimer(lastId);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace AlarmTimerMod.Core {

    public record class TimerPresetEntry(
        int Seconds,
        string Message
    );

    record class TimerPresetData(
        Dictionary<string, TimerPresetEntry> TimerPresetEntries
    ) {
        public TimerPresetData() : this(new Dictionary<string, TimerPresetEntry>()) {
        }
    }

    internal class TimerPresetAccessor {
        public TimerPresetData TimerPresetData { get; set; }

        public TimerPresetAccessor() { }

        public TimerPresetAccessor(TimerPresetData data) {
            TimerPresetData = data;
        }

        public TimerPresetEntry GetOrNull(string name) {
            if (TimerPresetData == null) throw new Exception("Operation called before loading timer preset data");
            var result = TimerPresetData.TimerPresetEntries.GetValueOrDefault(name);

            return result;
        }

        public void AddAndUpdate(string name, TimerPresetEntry entry) {
            if (TimerPresetData == null) throw new Exception("Operation called before loading timer preset data");
            TimerPresetData.TimerPresetEntries[name] = entry;
        }

        public bool Remove(string name) {
            if (TimerPresetData == null) throw new Exception("Operation called before loading timer preset data");
            return TimerPresetData.TimerPresetEntries.Remove(name);
        }

        public string GetListDescription() {
            var sb = new StringBuilder();
            sb.AppendLine("Name, Duration, Message");
            var listEntries = TimerPresetData.TimerPresetEntries
                .AsEnumerable()
                .OrderBy(kp => kp.Key)
                .Select(kp => $"[{kp.Key}], {kp.Value.Seconds}sec, \"{kp.Value.Message}\"");
            listEntries.Foreach(line => sb.AppendLine(line));

            return sb.ToString();
        }

        public void LoadFrom(AlarmTimerModData modData) {
            TimerPresetData = modData.TimerPresetData;
        }

        public void SaveTo(AlarmTimerModData modData) {
            modData.TimerPresetData = TimerPresetData;
        }
    }
}

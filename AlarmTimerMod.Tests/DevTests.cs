using AlarmTimerMod.Core;
using FluentAssertions;
using Vintagestory.API.Common;

namespace AlarmTimerMod.Tests {
    public record class MockTimerPresetEntry(
        int Seconds,
        string Message,
        double? added
    );
    public record class MockTimerPresetData(
        Dictionary<string, MockTimerPresetEntry> TimerPresetEntries
    ) {
        public MockTimerPresetData() : this([]) {
        }
    };

    public class MockModData {
        public MockTimerPresetData TimerPresetData { get; set; } = new MockTimerPresetData();
    }

    [TestClass]
    public sealed class DevTests {
        [TestMethod()]
        public void SerializeTest() {
            var modData = new AlarmTimerModData();

            var presetAcc = new TimerPresetAccessor();
            presetAcc.LoadFrom(modData);

            presetAcc.TimerPresetData.TimerPresetEntries["1"] = new(10, "Hoge");

            var serialized = JsonUtil.ToString(modData);

        }

        [TestMethod()]
        public void DeSerializeOldTest() {
            var oldSerialized = """{"TimerPresetData":{"TimerPresetEntries":{"1":{"Seconds":10,"Message":"Hoge"}}}}""";

            var deserialized = JsonUtil.FromString<MockModData>(oldSerialized);
        }

        [TestMethod]
        public void LimitedStackTest() {
            var stack = new LimitedStack<MockTimerPresetEntry>(3);

            stack.Push(new MockTimerPresetEntry(3, "1", 1.0));
            stack.Push(new MockTimerPresetEntry(4, "2", 1.0));
            stack.Push(new MockTimerPresetEntry(5, "3", 1.0));

            stack.Count.Should().Be(3);

            stack.Push(new MockTimerPresetEntry(6, "4", 1.0));

        }
    }
}

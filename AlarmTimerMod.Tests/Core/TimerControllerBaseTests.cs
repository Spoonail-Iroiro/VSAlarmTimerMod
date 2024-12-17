using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlarmTimerMod.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using FluentAssertions;
using System.Security.Cryptography.X509Certificates;

namespace AlarmTimerMod.Core.Tests {
    record class CallbackRecord(int second, Action action);

    // Stub system with callback registration
    class StubSystem {
        List<CallbackRecord> callbacks = new List<CallbackRecord>();

        int currentTime = 0;

        public string LastNotified { get; set; }

        public void SetTime(int second) {
            currentTime = second;
            foreach (var rec in callbacks.Where(rec => rec.second <= currentTime)) {
                rec.action();
            }

            callbacks.RemoveAll(rec => rec.second <= currentTime);
        }

        public void RegisterCallback(int afterSeconds, Action action) {
            callbacks.Add(new CallbackRecord(currentTime + afterSeconds, action));
        }

        public void ShowMessage(string message) {
            Console.WriteLine(message);
            LastNotified = message;
        }
    }

    class TestTimerController : TimerControllerBase {
        StubSystem api;

        public TestTimerController(StubSystem api) : base() {
            this.api = api;
        }

        protected override void RegisterTimerToSystem(int seconds, Action actionOnTime) {
            api.RegisterCallback(seconds, actionOnTime);
        }
        protected override void NotifyToSystem(TimerReservation reservation) {
            api.ShowMessage(reservation.message);
        }

    }

    [TestClass()]
    public class TimerControllerBaseTests {
        StubSystem api = new StubSystem();

        [TestMethod()]
        public void StartTimerTest() {
            var timer = new TestTimerController(api);

            timer.StartTimer(20, "Test");

            api.LastNotified.Should().BeNull();

            timer.CurrentTimerReservations.Should().HaveCount(1);

            api.SetTime(20);

            api.LastNotified.Should().Be("Test");

            timer.CurrentTimerReservations.Should().HaveCount(0);
        }

        [TestMethod()]
        public void StartTimerTest1() {
            var presetData = new TimerPresetData(new Dictionary<string, TimerPresetEntry> {
                ["1"] = new TimerPresetEntry(20, "Test1"),
                ["abc"] = new TimerPresetEntry(10, "Hoge")
            });
            var presetAcc = new TimerPresetAccessor(presetData);
            var timer = new TestTimerController(api);
            timer.PresetAccessor = presetAcc;

            timer.StartTimer("abc");

            api.SetTime(5);

            timer.StartTimer("1");

            api.LastNotified.Should().BeNull();
            timer.CurrentTimerReservations.Should().HaveCount(2);

            api.SetTime(20);

            api.LastNotified.Should().Be("Hoge");
            timer.CurrentTimerReservations.Should().HaveCount(1);

            api.SetTime(25);

            api.LastNotified.Should().Be("Test1");
            timer.CurrentTimerReservations.Should().HaveCount(0);
        }

        [TestMethod()]
        public void StartTimerNoPresetTest() {
            var presetData = new TimerPresetData(new Dictionary<string, TimerPresetEntry> {
                ["1"] = new TimerPresetEntry(20, "Test1"),
            });
            var presetAcc = new TimerPresetAccessor(presetData);
            var timer = new TestTimerController(api);
            timer.PresetAccessor = presetAcc;

            var act = () => timer.StartTimer("2");
            act.Should().Throw<ArgumentException>()
                .WithMessage("Preset '2' not found");
        }

        [TestMethod()]
        public void CancelLastTimerTest() {
            var timer = new TestTimerController(api);

            timer.StartTimer(10, "CancelTest");

            api.SetTime(8);
            timer.CurrentTimerReservations.Should().HaveCount(1);

            var rec = timer.CurrentTimerReservations.First();

            var canceledTimer = timer.CancelLastTimer();

            canceledTimer.Should().BeSameAs(rec.Value);

            api.SetTime(10);

            api.LastNotified.Should().BeNull();
            timer.CurrentTimerReservations.Should().HaveCount(0);

        }
    }
}
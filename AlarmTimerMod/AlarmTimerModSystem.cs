using AlarmTimerMod.Config;
using AlarmTimerMod.Core;
using AlarmTimerMod.Util;
using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.Client.NoObf;

namespace AlarmTimerMod {
    public class AlarmTimerModSystem : ModSystem {
        ICoreClientAPI capi;

        internal AlarmTimerModConfig Config { get; private set; }

        public string ModDataFileName {
            get {
                return Mod.Info.ModID + ".json";
            }

        }

        AlarmTimerModData modData;

        TimerPresetAccessor timerPresetAccessor;

        internal TimerController TimerController { get; private set; }

        public override void StartClientSide(ICoreClientAPI api) {
            capi = api;

            var configName = Mod.Info.ModID + ".json";
            Config = api.LoadModConfig<AlarmTimerModConfig>(configName);
            if (Config == null) {
                Config = new AlarmTimerModConfig();
                api.StoreModConfig(Config, configName);
            }

            LoadModData();

            api.Event.LeaveWorld += SaveModData;
            api.Event.RegisterGameTickListener((dt) => SaveModData(), 300 * 1000);

            var rootCommand = api.ChatCommands
                .Create(Config.rootCommandName)
                .WithDescription("Alarm timer")
                .RequiresPlayer()
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(args => {
                    api.ShowChatMessage($"See `.help {Config.rootCommandName}` or `.chb`");
                    return TextCommandResult.Success("");
                });

            var parsers = api.ChatCommands.Parsers;

            var presetCommand = rootCommand
                .BeginSubCommand("preset")
                .WithAlias("p");
            var presetAddCommand = presetCommand
                .BeginSubCommand("add")
                .WithAlias("a")
                .WithArgs(parsers.Word("name"), parsers.Int("duration_in_second"), parsers.All("message")) // To allow spaces in an argument, it should be parser.All and located end of the arguments
                .HandleWith(args => {
                    var name = args.Parsers[0].GetValue() as string;
                    var seconds = (int)args.Parsers[1].GetValue();
                    var message = args.Parsers[2].GetValue() as string;

                    if (seconds < 1) seconds = 1;

                    api.Logger.Event("{0},{1},{2}", name, message, seconds);

                    timerPresetAccessor.AddAndUpdate(name, new TimerPresetEntry(seconds, message));

                    return TextCommandResult.Success($"Added preset '{name}'");
                });
            var presetListCommand = presetCommand
                .BeginSubCommand("list")
                .WithAlias("l")
                .HandleWith(args => {
                    var result = timerPresetAccessor.GetListDescription();
                    return TextCommandResult.Success(result);
                });
            var presetDeleteCommand = presetCommand
                .BeginSubCommand("delete")
                .WithAlias("d")
                .WithArgs(parsers.Word("name"))
                .HandleWith(args => {
                    var name = args.Parsers[0].GetValue() as string;
                    var removed = timerPresetAccessor.Remove(name);

                    if (removed) {
                        return TextCommandResult.Success($"Removed preset '{name}'");
                    }
                    else {
                        return TextCommandResult.Error($"Preset '{name}' not found");
                    }
                });
            var presetUpdateDurationCommand = presetCommand
                .BeginSubCommand("update-duration")
                .WithAlias("ud")
                .WithArgs(parsers.Word("name"), parsers.Int("duration_in_second"))
                .HandleWith(args => {
                    var name = args.Parsers[0].GetValue() as string;
                    var seconds = (int)args.Parsers[1].GetValue();

                    if (seconds < 1) seconds = 1;

                    var old = timerPresetAccessor.GetOrNull(name);

                    if (old == null) {
                        return TextCommandResult.Error($"Preset '{name}' not found");
                    }

                    timerPresetAccessor.AddAndUpdate(name, new TimerPresetEntry(seconds, old.Message));

                    return TextCommandResult.Success($"Updated preset '{name}'");
                });

            var startCommand = rootCommand
                .BeginSubCommand("start")
                .WithAlias("s")
                .WithArgs(parsers.Int("duration_in_second"), parsers.All("message"))
                .HandleWith(args => {
                    var seconds = (int)args.Parsers[0].GetValue();
                    var message = args.Parsers[1].GetValue() as string;

                    TimerController.StartTimer(seconds, message);

                    return TextCommandResult.Success($"Timer started. It will notify you in {seconds} seconds");
                });

            var startPresetCommand = rootCommand
                .BeginSubCommand("start-preset")
                .WithAlias("sp")
                .WithArgs(parsers.OptionalWord("name"))
                .HandleWith(args => {
                    var name = args.Parsers[0].GetValue() as string;

                    if (name == null) {
                        var sb = new StringBuilder();
                        sb.AppendLine("Available presets:");
                        sb.AppendLine(timerPresetAccessor.GetListDescription());
                        return TextCommandResult.Success(sb.ToString());
                    }

                    var preset = timerPresetAccessor.GetOrNull(name);

                    if (preset == null) {
                        return TextCommandResult.Error($"Preset '{name}' not found");
                    }

                    TimerController.StartTimer(name);

                    return TextCommandResult.Success($"Timer started. It will notify you in {preset.Seconds} seconds");
                });

            var undoCommand = rootCommand
                .BeginSubCommand("undo")
                .HandleWith(args => {
                    var canceled = TimerController.CancelLastTimer();

                    if (canceled == null) {
                        return TextCommandResult.Error($"Failed to cancel. No history found, or the most recent timer has already completed");
                    }

                    return TextCommandResult.Success($"Canceled the timer with a duration of {canceled.seconds} seconds and message: '{canceled.message}'");
                });

            var volumeCommand = rootCommand
                .BeginSubCommand("volume")
                .WithArgs(parsers.Double("volume"))
                .HandleWith(args => {
                    var volume = (double)args.Parsers[0].GetValue();
                    Config.masterVolume = volume;
                    api.StoreModConfig(Config, configName);

                    return TextCommandResult.Success($"Volume set to {volume}");
                });


        }

        void SaveModData() {
            var futil = new FileUtil(Mod.Info.ModID);

            timerPresetAccessor.SaveTo(modData);

            futil.SaveModDataFile(ModDataFileName, modData);
        }

        void LoadModData() {
            var futil = new FileUtil(Mod.Info.ModID);

            modData = futil.LoadFromModDataFile<AlarmTimerModData>(ModDataFileName);

            if (modData == null) {
                modData = new AlarmTimerModData();
            }

            timerPresetAccessor = new TimerPresetAccessor();
            timerPresetAccessor.LoadFrom(modData);

            TimerController = new TimerController(capi);
            TimerController.PresetAccessor = timerPresetAccessor;
        }
    }
}

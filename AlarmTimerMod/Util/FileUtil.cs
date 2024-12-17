using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace AlarmTimerMod.Util {
    internal class FileUtil {
        string modId;
        public FileUtil(string modId) {
            this.modId = modId;
        }

        public string GetModDataDirectory() {
            return Path.Combine(GamePaths.DataPath, "ModData", modId);
        }

        public string GetModDataFilePath(string filename) {
            return Path.Combine(GetModDataDirectory(), filename);
        }
        public void SaveModDataFile<T>(string filename, T modData) {
            var path = GetModDataFilePath(filename);
            // Ensure parent directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var content = JsonUtil.ToString(modData);
            File.WriteAllText(path, content, Encoding.UTF8);
        }

        public T LoadFromModDataFile<T>(string filename) {
            var path = GetModDataFilePath(filename);
            if (!Path.Exists(path)) return default;

            return JsonUtil.FromString<T>(File.ReadAllText(path, Encoding.UTF8));
        }



    }
}

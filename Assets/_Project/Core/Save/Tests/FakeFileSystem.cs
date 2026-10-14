using System.Collections.Generic;
using System.IO;

namespace Kturker.UnityBase.Core.Save.Tests
{
    // In-memory files keyed by full path; Move refuses an existing target like File.Move. FailOn ("write x.json.tmp",
    // "move x.json", "read x.json") throws IOException for that one operation.
    public sealed class FakeFileSystem : IFileSystem
    {
        public readonly Dictionary<string, string> Files = new Dictionary<string, string>();
        public readonly List<string> Ops = new List<string>();
        public string FailOn;

        public bool Exists(string path) => Files.ContainsKey(path);

        public string ReadAllText(string path)
        {
            Check("read", path);
            return Files[path];
        }

        public void WriteAllText(string path, string text)
        {
            Check("write", path);
            Files[path] = text;
            Ops.Add($"write {Path.GetFileName(path)}");
        }

        public void Move(string from, string to)
        {
            Check("move", from);
            if (Files.ContainsKey(to)) throw new IOException($"{to} exists");
            Files[to] = Files[from];
            Files.Remove(from);
            Ops.Add($"move {Path.GetFileName(from)} {Path.GetFileName(to)}");
        }

        public void Delete(string path)
        {
            Files.Remove(path);
            Ops.Add($"delete {Path.GetFileName(path)}");
        }

        public void CreateDirectory(string path)
        {
        }

        private void Check(string operation, string path)
        {
            if (FailOn == $"{operation} {Path.GetFileName(path)}") throw new IOException($"{operation} failed");
        }
    }
}

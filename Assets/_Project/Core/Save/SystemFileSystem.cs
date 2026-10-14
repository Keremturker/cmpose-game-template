using System.IO;
using System.Text;

namespace Kturker.UnityBase.Core.Save
{
    public sealed class SystemFileSystem : IFileSystem
    {
        private static readonly Encoding Utf8 = new UTF8Encoding(false);

        public bool Exists(string path) => File.Exists(path);

        public string ReadAllText(string path) => File.ReadAllText(path, Utf8);

        public void WriteAllText(string path, string text) => File.WriteAllText(path, text, Utf8);

        public void Move(string from, string to) => File.Move(from, to);

        public void Delete(string path) => File.Delete(path);

        public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    }
}

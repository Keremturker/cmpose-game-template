namespace Kturker.UnityBase.Core.Save
{
    public interface IFileSystem
    {
        bool Exists(string path);
        string ReadAllText(string path);
        void WriteAllText(string path, string text);
        void Move(string from, string to);
        void Delete(string path);
        void CreateDirectory(string path);
    }
}

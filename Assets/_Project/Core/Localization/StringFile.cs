namespace Kturker.UnityBase.Core.Localization
{
    // One table file: its name (for messages and merge order) and its JSON text
    public readonly struct StringFile
    {
        public StringFile(string name, string json)
        {
            Name = name;
            Json = json;
        }

        public string Name { get; }
        public string Json { get; }
    }
}

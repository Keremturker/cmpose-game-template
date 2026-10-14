namespace Kturker.UnityBase.Core.Save
{
    // Turns the data JSON of version From into version To (To > From)
    public interface ISaveMigration
    {
        int From { get; }
        int To { get; }
        string Migrate(string dataJson);
    }
}

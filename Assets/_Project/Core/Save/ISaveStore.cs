namespace Kturker.UnityBase.Core.Save
{
    // Game state per slot (preferences stay in IPreferences). Load returns null when nothing usable is saved.
    public interface ISaveStore
    {
        T Load<T>(SaveSchema<T> schema) where T : class;
        void Save<T>(SaveSchema<T> schema, T data) where T : class;
        void Delete(string slot);
    }
}

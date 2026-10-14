using System;

namespace Kturker.UnityBase.Core.Save
{
    // A slot, the version this build writes, and the migrations that bring older saves up to it
    public sealed class SaveSchema<T> where T : class
    {
        private readonly ISaveMigration[] migrations;

        public SaveSchema(string slot, int version, params ISaveMigration[] migrations)
        {
            if (version < 1) throw new ArgumentOutOfRangeException(nameof(version), version, "versions start at 1");
            Slot = slot;
            Version = version;
            this.migrations = migrations ?? Array.Empty<ISaveMigration>();
            foreach (var migration in this.migrations)
            {
                if (migration.To <= migration.From || migration.To > version)
                    throw new ArgumentException($"migration {migration.From} -> {migration.To} does not lead towards version {version}");
            }
        }

        public string Slot { get; }
        public int Version { get; }

        public ISaveMigration MigrationFrom(int version)
        {
            foreach (var migration in migrations)
            {
                if (migration.From == version) return migration;
            }
            return null;
        }
    }
}

using System;
using Kturker.UnityBase.Core.Save;
using UnityEngine;

namespace Kturker.UnityBase.Features.Sample.Presentation
{
    // saveSystem flag: the sample's Complete count survives restarts (slot 'sample')
    public sealed class SavedCompletionCounter : ICompletionCounter
    {
        public static readonly SaveSchema<Progress> Schema = new SaveSchema<Progress>("sample", 1);

        private readonly ISaveStore store;
        private Progress progress;

        public SavedCompletionCounter(ISaveStore store) => this.store = store;

        public int? Current => Loaded().Completed;

        public int? Record()
        {
            var current = Loaded();
            if (current.Completed < int.MaxValue) current.Completed++;
            store.Save(Schema, current);
            return current.Completed;
        }

        private Progress Loaded()
        {
            if (progress == null) progress = store.Load(Schema) ?? new Progress();
            if (progress.Completed < 0) progress.Completed = 0;
            return progress;
        }

        [Serializable]
        public sealed class Progress
        {
            [SerializeField] private int completed;

            public int Completed
            {
                get => completed;
                set => completed = value;
            }
        }
    }
}

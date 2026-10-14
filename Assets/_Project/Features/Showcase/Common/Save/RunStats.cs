using System;
using Kturker.UnityBase.Core.Save;
using UnityEngine;

namespace Kturker.UnityBase.Features.Showcase.Common.Save
{
    // saveSystem flag in the showcase: finished runs and collected stars survive restarts (slot 'showcase')
    public sealed class RunStats
    {
        public static readonly SaveSchema<Totals> Schema = new SaveSchema<Totals>("showcase", 1);

        private readonly ISaveStore store;
        private Totals totals;

        public RunStats(ISaveStore store) => this.store = store;

        public int Runs => Loaded().Runs;
        public int Stars => Loaded().Stars;

        public void Record(int stars)
        {
            var current = Loaded();
            if (current.Runs < int.MaxValue) current.Runs++;
            current.Stars = (int)Math.Min(int.MaxValue, (long)current.Stars + Math.Max(0, stars));
            store.Save(Schema, current);
        }

        private Totals Loaded()
        {
            if (totals == null) totals = store.Load(Schema) ?? new Totals();
            if (totals.Runs < 0) totals.Runs = 0;
            if (totals.Stars < 0) totals.Stars = 0;
            return totals;
        }

        [Serializable]
        public sealed class Totals
        {
            [SerializeField] private int runs;
            [SerializeField] private int stars;

            public int Runs
            {
                get => runs;
                set => runs = value;
            }

            public int Stars
            {
                get => stars;
                set => stars = value;
            }
        }
    }
}

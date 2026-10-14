namespace Kturker.UnityBase.Core.Purchases
{
    // A free trial's length ("7 days" is 7 x Day)
    public readonly struct TrialPeriod
    {
        public TrialPeriod(int value, TrialUnit unit)
        {
            Value = value;
            Unit = unit;
        }

        public int Value { get; }
        public TrialUnit Unit { get; }
    }
}

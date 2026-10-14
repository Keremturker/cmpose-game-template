namespace Kturker.UnityBase.Core.Purchases
{
    // Success: IsPremium has followed. Cancelled: the user backed out, show nothing. Error: show MessageKey's text.
    public sealed class PurchaseResult
    {
        public static readonly PurchaseResult Success = new PurchaseResult(PurchaseOutcome.Success, null);
        public static readonly PurchaseResult Cancelled = new PurchaseResult(PurchaseOutcome.Cancelled, null);

        private PurchaseResult(PurchaseOutcome outcome, string messageKey)
        {
            Outcome = outcome;
            MessageKey = messageKey;
        }

        public PurchaseOutcome Outcome { get; }
        public string MessageKey { get; }

        public static PurchaseResult Error(string messageKey) => new PurchaseResult(PurchaseOutcome.Error, messageKey);
    }
}

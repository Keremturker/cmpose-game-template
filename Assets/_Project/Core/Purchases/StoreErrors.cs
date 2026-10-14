namespace Kturker.UnityBase.Core.Purchases
{
    // RevenueCat's ErrorCode numbers (the same on iOS and Android through purchases-hybrid-common)
    public static class StoreErrors
    {
        public static StoreErrorKind Kind(int code)
        {
            switch (code)
            {
                case 1: return StoreErrorKind.Cancelled;            // PurchaseCancelledError
                case 2: return StoreErrorKind.Store;                // StoreProblemError
                case 3: return StoreErrorKind.NotAllowed;           // PurchaseNotAllowedError
                case 5: return StoreErrorKind.ProductUnavailable;   // ProductNotAvailableForPurchaseError
                case 10: return StoreErrorKind.Network;             // NetworkError
                case 20: return StoreErrorKind.Pending;             // PaymentPendingError
                case 32: return StoreErrorKind.Store;               // ProductRequestTimedOut
                case 35: return StoreErrorKind.Network;             // OfflineConnectionError
                default: return StoreErrorKind.Other;
            }
        }
    }
}

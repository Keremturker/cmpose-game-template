using System;
using Sdk = global::Purchases;

namespace Kturker.UnityBase.Core.Purchases
{
    // RevenueCat calls its listener component whenever the customer info changes (and with the cached one at start)
    public sealed class CustomerInfoListener : Sdk.UpdatedCustomerInfoListener
    {
        public Action<Sdk.CustomerInfo> Received;

        public override void CustomerInfoReceived(Sdk.CustomerInfo customerInfo) => Received?.Invoke(customerInfo);
    }
}

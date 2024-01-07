using Braintree;

namespace Checkout.utils
{
    public class BraintreeUtil
    {
        public static BraintreeGateway CreateGateway()
        {
            return new BraintreeGateway(Braintree.Environment.SANDBOX, "yqck6hmf4yqxwsmg", "gb4cx3j78ftbt9mn", "bc21fc818cffcfeda573c2afd0525273");
        }
    }
}
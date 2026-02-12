namespace AppIt.Core.Configuration
{
    public class PaymentProviderOptions
    {
        public StripeOptions Stripe { get; set; } = new();
        public PayPalOptions PayPal { get; set; } = new();
    }

    public class StripeOptions
    {
        public string SecretKey { get; set; } = "REPLACE_WITH_STRIPE_SECRET_KEY";
    }

    public class PayPalOptions
    {
        public string ClientId { get; set; } = "REPLACE_WITH_PAYPAL_CLIENT_ID";
        public string ClientSecret { get; set; } = "REPLACE_WITH_PAYPAL_CLIENT_SECRET";
        public string Environment { get; set; } = "Sandbox";
    }
}

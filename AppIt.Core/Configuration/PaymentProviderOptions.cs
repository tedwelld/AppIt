namespace AppIt.Core.Configuration
{
    public class PaymentProviderOptions
    {
        public StripeOptions Stripe { get; set; } = new();
        public PayPalOptions PayPal { get; set; } = new();
        public Dictionary<string, string> MethodAliases { get; set; } = new(StringComparer.OrdinalIgnoreCase)
        {
            ["PayPal"] = "PayPal",
            ["Mastercard"] = "Stripe",
            ["Visa"] = "Stripe",
            ["Card"] = "Stripe",
            ["Stripe"] = "Stripe",
            ["CashApp"] = "Manual",
            ["EcoCash"] = "Manual",
            ["Bank Transfer"] = "Manual",
            ["Manual"] = "Manual"
        };
    }

    public class StripeOptions
    {
        public string SecretKey { get; set; } = "REPLACE_WITH_STRIPE_SECRET_KEY";
        public string WebhookSecret { get; set; } = "REPLACE_WITH_STRIPE_WEBHOOK_SECRET";
    }

    public class PayPalOptions
    {
        public string ClientId { get; set; } = "REPLACE_WITH_PAYPAL_CLIENT_ID";
        public string ClientSecret { get; set; } = "REPLACE_WITH_PAYPAL_CLIENT_SECRET";
        public string Environment { get; set; } = "Sandbox";
        public string WebhookId { get; set; } = "REPLACE_WITH_PAYPAL_WEBHOOK_ID";
    }
}

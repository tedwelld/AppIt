namespace AppIt.Core.AppServices
{
    public static class BookingStatusResolver
    {
        public static (string Status, string PaymentStatus) Resolve(
            bool isCreditAgent, decimal paymentAmount, decimal totalAmount, string? requestedStatus)
        {
            if (!string.IsNullOrWhiteSpace(requestedStatus))
            {
                var status = requestedStatus.Trim();
                var payment = paymentAmount >= totalAmount ? "FullyPaid"
                    : paymentAmount > 0 ? "Deposited" : "NotPaid";
                if (isCreditAgent && status is "Confirmed" or "Provisional")
                    return (status, "NotPaid");
                return (status, payment);
            }

            if (isCreditAgent)
                return ("Confirmed", "NotPaid");
            if (paymentAmount >= totalAmount && totalAmount > 0)
                return ("Closed", "FullyPaid");
            if (paymentAmount > 0)
                return ("Confirmed", "Deposited");
            return ("Provisional", "NotPaid");
        }
    }
}

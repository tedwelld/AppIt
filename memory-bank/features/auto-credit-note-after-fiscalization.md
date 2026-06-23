# Auto Credit Note After Fiscalization (AppIt)

When a **fiscalized** booking is amended, AppIt must reverse the prior fiscal document before re-fiscalizing the new invoice.

## Sequence

```mermaid
sequenceDiagram
    participant Booking
    participant InvoiceSvc as InvoiceService
    participant Refund as Refund CN
    participant FiscalJob
    participant ZRA

    Booking->>InvoiceSvc: Save amended fiscalized booking
    InvoiceSvc->>InvoiceSvc: MarkForRefiscalizationIfNeeded
    InvoiceSvc->>Refund: AutoCreateFiscalCreditNoteAsync
    FiscalJob->>ZRA: FiscalizeCreditNoteAsync
    Note over Booking: Staff re-fiscalize invoice from Refunds
```

## Implementation

- `InvoiceService.MarkForRefiscalizationIfNeeded` sets `IsReadyToRefiscalize`, snapshots baseline JSON.
- `FiscalCreditNoteService` creates pending credit note linked to original fiscal receipt.
- `FiscalJob` processes pending CNs then pending invoices when `FiscalIntegration:ZraEnabled=true`.
- When `ZraEnabled=false`, fiscal fields update in DB only (dev mode).

## UI

- Refunds / credit-notes operational flow shows refiscalization queue.
- Booking modal fiscal banner when `IsReadyToRefiscalize`.

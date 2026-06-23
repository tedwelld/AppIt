# Combo Products (AppIt)

Combo packages bundle multiple catalog services (accommodation, activity, transfer, misc) under one sellable header with mandatory child splits on booking.

## Entities

| Entity | Role |
|--------|------|
| `Combo` | Header: name, code, supplier, category, `MaxProducts` |
| `ComboProduct` | Child line: `ServiceType` + `ServiceId`, mandatory flag |
| `ComboPrice` | Optional package price override |
| `ReservationServiceItem` | Booking line; `ComboId` + `ProductKind=Combo` when header |
| `ReservationServiceItemSplit` | Per-child split amounts for GL/commission |

## API

- `GET/POST/PUT/DELETE /api/combos`
- `GET/POST /api/combo-product-prices`
- Booking checkout expands combo into splits via `BookingService` / `ComboService`

## UI

- Setup: `entities.page` combo tab or `/admin/setup/manage-combos`
- Booking: combo picker in `BookingWizardPage` step 2; split breakdown in line detail

## GoldenDusk reference

Ported from `GoldenDusk.Core/AppServices/SWAIBMS/ComboService.cs` and `BookingComponentService` combo save path.

## Business rules

1. All mandatory `ComboProduct` rows must appear as splits when a combo is sold.
2. Combo header price may differ from sum of children (surplus/shortfall handled via misc `SP`/`SHORT` if needed).
3. Day-end journals post combo splits separately (`DayEndService`).

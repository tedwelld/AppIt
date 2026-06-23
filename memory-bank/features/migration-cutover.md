# Migration and Sync Cutover (AppIt)

Optional tooling for legacy system migration (GoldenDusk `GoldenDuskMigration` / `GdSyncService.Api` parity).

## Components

| Component | Purpose |
|-----------|---------|
| `AppItMigration` (console) | One-off SWAPOS/legacy → AppIt data load |
| `AppItSyncService.Api` | Ongoing reservation sync between environments |
| `SimunyeJob` | IMAP CSV import for DPO payment bookings |

## Greenfield deployments

Not required when starting with empty operational data (`OperationalDataCleaner` / `APPIT_PURGE_OPERATIONAL_DATA`).

## Cutover checklist

1. Run catalog + company setup in AppIt.
2. Map products via `HConnectProductMapping` / Beds24 inventory sync.
3. Disable Simunye job after cutover (`Jobs:EnableSimunyeJob=false`).
4. Enable fiscal job only after VSDC device init (`Jobs:EnableFiscalJob=true`).

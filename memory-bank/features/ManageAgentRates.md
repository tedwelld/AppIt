# Manage Agent Rates (AppIt)

Annual agent-specific rate schedules with GM approval and external agent sign-off.

## Entity

`AgentProductPrice` — keyed by `CompanyId` (agent), `ProductType`, `ProductId`, `YearEffected`.

Approval flags: `IsVerified`, `IsApproved`, `IsAgentApproved`, `Sent`, `Query`.

## Workflow

1. Staff creates rates → `POST /api/product-price-agent`
2. GM verification → `PUT /api/product-price-agent/verify`
3. GM approval + email to agent → `PUT /api/product-price-agent/send-to-agent`
4. Agent signs via portal → `PUT /api/product-price-agent/agent-approval`
5. Query loop → `POST /api/product-price-agent/query`

## UI routes

| Route | Audience |
|-------|----------|
| `/admin/setup/manage-agent-rates` | Internal staff |
| `/agent-portal/manage-rates/:agentId/:year/:name` | External agent (no shell) |

## Pricing integration

`PricingService.ResolveUnitPriceAsync` checks agent tier after special price, before rack:

**Contract → Special → Agent → Rack**

Agent rates apply only when `IsApproved && IsAgentApproved`.

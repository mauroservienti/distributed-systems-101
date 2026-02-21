# Integration Tests Plan

## Overview

This document describes the plan to add basic integration tests to each demo in this repository. The primary goal is to validate that demos continue working correctly when NuGet packages are updated.

## Technology Choices

- **Test framework**: xUnit 2.x
- **NServiceBus handler testing**: `NServiceBus.Testing` 10.x — provides `TestableMessageHandlerContext` so handlers and sagas can be exercised without a running broker or database
- **Project structure**: one `tests/Demo.Tests/` folder per demo, referenced by the demo's `Demo.sln`

## Demo-by-Demo Plan

### Demos 01–05 (Raw RabbitMQ)

**Demos**: `01-basic-send`, `02-req-resp`, `03-req-multi-resp`, `04-basic-pub-sub`, `05-pub-multi-sub`

All logic in these demos lives inside `Program.cs` event-handler lambdas and is tightly coupled to live RabbitMQ connections. There are no extracted handler classes to unit-test in isolation.

**Testing strategy**: *API-surface smoke tests.*  
Each test project references the demo's source projects and verifies that the RabbitMQ.Client types used in the demo (`ConnectionFactory`, `CreateChannelOptions`, `BasicProperties`, `AsyncEventingBasicConsumer`) can be instantiated. This catches breaking changes in the `RabbitMQ.Client` package without requiring a running broker.

**Test cases per demo**:
1. `ConnectionFactory_can_be_created` — `new ConnectionFactory()` succeeds
2. `CreateChannelOptions_can_be_created` — `new CreateChannelOptions(true, true)` succeeds
3. `BasicProperties_can_be_created` — `new BasicProperties()` succeeds

---

### Demo 06 — Commands & Pub/Sub with NServiceBus (`06-cmd-events`)

Uses `NServiceBus.Testing.TestableMessageHandlerContext`.

**Handler: `Sales.PlaceOrderHandler`**
1. `PlaceOrder_should_reply_with_PlaceOrderReply` — handler replies with `PlaceOrderReply` containing the same `OrderId`
2. `PlaceOrder_should_publish_OrderPlaced` — handler publishes `OrderPlaced` with the same `OrderId`

**Handler: `Billing.OrderPlacedHandler`**
3. `OrderPlaced_should_publish_PaymentAuthorized` — handler publishes `PaymentAuthorized` with the same `OrderId`

**Handler: `Warehouse.OrderPlacedHandler`**
4. `OrderPlaced_should_publish_OrderItemsCollected` — handler publishes `OrderItemsCollected` with the same `OrderId`

---

### Demo 07 — Commands, Pub/Sub & Recoverability (`07-cmd-events-recoverability`)

Handlers are structurally identical to Demo 06. The Shipping handlers (`PaymentAuthorizedHandler`, `OrderItemsCollectedHandler`) make direct PostgreSQL calls, so they are excluded from automated testing.

**Handler: `Sales.PlaceOrderHandler`**
1. `PlaceOrder_should_reply_with_PlaceOrderReply`
2. `PlaceOrder_should_publish_OrderPlaced`

**Handler: `Billing.OrderPlacedHandler`**
3. `OrderPlaced_should_publish_PaymentAuthorized`

**Handler: `Warehouse.OrderPlacedHandler`**
4. `OrderPlaced_should_publish_OrderItemsCollected`

---

### Demo 08 — Basic Saga (`08-basic-saga`)

**Handler: `Sales.PlaceOrderHandler`** (same tests as Demo 06/07)

**Saga: `Shipping.ShippingPolicy`**
1. `PaymentAuthorized_received_first_should_not_publish_ShipmentReady` — only first message; saga not complete
2. `OrderItemsCollected_received_first_should_not_publish_ShipmentReady` — only first message; saga not complete
3. `Both_messages_received_should_publish_ShipmentReady` — after both messages are handled, `ShipmentReady` is published

---

### Demo 09 — Timeouts (`09-timeouts`)

**Saga: `Shipping.ShippingPolicy`** (same three tests as Demo 08)

**Handler: `Finance.ShipmentReadyHandler`**
1. `ShipmentReady_should_publish_InvoiceIssued` — handler publishes `InvoiceIssued` with matching `OrderId`

**Saga: `Finance.OverdueInvoicePolicy`**
2. `InvoiceIssued_should_request_timeout` — saga stores data and requests a `CheckPayment` timeout
3. `InvoiceIssued_for_Italy_should_request_extended_timeout` — Italy customers get a 20-day extension
4. `InvoicePaid_should_mark_saga_as_complete` — saga completes when invoice is paid
5. `Timeout_when_unpaid_should_publish_InvoiceOverdue` — timeout handler publishes `InvoiceOverdue` and completes

---

### Demo 10 — ViewModel Composition (`10-composition`)

Composition handlers implement `ICompositionRequestsHandler` (ServiceComposer.AspNetCore) and make live HTTP calls to back-end APIs. Full integration testing requires running databases and HTTP servers.

**Testing strategy**: *Compilation smoke test.*  
A test project references the composition projects and includes a single always-passing test confirming the assembly loads. This guarantees all source projects (and their transitive dependencies) still compile after a package update.

---

### Demo 11 — ViewModel Decomposition (`11-decomposition`)

Same situation as Demo 10.

**Testing strategy**: Same compilation smoke test approach as Demo 10.

---

### Demo 12 — Shopping Cart Lifecycle (`12-cart-lifecycle`)

Most handlers require a running database (`SalesContext`, `SalesData`, etc.). The `ShoppingCartLifecyclePolicy` saga contains pure business logic.

**Saga: `Sales.Service.Policies.ShoppingCartLifecyclePolicy`**
1. `ProductAddedToCart_should_request_stale_and_wipe_timeouts` — two timeouts are scheduled
2. `Stale_timeout_when_cart_still_active_should_not_publish` — `LastTouched` updated after timeout was scheduled; no event published
3. `Stale_timeout_when_cart_untouched_should_publish_ShoppingCartGotStale` — publishes the event
4. `Wipe_timeout_when_cart_untouched_should_publish_ShoppingCartGotInactive_and_complete` — publishes event and marks saga complete

---

## Project Structure

```
{demo}/
└── tests/
    └── Demo.Tests/
        ├── Demo.Tests.csproj
        └── *Tests.cs
```

Test projects are added to each demo's `Demo.sln`.

## Running the Tests

```bash
# Run tests for a single demo
cd 06-cmd-events && dotnet test tests/Demo.Tests/Demo.Tests.csproj

# Run all tests across all demos
for dir in 0*/; do dotnet test "$dir/tests/Demo.Tests/Demo.Tests.csproj" --no-build 2>/dev/null || dotnet test "$dir/tests/Demo.Tests/Demo.Tests.csproj"; done
```

## Limitations

- **Demos 01–05**: Tests verify API surface only; end-to-end message routing still requires a running RabbitMQ instance.
- **Demo 07 Shipping**: `PaymentAuthorizedHandler` and `OrderItemsCollectedHandler` require a PostgreSQL database and are excluded.
- **Demos 10–11**: ViewModelComposition handlers make live HTTP calls; only compilation is verified.
- **Demo 12**: Handlers that use `SalesContext`/`SalesData`/etc. require a running database and are excluded.

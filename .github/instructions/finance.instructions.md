---
applyTo: "modules/Finance/**/*"
---

# Finance Module Directives

When working within the `modules/Finance` boundary, you MUST adhere to these critical financial directives:

- **No Floating-Point Money:** Strictly reject floating-point data types (`float`, `double`) for monetary amounts. You must mandate integer cents (or the smallest denomination of the respective currency) for all financial storage and calculation.
- **ISO 4217 Currency Pairings:** Require explicit ISO 4217 currency codes/pairings for every financial transaction payload or storage structure. Never assume a default currency.
- **Cryptographic Ledger Integrity:** Mandate cryptographically chained immutable transaction ledgers. All transaction records must enforce the chaining rule: `H_n = SHA-256(H_{n-1} || Payload_Hash_n)`.

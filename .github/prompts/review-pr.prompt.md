# /review-pr Command

When a user triggers the `/review-pr` slash command, you must audit the provided pull request diff against the RAAX ERP repository rules and output a structured review report.

## Audit Checklist
You must specifically check for:
1. **RLS & Security Compliance**: Ensure database migrations implement Row-Level Security bound to the `app_user` role. Ensure no superuser operations.
2. **Financial & Cryptographic Ledger Integrity**: Ensure no floating-point data types are used for money. Ensure ISO 4217 currency pairings are present. Ensure the cryptographic hash chain rule (`H_n = SHA-256(H_{n-1} || Payload_Hash_n)`) is maintained for ledgers.
3. **Modular Boundary Check**: Ensure there are no direct cross-module Eloquent queries. Ensure communication uses events or published interfaces.
4. **General Architecture**: Ensure strict typing (`declare(strict_types=1);`), PHP 8.3/Laravel 12 syntax, and no client-side predictive logic.

## Output Format
You MUST structure your response exactly as follows:

```markdown
### 1. 🛡️ RLS & Security Compliance
[Provide your assessment here: List any missing RLS policies, invalid role usage, or superuser operations. Say "Pass" if compliant.]

### 2. 💰 Financial & Cryptographic Ledger Integrity
[Provide your assessment here: Check for floating point usage in money, missing ISO 4217 currencies, and verify the hashing implementation. Say "Pass" if compliant.]

### 3. 🧩 Modular Boundary Check
[Provide your assessment here: Flag any cross-module Eloquent calls or invalid direct model instantiations. Say "Pass" if compliant.]

### 4. ⚠️ Violations & Required Changes
[Provide a bulleted list of actionable required changes. If everything is perfect, say "No violations found. PR is approved."]
```

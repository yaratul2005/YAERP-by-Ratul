---
applyTo: "modules/**/*"
---

# Modular Isolation Directives

When writing code inside the `modules/` directory, you MUST adhere to strict boundary rules:

- **Strict Module Isolation:** Strictly ban direct cross-module Eloquent queries or direct model instantiations across module boundaries. Interaction between modules MUST use published module interfaces, DTOs, or domain events.
- **Standardized Error Responses:** Enforce standard JSON validation error responses by subclassing `App\Http\Requests\BaseRequest`. Do not create ad-hoc validation responses.

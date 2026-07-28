---
applyTo: "database/migrations/**/*"
---

# Database Migration Directives

When writing database migrations for the RAAX ERP project, you MUST adhere to the following security directives:

- **Row-Level Security (RLS) is Mandatory:** Every migration that creates a new table must explicitly enable Row-Level Security on that table (`ALTER TABLE ... ENABLE ROW LEVEL SECURITY;`).
- **Policy Attachment:** You must force policy attachments using the `tenant_isolation_policy` bound strictly to the non-superuser database role `app_user`.
- **No Superuser Operations:** Reject any migration attempting to run table operations as a database superuser.
- **No RLS Bypass:** Do not generate any code or migration step that attempts to bypass RLS policies.

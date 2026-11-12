# IdentityAccess structure demo

This isolated service demonstrates two user-data operations: `UsersCreate` (command) and `UsersGetOne` (query). Domain owns the user invariant; Application owns handlers, typed results and narrow persistence ports; Database owns EF records/model; Infrastructure owns mapping and PostgreSQL access; Migrations owns schema changes; API owns HTTP mapping and exception handling. A caller supplies a UUIDv7 `requestId` to `UsersCreate`; the unique request ID allows a matching retry to return the same created user after a lost response.

The API is an internal Aspire resource and has no Gateway route. The Angular `/identity-demo` feature uses a separate in-memory synthetic fixture to show feature ownership and styling without exposing unauthenticated identity administration. It does not read or write the IdentityAccess database. Authentication, authorization, Google OIDC, session, role, and resource catalogs remain outside this demo.

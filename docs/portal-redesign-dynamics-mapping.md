# Portal Redesign: Prototype-to-Dynamics Mapping

Draft technical design. Purpose: when we pick a feature to implement (for example Special Event Permits), this document says which **new portal (prototype)** pages, components and data modules to touch, which **current portal** APIs and Dynamics entities they depend on, and which prototype code is unrelated and can be left alone.

High level only. Field-level mappings are in [portal-redesign-field-mapping.md](./portal-redesign-field-mapping.md). All mappings are proposals to confirm with the Dynamics team.

## 1. How to read this document

Every file, type and name is from one of two code bases. The tags and formatting below tell you which.

| Tag | Code base | How it is written |
|---|---|---|
| **[NEW]** | Prototype: `v0-liquor-license-portal-redesign` (Next.js, TypeScript). This is the new portal design. | Plain code text, path relative to the prototype repo root, for example `app/special-events/page.tsx`. **Never a link**, because it is not in this repo. |
| **[CURRENT]** | This repo: the existing portal (ASP.NET Core API, Angular client) and its Dynamics (Dataverse) model. | Markdown link to the file, for example [SpecialEventsController](../cllc-public-app/Controllers/SpecialEventsController.cs). Dynamics entities and fields (`adoxio_*`, `account`, `contact`, `invoice`) are always current. |

Type names follow the same rule:

- **[NEW]** TypeScript types in the prototype, for example `SepRecord`, `SepStatus`, `LENode`, `TiedHouseDeclaration`, `CatalogEntry`.
- **[CURRENT]** C# view models, enums and entities, for example `SpecialEvent`, `AdoxioApplicationStatusCodes`, `LegalEntity`, `TiedHouseConnection`.

In tables, column headings carry the tag, so a row reads as "new thing on the left, existing thing it maps to on the right".

Sources:

| Side | Location |
|---|---|
| **[NEW]** | `v0-liquor-license-portal-redesign`. Also read its `DEVELOPER_HANDOFF.md` and `FEATURES.md`. |
| **[CURRENT]** | [cllc-public-app/Controllers](../cllc-public-app/Controllers) (API), [cllc-public-app/ViewModels](../cllc-public-app/ViewModels) (API models), [cllc-interfaces/Dynamics-Dataverse/Generated](../cllc-interfaces/Dynamics-Dataverse/Generated) (Dataverse entities). |

Only concepts that exist in the current portal are covered. Prototype-only features are listed in section 9.

How the prototype is layered: **[NEW]** pages in `app/*` read and write through client-side stores (`components/providers/*-store-provider.tsx`, `lib/services/*-store.ts`) seeded from mock data in `lib/data/*`, persisted in `localStorage`. **Implementing a slice means replacing that slice's store and `lib/data` module with calls to the existing [CURRENT] portal API**, keeping the exported **[NEW]** types where the UI imports them.

Fit legend: **Direct** = concept and most fields exist. **Partial** = concept exists, some prototype fields have no home. **Gap** = new field or entity needed.

## 2. Shared foundation (needed by every slice)

Whichever slice is first, these are touched or at least wired up once.

| Prototype piece **[NEW]** | Role | Current-portal counterpart **[CURRENT]** | Work |
|---|---|---|---|
| `components/providers/session-provider.tsx`, `app/login/page.tsx`, `lib/roles.ts` | Mock identity and role | BCeID login via [LoginController](../cllc-public-app/Controllers/LoginController.cs), [UserController](../cllc-public-app/Controllers/UserController.cs), [AccountsController](../cllc-public-app/Controllers/AccountsController.cs) (`current`) | Replace the mock with the real session. Roles are derived from the user and account, not selected. |
| `app/layout.tsx` | Mounts all store providers | n/a | Remove the providers for slices that move to the API. |
| `lib/services/kv-store.ts` | `localStorage` persistence | n/a | Delete per slice as stores move to the API. |
| `components/bc-gov/*` (`header`, `footer`, `page-hero`, status badges), `components/ui/*` | Shared UI | n/a | Reused as is. |
| `components/application-framework/*` (`focus-flow`, `config-flow`, `fields`, `field-schema`) | Form engine used by SEP and licensee applications | Dynamics form metadata through [FormsController](../cllc-public-app/Controllers/FormsController.cs), [ApplicationTypesController](../cllc-public-app/Controllers/ApplicationTypesController.cs) | Decide where flow definitions live (see section 10). Needed by slices A and B. |
| `app/upload/page.tsx`, `app/api/upload`, `app/api/file`, `app/api/files`, `components/documents/document-manager.tsx` | Vercel Blob uploads | [FileController](../cllc-public-app/Controllers/FileController.cs), File Manager service, SharePoint (see [azure-document-storage-discovery.md](./azure-document-storage-discovery.md)) | Point the uploader at `FileController`. |
| `lib/services/payment-service.ts`, `components/application-framework/payment-receipt.tsx`, `lib/receipts/*`, `app/receipts/page.tsx` | Fee and receipt | [PaymentController](../cllc-public-app/Controllers/PaymentController.cs), [SepPaymentController](../cllc-public-app/Controllers/SepPaymentController.cs), [InvoiceController](../cllc-public-app/Controllers/InvoiceController.cs), `invoice` entity | Needed by any slice that charges a fee. |
| `lib/services/notification-service.ts`, `app/notifications/page.tsx`, `components/notifications/*` | Notifications | None confirmed | Treat as later; see slice F. |

## 3. Slice A: Special Event Permits

Smallest self-contained slice. The current portal has a complete SEP API and data model.

### A1. Applicant side: prototype files to touch [NEW]

| Prototype file **[NEW]** | Purpose |
|---|---|
| `app/special-events/page.tsx` | My permits list, stats, tabs |
| `app/special-events/apply/page.tsx` | Five-step application (Applicant, Eligibility, Location, Liquor, Summary) |
| `lib/data/sep-flow.ts` | `sepGuidedConfig` flow definition |
| `app/special-events/[id]/page.tsx` | Permit detail, conditions, decision |
| `app/special-events/servings-calculator/page.tsx`, `lib/special-events/servings-prefill.ts` | Servings calculator and prefill into the form |
| `components/special-events/sep-access-guard.tsx` | Role gate |
| `components/providers/sep-store-provider.tsx`, `lib/data/seps.ts` | Store and types. **Replace with an API client**, keep the `SepRecord` and `SepStatus` shapes where the UI imports them. |
| `components/bc-gov/sep-status-badge.tsx`, `lib/sep-pdf.ts` | Status badge and permit PDF |
| `lib/data/drafts.ts`, `components/applications/cancel-application-dialog.tsx` | Drafts and cancel (shared with slice B) |

### A2. Police side: prototype files to touch [NEW]

| Prototype file **[NEW]** | Purpose |
|---|---|
| `app/police/page.tsx` | Dashboard |
| `app/police/queue/page.tsx` | Review queue |
| `app/police/review/[id]/page.tsx` | Review, assign officer, conditions, approve or deny |
| `app/police/events/page.tsx` | Upcoming approved events |
| `components/police/related-permits.tsx` | Related permits |

### A3. Current portal dependencies [CURRENT]

Controllers: [SpecialEventsController](../cllc-public-app/Controllers/SpecialEventsController.cs) (`api/special-events`), [SepPaymentController](../cllc-public-app/Controllers/SepPaymentController.cs) for payment, [PoliceJurisdictionsController](../cllc-public-app/Controllers/PoliceJurisdictionsController.cs) for autocomplete.

| Prototype concept **[NEW]** | Current endpoint or entity **[CURRENT]** |
|---|---|
| Create, update, submit, invoice | `POST`, `PUT {eventId}`, `POST submit/{eventId}`, `POST generate-invoice/{eventId}` |
| Applicant list, detail, permit PDF | `GET current/submitted`, `GET applicant/{eventId}`, `GET applicant/{eventId}/permit/{filename}` |
| Servings calculator drink list | `GET drink-types` (`adoxio_sepdrinktype`) |
| City to jurisdiction (`jurisdictionForCity` in `lib/data/seps.ts`) | `GET sep-city/autocomplete` (`adoxio_sepcity`, police jurisdiction, LG/IN) |
| Police queues | `GET police/all`, `police/pending-review`, `police/approved`, `police/denied`, `police/my` |
| Police actions | `POST police/{id}/assign`, `approve`, `deny`, `cancel`, `setMunicipality/{cityId}` |
| Conditions | `PUT terms-and-conditions/{eventId}` (`adoxio_specialeventtandc`) |
| Core data | `adoxio_specialevent`, `adoxio_specialeventlocation`, `adoxio_specialeventschedule`, `adoxio_specialeventlicencedarea`, `adoxio_sepdrinksalesforecast`, `invoice` |

### A4. Data fit and gaps

Prototype type: `SepRecord` **[NEW]**, in `lib/data/seps.ts`. Current type: `SpecialEvent` **[CURRENT]**, in [SpecialEvent.cs](../cllc-public-app/ViewModels/SpecialEvent.cs), with [SepEventLocation](../cllc-public-app/ViewModels/SepEventLocation.cs), [SepEventDates](../cllc-public-app/ViewModels/SepEventDates.cs), [SepServiceArea](../cllc-public-app/ViewModels/SepServiceArea.cs) and [SepDrinkSalesFocast](../cllc-public-app/ViewModels/SepDrinkSalesFocast.cs).

| Area | Fit | Notes |
|---|---|---|
| Application form (eligibility flags, event, terms, responsible-service certificate, admission, annual event) | Direct | `SpecialEvent` covers these. |
| Location, dates, guests, minors | Direct | `SpecialEvent` allows several locations and dates per event. The **[NEW]** form captures one. |
| Liquor types, estimated drinks, selling vs free | Partial | **[CURRENT]** model is a drink-type forecast with servings and price. **[NEW]** `SepRecord` holds a flat `liquorTypes` list and one `estimatedDrinks` total. |
| Security staff (`hasSecurity`, `securityStaffCount`) | Gap | **[NEW]** only. No confirmed current field. |
| Status | Partial | **[NEW]** `SepStatus` needs a translation to **[CURRENT]** `EventStatus`, `PoliceApproval`, `LcrbApproval`. |
| History entries (`SepHistoryEntry`) | Gap | **[NEW]** only. No per-permit audit trail in the current model. |
| Assigned officer, conditions, denial and cancel reasons | Direct | **[CURRENT]** `PoliceDecisionBy`, terms and conditions, `DenialReason`, `CancelReason`. |

### A5. Prototype code not part of slice A [NEW]

Leave alone or defer. The current portal has no confirmed counterpart for any of these.

- `lib/services/sep-auto-approval.ts`, `components/police/auto-approval-*.tsx`, and the attendance and referral rules in `lib/data/seps.ts`. The prototype auto-approves by rule. The **[CURRENT]** portal has police approve and deny endpoints and no confirmed auto-approval. This is a business-rule decision, not a mapping.
- `components/police/scheduled-reports-card.tsx`, `lib/services/sep-report-schedule-service.ts`, `lib/pdf/sep-daily-report.ts`.
- `components/chat/case-thread.tsx` (applicant to police messages).
- AI cards used on these pages: `components/ai/reviewer-brief.tsx`, `draft-rationale-button.tsx`, `status-explainer.tsx`, `reviewer-suggested-actions.tsx`. They can stay unwired.

## 4. Slice B: Licensee applications

| Prototype file **[NEW]** | Purpose |
|---|---|
| `app/applications/new/page.tsx`, `app/applications/new/[slug]/page.tsx`, `app/applications/food-primary/page.tsx` | Catalog and flow entry |
| `lib/data/application-factory.ts`, `application-catalog-extended.ts`, `applications.ts`, `application-eligibility.ts`, `application-search.ts` | Catalog (about 90 `CatalogEntry` items) and step definitions |
| `components/application-framework/*` | Flow engine |
| `app/applications/page.tsx`, `lib/data/my-applications.ts`, `drafts.ts` | My applications and drafts |
| `app/applications/submitted/[id]/page.tsx` and `documents/page.tsx`, `lib/data/submitted-applications.ts`, `components/applications/submitted-file-panel.tsx` | Submitted detail and documents |
| `app/applications/confirmation/[id]/page.tsx`, `lib/services/submit-application.ts`, `lib/data/submissions.ts` | Submit and confirmation |
| `lib/services/cancellation-service.ts` | Cancel |
| `app/dashboard/page.tsx`, `components/dashboard/*` | Drafts panel and quick actions |

Current portal **[CURRENT]**: [ApplicationsController](../cllc-public-app/Controllers/ApplicationsController.cs) (`current`, `{id}`, `POST`, `PUT {id}`, `{id}/cancel`, `{id}/process`), [ApplicationTypesController](../cllc-public-app/Controllers/ApplicationTypesController.cs), [FormsController](../cllc-public-app/Controllers/FormsController.cs), [LicensesController](../cllc-public-app/Controllers/LicensesController.cs) (`create-action-application`, `initiate-transfer`, `set-third-party-operator`), [PaymentController](../cllc-public-app/Controllers/PaymentController.cs), [InvoiceController](../cllc-public-app/Controllers/InvoiceController.cs). Main model: [Application.cs](../cllc-public-app/ViewModels/Application.cs).

| Prototype concept **[NEW]** | Current model **[CURRENT]** | Fit |
|---|---|---|
| `CatalogEntry` (slug, title, `LicenceType`, `kind`, fee) | `adoxio_applicationtype`, `adoxio_licencetype` | Partial. Each entry needs a matching application type, and the fee needs a source. |
| Shared steps: Before Starting, Establishment, Documents, Contact, Review | `adoxio_application` establishment and contact-person fields (`Application` view model) | Direct |
| Renewal, dormancy, relocation, transfer, name change, structural change, patio, temporary changes, hours, third-party operator | Existing fields on `adoxio_application` and `adoxio_applicationextension` | Mostly direct. Confirm each slug has an application type. |
| Application statuses (`MyApplicationStatus`, `SubmittedStatus`) | `AdoxioApplicationStatusCodes` | Partial. Needs a translation table. |
| Fee and receipt | `invoice` | Direct. Reconcile the **[NEW]** fee catalog. |
| Pre-filled business details (`BusinessProfile` in `lib/data/business-profile.ts`) | `account`, via `AccountsController.business-profile/{accountId}` | Direct |

Not part of slice B **[NEW]**: `lib/ai/*` and `app/api/*` AI routes, `lib/services/prescreen-agent.ts`, `components/application-framework/assistant-bridge.tsx`, `eligibility-gate.tsx` (rule-based, evaluate separately), and `app/establishment-planner/*`.

## 5. Slice C: Licences and endorsements

| Prototype file **[NEW]** | Purpose |
|---|---|
| `app/licences/page.tsx`, `app/licences/[id]/page.tsx` | List and detail |
| `lib/data/licences.ts`, `lib/data/business-profile.ts` | `Licence` and `Endorsement` types, mock data |
| `components/licences/licence-cap-card.tsx`, `licence-inspections.tsx` | Defer (see section 9) |
| `app/calendar/page.tsx`, `components/calendar/*` | Renewals calendar. Derived from licence expiry dates. |

Current portal **[CURRENT]**: [LicensesController](../cllc-public-app/Controllers/LicensesController.cs) (`current`, `{id}`, `{licenceId}/representative`, `{licenceId}/offsite-storage`), [EndorsementsController](../cllc-public-app/Controllers/EndorsementsController.cs), [EstablishmentsController](../cllc-public-app/Controllers/EstablishmentsController.cs). Models: [License.cs](../cllc-public-app/ViewModels/License.cs), [Endorsement.cs](../cllc-public-app/ViewModels/Endorsement.cs).

| Prototype concept **[NEW]** | Current model **[CURRENT]** | Fit |
|---|---|---|
| `Licence` list and detail | `adoxio_licences`, `adoxio_establishment` (`License` view model) | Partial. `operatedBy`, `licencedTo` and `flags` have no confirmed source. |
| `Endorsement` | `adoxio_endorsement` | Partial. Permanent vs temporary and expiry need confirming. |
| Terms and conditions, off-site storage, service areas | `adoxio_termsconditionslimitationspreset`, `adoxio_offsitestorage`, `adoxio_servicearea` | Direct |
| Licence actions (renew, transfer, change) | Links into slice B | Direct |

## 6. Slice D: Business structure

### D1. Legal entity builder

Prototype **[NEW]**: `app/legal-entity/*`, `components/legal-entity/*`, `lib/data/legal-entity.ts` (types `LENode`, `LEEdge`, `LegalEntityBuild`), `lib/services/legal-entity-store.ts`.

Current portal **[CURRENT]**: [LegalEntitiesController](../cllc-public-app/Controllers/LegalEntitiesController.cs) (`current-hierarchy`, `child-legal-entity`, `save-change-tree/...`, `legal-entity-change-logs/...`, `cancel-change-logs`), [LEConnectionsController](../cllc-public-app/Controllers/LEConnectionsController.cs), [ApplicationsController](../cllc-public-app/Controllers/ApplicationsController.cs) (`legal-entity-review-data`, `pcl-for-le-review/{id}`). Model: [AdoxioLegalEntity.cs](../cllc-public-app/ViewModels/AdoxioLegalEntity.cs) (`LegalEntity`).

| Prototype concept **[NEW]** | Current model **[CURRENT]** | Fit |
|---|---|---|
| `LENode` and `LERole`, ownership percentage | `adoxio_legalentity` (parent and child tree, role flags, interest percentage) | Direct. **[NEW]** role `beneficiary` has no counterpart. |
| `LEEdge` (family, business agreement, DBA, connection) | `adoxio_leconnection` | Partial. Confirm option set coverage. |
| `LEBuildStatus`, draft changes, submit for review | `adoxio_licenseechangelog` and the change-tree endpoints, plus the LE review application | Partial. Closest existing workflow. Confirm it supports the prototype status model. |
| Review notes (`LEReviewNote`) and lock by LCRB | None confirmed | Gap |
| Canvas layout (node `position` x/y) | None | Gap. Presentation only. |

### D2. Tied house

Prototype **[NEW]**: `app/tied-house/*`, `components/tied-house/*`, `lib/data/tied-house.ts` (types `TiedHouseDeclaration`, `TiedHouseApplication`), `lib/services/tied-house-store.ts`.

Current portal **[CURRENT]**: [TiedHouseConnectionsController](../cllc-public-app/Controllers/TiedHouseConnectionsController.cs), [LicensesController](../cllc-public-app/Controllers/LicensesController.cs) (`initiate-tied-house-excemption`). Models: [TiedHouseConnection.cs](../cllc-public-app/ViewModels/TiedHouseConnection.cs), [TiedHouseAssociation.cs](../cllc-public-app/ViewModels/TiedHouseAssociation.cs).

| Prototype concept **[NEW]** | Current model **[CURRENT]** | Fit |
|---|---|---|
| `TiedHouseDeclaration` (individual or entity), relationship, date of birth | `adoxio_tiedhouseconnection` | Direct |
| `TiedHouseDomain` (liquor, cannabis) | `CategoryType` (`TiedHouseCategoryType`) | Direct |
| Associated licence | `adoxio_tiedhouseassociation` | Direct |
| `TiedHouseChangeType` (add, change, remove) | `StatusCode`, `SupersededById`, `MarkedForRemoval` | Direct |
| `TiedHouseApplication` wrapper, source, split fees | `adoxio_application`, `adoxio_applicationextension`, `invoice` | Partial |

### D3. Security screening

Prototype **[NEW]**: `app/security-screening/page.tsx`, `components/security/screening-person-card.tsx`, `lib/services/screening-store.ts`, `personnel-screening-service.ts`.

Current portal **[CURRENT]**: [ContactController](../cllc-public-app/Controllers/ContactController.cs) (`security-screening/{token}`, `phs-link`, `cass-link`), [LegalEntitiesController](../cllc-public-app/Controllers/LegalEntitiesController.cs) (`current-security-summary`, `sendconsentrequests`), [WorkerController](../cllc-public-app/Controllers/WorkerController.cs). Models: [Contact.cs](../cllc-public-app/ViewModels/Contact.cs), [Worker.cs](../cllc-public-app/ViewModels/Worker.cs).

Data: `contact` PHS and CAS fields, `adoxio_personalhistorysummary`, `adoxio_worker`. Fit: Direct. Defer **[NEW]** `lib/services/fit-and-proper-engine.ts` and `components/lcrb/fit-and-proper-panel.tsx` (no current counterpart).

## 7. Slice E: Local and Indigenous government review

Prototype **[NEW]**: `app/local-government/*` (dashboard, queue, review, map), `components/local-government/*`, `components/providers/lg-store-provider.tsx`, `lg-commenting-provider.tsx`, `lib/data/lg-applications.ts` (type `LgApplication`), `lg-commenting.ts`.

Current portal **[CURRENT]**: [ApplicationsController](../cllc-public-app/Controllers/ApplicationsController.cs) (`current/lg-approvals`, `lg-approvals-decision-not-made`, `lg-approvals-for-zoning`, `resolved-lg-applications`), [LocalGovernmentsController](../cllc-public-app/Controllers/LocalGovernmentsController.cs), [IndigenousNationsController](../cllc-public-app/Controllers/IndigenousNationsController.cs).

| Prototype concept **[NEW]** | Current model **[CURRENT]** | Fit |
|---|---|---|
| Referral queue and review (`LgApplication`, `LgDecision`) | `Application` LG fields (`LGApprovalDecision`, comments, official, `LGDecisionSubmissionDate`, `DateApplicantSentToLG`, `LgZoning`) | Partial. **[NEW]** decisions (no comment, community review, reject, documents requested) must be reconciled with **[CURRENT]** `LGDecision` (approved, opt out, rejected, pending). |
| Comment-due countdown | `DateApplicantSentToLG` plus deadline rule | Partial |
| Commenting opt-out (`LgCommentingPreference`) | `adoxio_localgovindigenousnation` | Gap |

## 8. Slice F: Account, users and notifications

Prototype **[NEW]**: `app/account/page.tsx`, `lib/data/account-settings.ts`.

Current portal **[CURRENT]**: [AccountsController](../cllc-public-app/Controllers/AccountsController.cs), [ContactController](../cllc-public-app/Controllers/ContactController.cs), [UserController](../cllc-public-app/Controllers/UserController.cs).

| Prototype concept **[NEW]** | Current model **[CURRENT]** | Fit |
|---|---|---|
| Profile and business details | `account`, `contact` | Direct |
| Delegated users (Full, View-only, Submit-only) | `contact` representative permission flags, `adoxio_login` | Gap. Existing flags are not the same access levels. |
| Notification preferences (email, SMS per category) | None confirmed | Gap |

## 9. Prototype code with no confirmed current counterpart [NEW]

Not mapped. Either prototype-only, or no matching entity was found in the generated Dynamics set. Safe to ignore when implementing slices A to F.

- AI layer: `app/api/*` AI routes, `lib/ai/*`, `components/ai/*`, `components/chat/*`, `components/bc-gov/ai-assistant.tsx`.
- Planning tools: `app/establishment-planner/*`, `app/licence-finder/*`, `lib/services/establishment-feasibility.ts`, `licence-cap-engine.ts`, `section-119-evaluator.ts`.
- Compliance and enforcement, inspections, notices, announcements: `app/compliance`, `app/notices`, `app/announcements`, `lib/data/enforcement.ts`, `inspections.ts`, `notices.ts`. Enforcement and inspection appear in [SharePointConstants.cs](../cllc-interfaces/SharePoint/SharePointConstants.cs) and in Dynamics option sets, but not in the generated entity list. Needs confirmation.
- LCRB staff surface: `app/lcrb/*`, `components/lcrb/*`. Staff work in Dynamics.
- Analytics, activity console, one-door, gate: `app/activity`, `app/one-door`, `app/gate`, `lib/services/site-analytics.ts`, `audit-service.ts`.
- Map: `app/map`, `components/map/*`. The **[CURRENT]** portal has an establishment map endpoint ([EstablishmentsController](../cllc-public-app/Controllers/EstablishmentsController.cs) `map`), so it may be a UI-only change. Not analysed.

## 10. Dependencies and suggested order

| Slice | Needs from section 2 | Depends on other slices |
|---|---|---|
| A. Special Event Permits | Session, framework, payment, documents | None |
| B. Applications | Session, framework, payment, documents | Business profile from F |
| C. Licences | Session | Actions link to B |
| D. Business structure | Session | LE review ties to B |
| E. LG review | Session | Reads applications from B |
| F. Account | Session | None |

Suggested order: foundation, then A (self-contained, full API exists), then B and C, then D and E, then F.

Decisions needed before building:

| # | Decision |
|---|---|
| 1 | Where flow definitions live: Dynamics form metadata **[CURRENT]**, or portal-owned config as in the prototype **[NEW]** (affects A and B). |
| 2 | Status translation tables for applications and SEPs. |
| 3 | SEP auto-approval: keep the prototype rule, or keep manual police decisions. |
| 4 | LE builder workflow: extend `adoxio_licenseechangelog`, or add new entities. |
| 5 | Delegate access levels, notification preferences, LG commenting opt-out: new fields. |
| 6 | Catalog items with no existing application type. |
| 7 | Tied house split fees across liquor and cannabis invoices. |

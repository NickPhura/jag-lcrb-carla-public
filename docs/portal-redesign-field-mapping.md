# Portal Redesign: Field-Level Mapping (Prototype to Current Model)

Companion to [portal-redesign-dynamics-mapping.md](./portal-redesign-dynamics-mapping.md). That document breaks the work into slices A to F. This one lists every prototype model field and its best-guess counterpart in the current portal model.

This is a best-guess draft, not a verified mapping. Most mappings were inferred from field names, labels and view-model comments. None have been checked against Dynamics data or the Dataverse-side attribute definitions. Confirm with the Dynamics team before building.

## 1. How to read this document

| Tag | Code base | Written as |
|---|---|---|
| **[NEW]** | Prototype `v0-liquor-license-portal-redesign` (TypeScript) | Plain text. Types and fields in `camelCase`, for example `SepRecord.eventName`. Files are relative to the prototype root. |
| **[CURRENT]** | This repo: C# view models in [cllc-public-app/ViewModels](../cllc-public-app/ViewModels) and Dynamics entities | `Type.Property`, for example `SpecialEvent.EventName`. Dynamics logical names (`adoxio_*`) appear only where a view-model comment states them. |

Column headings carry the tag. The left column is always **[NEW]**, the middle is **[CURRENT]**.

### Match levels

| Level | Meaning |
|---|---|
| **Direct** | Same concept and the current field clearly exists. |
| **Likely** | Best guess. The concept matches, but the type, cardinality or semantics may differ. |
| **Weak** | Loose guess. Only part of the concept maps, or the field is a stand-in. |
| **Derived** | No stored field is needed. The value is computed, comes from the session, or comes from another record. |
| **Missing** | No counterpart found in the current model. A new field, entity or decision is needed. |
| **Out of scope** | Prototype-only feature (AI output, analytics, staff-only tooling). See section 9 of the companion doc. |

### Notes column prefixes

Every note starts with a prefix that says what the note is about. A cell can hold more than one.

| Prefix | The note is about | Example |
|---|---|---|
| **New:** | The prototype field on the left: what it holds, how the prototype uses it. | "New: boolean, true when minors are expected." |
| **Current:** | The existing field or model in the middle: what it holds, how the current portal uses it, other fields near it. | "Current: `ModifiedOn` is the last-saved timestamp, not a completion date." |
| **Map:** | The mapping itself: a transformation needed, a decision, or an action to take. | "Map: use only when the status is Approved or Refused." |

### Scope

Covers the models behind slices A to F in the companion doc. Models that exist only for prototype features with no current counterpart (AI, planner, licence cap, compliance, inspections, notices, analytics, map markers) are not mapped.

Type names used on the current side, with files:

| Current type | File |
|---|---|
| `SpecialEvent`, `SepEventLocation`, `SepEventDates`, `SepServiceArea`, `SepDrinksSalesForecast`, `SepTermAndCondition`, `SepCity` | [SpecialEvent.cs](../cllc-public-app/ViewModels/SpecialEvent.cs), [SepEventLocation.cs](../cllc-public-app/ViewModels/SepEventLocation.cs), [SepEventDates.cs](../cllc-public-app/ViewModels/SepEventDates.cs), [SepServiceArea.cs](../cllc-public-app/ViewModels/SepServiceArea.cs), [SepDrinkSalesFocast.cs](../cllc-public-app/ViewModels/SepDrinkSalesFocast.cs), [SepTermAndCondition.cs](../cllc-public-app/ViewModels/SepTermAndCondition.cs), [SepCity.cs](../cllc-public-app/ViewModels/SepCity.cs) |
| `Application`, `ApplicationExtension` | [Application.cs](../cllc-public-app/ViewModels/Application.cs) |
| `ApplicationType`, `ApplicationSummary` | [ApplicationType.cs](../cllc-public-app/ViewModels/ApplicationType.cs), [ApplicationSummary.cs](../cllc-public-app/ViewModels/ApplicationSummary.cs) |
| `License`, `Endorsement`, `OffsiteStorage`, `CapacityArea`, `HoursOfService` | [License.cs](../cllc-public-app/ViewModels/License.cs), [Endorsement.cs](../cllc-public-app/ViewModels/Endorsement.cs), [OffsiteStorage.cs](../cllc-public-app/ViewModels/OffsiteStorage.cs), [CapacityArea.cs](../cllc-public-app/ViewModels/CapacityArea.cs), [HoursOfService.cs](../cllc-public-app/ViewModels/HoursOfService.cs) |
| `Account`, `Contact`, `User`, `Worker` | [Account.cs](../cllc-public-app/ViewModels/Account.cs), [Contact.cs](../cllc-public-app/ViewModels/Contact.cs), [User.cs](../cllc-public-app/ViewModels/User.cs), [Worker.cs](../cllc-public-app/ViewModels/Worker.cs) |
| `LegalEntity`, `LicenseeChangeLog` | [AdoxioLegalEntity.cs](../cllc-public-app/ViewModels/AdoxioLegalEntity.cs), [LicenseeChangeLog.cs](../cllc-public-app/ViewModels/LicenseeChangeLog.cs) |
| `TiedHouseConnection`, `TiedHouseAssociation` | [TiedHouseConnection.cs](../cllc-public-app/ViewModels/TiedHouseConnection.cs), [TiedHouseAssociation.cs](../cllc-public-app/ViewModels/TiedHouseAssociation.cs) |
| `Invoice`, `MonthlyReport`, `LicenceEvent` | [Invoice.cs](../cllc-public-app/ViewModels/Invoice.cs), [MonthlyReport.cs](../cllc-public-app/ViewModels/MonthlyReport.cs), [LicenceEvent.cs](../cllc-public-app/ViewModels/LicenceEvent.cs) |

## 2. Slice A: Special Event Permits

Prototype sources **[NEW]**: `lib/data/seps.ts`, `lib/data/sep-flow.ts`, `app/special-events/apply/page.tsx` (form field list), `components/providers/sep-store-provider.tsx`.

### 2.1 `SepRecord` (permit record, list and detail views)

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `SepRecord.id` | `SpecialEvent.Id` | Direct | Current: the permit number is a separate field, `SpecialEventPermitNumber`. |
| `eventName` | `SpecialEvent.EventName` | Direct | |
| `eventType` | `SpecialEvent.PrivateOrPublic` (`SEPPublicOrPrivate`: Members, Family, Hobbyist, Anyone) | Weak | New: the record holds a display string, and the form field of the same name is private or public. Current: four audience values, not two. Map: needs a value mapping (private roughly Members and Family, public roughly Anyone). |
| `applicantName` | `SpecialEvent.Applicant` (`Contact` name fields) | Likely | |
| `applicantOrg` | `SpecialEvent.HostOrganizationName` | Likely | Current: `NonProfitName` is another candidate. |
| `applicantPhone` | `SpecialEvent.Applicant.telephone1` | Likely | |
| `applicantEmail` | `SpecialEvent.Applicant.emailaddress1` | Likely | |
| `eventStart` | `SpecialEvent.EventStartDate` | Direct | Current: per-day dates are in `SepEventDates.EventDate`. |
| `eventEnd` | `SpecialEvent.EventEndDate` | Direct | |
| `startTime` | `SepEventDates.EventStart` | Likely | Current: service times are separate fields (`ServiceStart`, `ServiceEnd`). |
| `endTime` | `SepEventDates.EventEnd` | Likely | Current: see `ServiceEnd`. |
| `venueName` | `SepEventLocation.LocationName` | Direct | |
| `address` | `SepEventLocation.EventLocationStreet1` and `EventLocationStreet2` | Direct | Current: event-level `SpecialEventStreet1` and `SpecialEventStreet2` also exist. |
| `city` | `SepEventLocation.EventLocationCity` | Direct | Current: `SpecialEvent.SepCity` (`SepCity.Name`) also holds the city. |
| `jurisdiction` | `SepCity.PoliceJurisdictionName` | Derived | New: a display value. Map: resolve from the selected `SepCity`. Current: `SepCity.LGINName` is the LG/IN. |
| `lat`, `lng` | none | Missing | New: used by the events map. Map: could be derived by geocoding the address ([GeocoderController](../cllc-public-app/Controllers/GeocoderController.cs)). |
| `expectedAttendance` | `SepEventLocation.MaximumNumberOfGuests` | Likely | New: drives the auto-approval threshold. Current: `SpecialEvent.MaximumNumberOfGuests` and `Capacity` also exist. |
| `minorsExpected` | `SepServiceArea.MinorPresent` | Likely | New: a boolean. Current: the minor count is `SepEventLocation.NumberOfMinors`. |
| `serviceType` | `SpecialEvent.ChargingForLiquorReason` and `DrinksIncluded` | Weak | New: selling vs free of charge. Current: `SepDrinksSalesForecast.IsCharging` is a related field. |
| `status` | `SpecialEvent.EventStatus`, `PoliceApproval`, `LcrbApproval` | Likely | Map: needs a translation table, see 2.4. |
| `submittedDate` | `SpecialEvent.DateSubmitted` | Direct | |
| `assignedOfficer` | `SpecialEvent.PoliceDecisionBy` | Likely | Current: set through `POST police/{id}/assign` in [SpecialEventsController](../cllc-public-app/Controllers/SpecialEventsController.cs). |
| `conditions[]` | `SpecialEvent.TermsAndConditions` (`SepTermAndCondition`) | Direct | Map: see 2.2. |
| `history[]` | none | Missing | Map: see 2.3. |
| `description` | `SpecialEvent.SpecialEventDescription` | Direct | |
| `siteplanProvided` | none | Derived | New: a boolean flag. Map: derive from whether a site plan was uploaded to the permit's SharePoint folder. |
| `liquorTypes[]` | `SpecialEvent.DrinksSalesForecasts` (`SepDrinksSalesForecast.DrinkTypeId`) | Weak | New: a flat list of strings. Current: per-drink-type rows, plus the category counts `Beer`, `Wine`, `Spirits`. |
| `estimatedDrinks` | `SpecialEvent.TotalServings` | Likely | |
| `hasSecurity` | none | Missing | Current: `LicenceEvent` has security fields, but that is a different entity (licensee events). |
| `securityStaffCount` | none | Missing | Current: same as `hasSecurity`. |
| `referralReasons[]` | none | Missing | New: output of the prototype's auto-approval rules. |
| `cancelled` (a `SepStatus` value) | `SpecialEvent.CancelReason`, `EventStatus` | Likely | Map: see 2.4. |

### 2.2 `SepCondition`

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `id` | `SepTermAndCondition.Id` | Direct | |
| `text` | `SepTermAndCondition.Content` | Direct | |
| `standard` (preset vs custom) | `SepTermAndCondition.Type` | Weak | New: true for a preset condition, false for one the reviewer added. Current: `Type` is a boolean, but its meaning is unconfirmed. |
| (not in prototype) | `SepTermAndCondition.Originator` | n/a | Current: records who added the condition. The prototype has no equivalent. |

### 2.3 `SepHistoryEntry`

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `date` | none | Missing | Current: there is no per-permit audit trail. The nearest dates are `DateSubmitted`, `DateIssued` and `DateOfPoliceDecision` (on `SpecialEventSummary`). |
| `actor` | `PoliceDecisionBy`, `LcrbApprovalBy` | Weak | Current: only records who made the final decision, not every action. |
| `action` | none | Missing | |
| `note` | `DenialReason`, `CancelReason` | Weak | Current: only populated on deny and cancel. |

### 2.4 Status translation (proposed)

Current enums, from [SpecialEvent.cs](../cllc-public-app/ViewModels/SpecialEvent.cs):

- `EventStatus`: Draft, Submitted, PendingReview, Approved, Issued, Denied, Cancelled.
- `ApproverStatus` (used by `PoliceApproval` and `LcrbApproval`): AutoReviewed, Reviewed, PendingReview, Approved, Denied, Cancelled.

| Prototype `SepStatus` **[NEW]** | Current **[CURRENT]** (best guess) |
|---|---|
| (draft, kept in `ApplicationDraft`) | `EventStatus.Draft` |
| `submitted` | `EventStatus.Submitted`, `PoliceApproval` PendingReview |
| `under-review` | `EventStatus.PendingReview` |
| `changes-requested` | no direct equivalent (Missing) |
| `approved` | `EventStatus.Approved` or `Issued`, `PoliceApproval.Approved` |
| `denied` | `EventStatus.Denied`, `PoliceApproval.Denied`, `DenialReason` set |
| `auto-approved` | `PoliceApproval.AutoReviewed` (Likely: the value exists, but the rule that sets it is not in the portal) |
| `cancelled` | `EventStatus.Cancelled`, `CancelReason` |

Current: `Approved` and `Reviewed` in `ApproverStatus` share the value 845280000. Confirm how the portal distinguishes them.

### 2.5 SEP application form fields (`app/special-events/apply/page.tsx`, `lib/data/sep-flow.ts`)

| Step | Prototype field **[NEW]** (label) | Current field **[CURRENT]** | Match |
|---|---|---|---|
| Applicant | applicant name, phone, email, organization (prefilled from session) | `SpecialEvent.Applicant` (`Contact`), `HostOrganizationName` | Derived |
| Applicant | `eventName` (Event Name) | `SpecialEvent.EventName` | Direct |
| Applicant | `termsAccepted` (Agree to terms and conditions) | `SpecialEvent.IsAgreeToTnC`, `DateAgreeToTnC` | Direct |
| Eligibility | `isPrivateResidence` | `SpecialEvent.IsPrivateResidence` | Direct |
| Eligibility | `isPublicProperty` | `SpecialEvent.IsOnPublicProperty` | Direct |
| Eligibility | `isSignificantEvent` | `SpecialEvent.IsMajorSignificance` | Likely |
| Eligibility | `eventType` (private or public) | `SpecialEvent.PrivateOrPublic` (Members, Family, Hobbyist, Anyone) | Weak |
| Eligibility | `eventStartDate` | `SpecialEvent.EventStartDate` | Direct |
| Eligibility | `hasCertificate` (SIR/SES certificate) | `SpecialEvent.ResponsibleBevServiceNumberDoesNotHave` (inverted) | Likely |
| Eligibility | `certificateNumber` | `SpecialEvent.ResponsibleBevServiceNumber` | Direct |
| Eligibility | `eventOccasion` | `SpecialEvent.SpecialEventDescription` | Likely |
| Eligibility | `hasAdmissionCharge` | `SpecialEvent.AdmissionFee` | Direct |
| Location | `eventCity` | `SpecialEvent.SepCity` (`SepCity.Id`, `Name`) | Direct |
| Location | `isAnnualEvent` | `SpecialEvent.IsAnnualEvent` | Direct |
| Location | `locationName` | `SepEventLocation.LocationName` | Direct |
| Location | `venueType` | `SepEventLocation.LocationDescription` | Weak |
| Location | `addressLine1` | `SepEventLocation.EventLocationStreet1` | Direct |
| Location | `postalCode` | `SepEventLocation.EventLocationPostalCode` | Direct |
| Location | `maxGuests` | `SepEventLocation.MaximumNumberOfGuests` | Direct |
| Location | `eventDate` | `SepEventDates.EventDate` | Direct |
| Location | `eventStartTime` | `SepEventDates.EventStart` (and `ServiceStart`) | Likely |
| Location | `eventEndTime` | `SepEventDates.EventEnd` (and `ServiceEnd`) | Likely |
| Liquor | `liquorTypes` (beer, wine, cider, coolers, spirits) | `SepDrinksSalesForecast` rows by `SepDrinkType`; `Beer`, `Wine`, `Spirits` | Weak |
| Liquor | `liquorSaleType` (selling or free) | `SpecialEvent.ChargingForLiquorReason`, `DrinksIncluded`, `SepDrinksSalesForecast.IsCharging` | Weak |
| Liquor | `estimatedDrinks` | `SpecialEvent.TotalServings` | Likely |
| Liquor | `hasSecurity` | none | Missing |
| Liquor | `securityStaffCount` | none | Missing |
| Summary | `confirmAccuracy` | `SpecialEvent.IsAgreeTsAndCs` | Weak |
| Summary | `acknowledgeFalseInfo` | none | Missing |

Current-only SEP data the prototype form does not capture: fundraising and proceeds (`FundRaisingPurpose`, `TotalRevenue`, `TotalProceeds`, `HowProceedsWillBeUsedDescription`), host organization details, `BeerGarden`, `TastingEvent`, price and cost averages, minors and licensed-area fields on `SepServiceArea`, and `LiquorServiceHoursExtensionReason` and `DisturbancePreventionMeasuresDetails` on `SepEventDates`. These may be required for a full cut-over.

### 2.6 Police and servings calculator

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| Police decision action (`decide`: approved, denied, changes-requested) | `POST police/{id}/approve`, `deny`; `SpecialEvent.PoliceApproval` | Likely | Map: `changes-requested` has no current equivalent. |
| Decision note | `SpecialEvent.DenialReason` | Weak | Current: only used when a permit is denied. Map: approval notes have no field. |
| `ReferralInput`, `ReferralAssessment`, `AUTO_APPROVAL_THRESHOLD`, `REFERRAL_RULES` | none | Missing | New: prototype rules that decide auto-approval. Map: a business-rule decision, not a field mapping. |
| Servings calculator drink types (`lib/data`, servings page) | `SepDrinkType` (`Name`, `ServingSizeMl`, `StorageSizeMl`, `CostPerServing`, `PricePerServing`, `Group`) | Likely | Current: served by `GET drink-types`. |

## 3. Slice B: Licensee applications

Prototype sources **[NEW]**: `lib/data/application-factory.ts`, `applications.ts`, `application-catalog-extended.ts`, `my-applications.ts`, `submitted-applications.ts`, `lib/data/submissions.ts` (`SubmissionRecord`), `drafts.ts`.

### 3.1 `CatalogEntry`, `DocumentSpec`

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `CatalogEntry.slug` | `ApplicationType.Name` | Weak | New: a prototype-defined URL key. Map: needs a slug-to-application-type mapping table. |
| `title` | `ApplicationType.Title` | Direct | |
| `description` | `ApplicationType.ActionText` | Weak | Current: `ActionText` appears to be call-to-action text, not a description. Unconfirmed. |
| `licenceType` (`LicenceType`) | `ApplicationType.LicenseType` (`LicenseType`) | Likely | Current: see [LicenceTypeNames.cs](../cllc-public-app/ViewModels/LicenceTypeNames.cs). |
| `kind` (application or authorization) | `ApplicationType.Category`, `IsEndorsement` | Weak | New: a two-value flag. Current: category is a different classification. |
| `fee` | `ApplicationType.IsFree`; fee amount on the invoice | Weak | New: a display string such as "$330.00". Current: only `IsFree` is on the type, and the amount comes from the invoice. Map: the amount source needs confirming. |
| `config` (`FlowConfig`) | `ApplicationType.DynamicsForm`, `FormReference` | Weak | Map: different design. The prototype defines flows in code, the current portal reads Dynamics forms. |
| `keywords` | none | Out of scope | New: search aid. |
| `DocumentSpec.name`, `label` | document type prefix in [FileController](../cllc-public-app/Controllers/FileController.cs); `DocumentTypeStatus` | Likely | |
| `required` | `adoxio_formelementuploadfield` (read by `documentStatus`) | Likely | |
| `hint`, `criteria`, `docType`, `occupancyField` | none | Out of scope | New: drive the AI document check. |

### 3.2 `MyApplication` (in progress, submitted, completed)

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `id` | `Application.Id` | Direct | |
| `type` | `ApplicationSummary.ApplicationTypeName` | Direct | |
| `reference` | `Application.JobNumber` or `Application.Name` | Likely | New: values look like "LP - LCRBTrainingBusiness5" or "LP-125998". Current: `Name` is the first style, `JobNumber` the second. |
| `status` | `Application.ApplicationStatus` | Likely | Map: see 3.6. |
| `lastModified` | `Application.ModifiedOn` | Direct | |
| `completionPercent` | none | Derived | Map: compute from the form's answered fields. |
| `sections[]` | none | Derived | Map: compute from the form's steps. |
| `captured[]` | none | Derived | Map: compute from saved answers. |
| `submittedDate` | `ApplicationSummary.DateApplicationSubmitted` | Direct | |
| `expectedCompletion` | none | Missing | New: derived from processing-time estimates. |
| `completedDate` | `Application.ModifiedOn` | Weak | Current: `ModifiedOn` is the last-saved timestamp, not a completion date. Map: treat it as the completion date only when the status is Approved or Refused. |
| `APPLICATION_REFERRALS` (LCRB, LG routing) | `ApplicationSummary.DateApplicantSentToLG`, `PoliceJurisdictionId`, `IndigenousNationId` | Weak | New: lists which partners an application was referred to. Current: only shows that a referral to LG, police or an Indigenous Nation exists. |

### 3.3 `SubmittedApplication` (detail view)

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `applicant` | `Application.Applicant.name` | Direct | |
| `reviewNote` | none | Missing | New: a staff note shown to the applicant when documents are pending. |
| `fee[]` (`label`, `amount`) | `Application.LicenceFeeInvoice`, `Invoice.totalamount` | Likely | Current: a primary and a secondary invoice exist (`InvoiceId`, `SecondaryInvoiceId`). Map: line items are not stored, only totals. |
| `currency` | none | Derived | Map: always CAD. |
| `paymentMethod` | none | Missing | |
| `confirmationNumber` | `Invoice.transactionId` or `returnedTransactionId` | Weak | Current: these are payment-gateway ids. Map: unclear whether they are the number shown to the user. |
| `summary[]` | none | Derived | Map: build from the application's fields. |
| `timeline[]` | none | Missing | Map: could be derived from status and dates. |
| `requiredDocuments[]` (`id`, `name`, `reason`, `required`) | `DocumentTypeStatus`; status `Incomplete` or `LicenseeActionRequired` | Weak | New: each item carries a staff-written reason. Current: the document checklist has no free-text reason. |

### 3.4 `SubmissionRecord` (receipt)

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `id`, `confirmationNumber` | `Invoice.id`, `Invoice.transactionId` | Weak | |
| `jobNumber` | `Application.JobNumber` | Direct | |
| `flowId`, `title` | `ApplicationType.Id`, `ApplicationType.Title` | Likely | |
| `applicant` | `Application.Applicant.name` | Direct | |
| `lineItems[]`, `total` | `Invoice.totalamount`, `totaltax` | Likely | Current: totals only, no line items. |
| `currency` | none | Derived | Map: always CAD. |
| `paymentMethod`, `payerEmail` | none | Missing | |
| `receiptNumber` | `Invoice.invoicenumber` | Weak | Current: an invoice number, not necessarily a receipt number. |
| `submittedAt` | `Application.PaymentReceivedDate` or `ApplicationSummary.DateApplicationSubmitted` | Likely | Map: pick one. The prototype treats submit and pay as one step. |
| `summary[]` | none | Derived | |
| `syncStatus` | none | Out of scope | New: simulates a push to Dynamics. The current portal writes to Dynamics directly. |
| `assessment`, `agentLog` | none | Out of scope | New: AI output, staff-only. |

### 3.5 `ApplicationDraft`

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `draftId` | `Application.Id` | Likely | Current: in-progress applications are saved rows in Dynamics. |
| `flowId` | `Application.ApplicationType` | Likely | |
| `title` | `ApplicationType.Title` | Likely | |
| `resumeHref` | none | Derived | |
| `formData` | the application's own fields | Likely | New: a free-form map of answers. Current: typed fields on `Application`. |
| `currentStep` | none | Missing | |
| `progress` | none | Missing | |
| `updatedAt` | `Application.ModifiedOn` | Direct | |

### 3.6 Application status translation (proposed)

| Prototype **[NEW]** (`MyApplicationStatus`, `SubmittedStatus`) | Current `AdoxioApplicationStatusCodes` **[CURRENT]** |
|---|---|
| `not_submitted` | `InProgress` |
| `under_review` | `Submitted`, `Intake`, `UnderReview`, `ApplicationAssessment`, `PendingForLGFNPFeedback`, `PendingForLicenceFee`, `PendingFinalInspection`, `ReviewingInspectionResults` |
| `pending_documents` | `Incomplete`, `LicenseeActionRequired` |
| `approved` | `Approved`, `Processed` |
| `rejected` | `Refused` |
| `cancelled` | `Cancelled`, `Terminated`, `TerminatedAndRefunded` |

### 3.7 Application flow fields (`FlowConfig` fields)

204 unique field names are defined across the catalog files. Most appear in one flow only. Targets are on `Application` unless stated. "File" means an upload handled by [FileController](../cllc-public-app/Controllers/FileController.cs), not a stored field. The notes column here is used only where the table needs one.

#### Shared steps (nearly every flow)

| Prototype field **[NEW]** (label) | Current field **[CURRENT]** | Match |
|---|---|---|
| `__requirementsAck` (read and understand) | none | Missing |
| `legalEntityName` (Legal Entity / Business Name) | `Application.Applicant.name` (`Account.name`) | Derived |
| `bceidBusinessNumber` | `Account.businessNumber` | Derived |
| `licenceNumber` | `Application.AssignedLicence` (`License.LicenseNumber`) | Direct |
| `establishmentName` | `Application.EstablishmentName` (`adoxio_establishmentpropsedname`) | Direct |
| `addressLine1` / `streetAddress` | `Application.EstablishmentAddressStreet` | Direct |
| `addressLine2` | none | Missing |
| `city` | `Application.EstablishmentAddressCity` | Direct |
| `province` | none | Missing |
| `postalCode` | `Application.EstablishmentAddressPostalCode` | Direct |
| `contactName` | `Application.ContactPersonFirstName` and `ContactPersonLastName` | Likely |
| `contactTitle` | `Application.ContactPersonRole` (`adoxio_role`) | Likely |
| `contactPhone` | `Application.ContactPersonPhone` | Direct |
| `contactEmail` | `Application.ContactPersonEmail` (`adoxio_email`) | Direct |
| `firstName`, `lastName`, `email`, `phone`, `title` (contact blocks in newer flows) | `ContactPersonFirstName`, `ContactPersonLastName`, `ContactPersonEmail`, `ContactPersonPhone`, `ContactPersonRole` | Direct |
| `declarationConfirm`, `renewalDeclaration`, `pclDeclaration`, `complianceConfirm` | `Application.SignatureAgreement`, `AuthorizedToSubmit`, `affirmInformationProividedTrueAndComplete` | Likely |
| `supportingDocuments` | File | Derived |

Shared-step notes:

- **New:** `__requirementsAck` is a UI acknowledgement only. `contactName` is one field, and `province` is a dropdown that also offers Alberta and Other.
- **Current:** the establishment address is stored as one street, one city and one postal code on `Application`, with no second-line or province field in the view model. Contact person is split into first and last name.
- **Map:** `contactName` needs to be split into first and last name. The four declaration checkboxes each collapse onto the same few acknowledgement fields.

#### Business identity (new-licence applications)

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match |
|---|---|---|
| `legalName` | `Account.name` | Derived |
| `businessNumber` | `Account.businessNumber` | Derived |
| `businessType`, `entityType` | `Account.businessType` (`AdoxioApplicantTypeCodes`) | Direct |
| `incorporationNumber` | `Account.bcIncorporationNumber` | Direct |
| `foodServiceType` | none | Missing |
| `businessDescription` | `Application.Description1` | Weak |
| `ppeEndorsement`, `cateringEndorsement` | `Endorsement` rows; `ApplicationType.IsEndorsement` | Weak |
| `occupantLoad` | `Establishment.Occupantload` | Likely |
| `serviceAreaCapacity`, `areaCapacity`, `patronCapacity` | `CapacityArea.Capacity` | Direct |
| `areaType` | `CapacityArea.IsIndoor`, `IsOutdoor`, `IsPatio` | Likely |
| `areaDescription` | `CapacityArea.AreaLocation` | Likely |
| `proposedOpenDate` | `Application.Establishmentopeningdate` | Direct |
| `proposedHours`, `currentHours`, `foodServiceHours` | `Application.ServiceHours{Day}Open` and `Close` | Weak |
| `establishmentType`, `commercialKitchen` | none | Missing |

Notes:

- **New:** `businessType` and `entityType` are two separate option sets in the prototype for the same concept. `proposedHours`, `currentHours` and `foodServiceHours` are single free-text values.
- **Current:** `Account` and `Establishment` hold the business and premises identity. Service hours are stored per day of the week (open and close for each of seven days), not as one value.
- **Map:** `ppeEndorsement` and `cateringEndorsement` are checkboxes in the prototype but endorsements in Dynamics. They need to become endorsement requests, not stored booleans.

#### Documents (File uploads)

| Prototype field **[NEW]** | Current counterpart **[CURRENT]** | Match |
|---|---|---|
| `floorPlan` | File; `ApplicationType.FloorPlan` control state | Direct |
| `sitePlan` | File; `ApplicationType.SitePlan` | Direct |
| `proofOfInterest`, `leaseAgreement` | File; `ApplicationType.ValidInterest` | Likely |
| `zoningConfirmation` | File; `ApplicationType.ProofofZoning` | Direct |
| `securitiesRegister`, `corporateRegistry` | File (corporate details documents, see `Account.isCorporateDetailsComplete`) | Likely |
| `certificateOfIncorporation`, `noticeOfArticles`, `incorporationDocument` | File (corporate details) | Likely |
| `letterOfIntent` | File; `ApplicationType.LetterOfIntent` | Direct |
| `servingItRight` | File | Weak |
| `operatorAgreement` | File | Weak |

Notes:

- **New:** each is a file upload field in a flow.
- **Current:** uploads are not application fields. They are files in SharePoint, found by a file-name prefix. `ApplicationType` controls only whether each upload is shown (a `FormControlState`).
- **Map:** the prototype's per-flow upload list must be matched to the current prefix and `ApplicationType` control.

#### Change and temporary-change flows

| Prototype field **[NEW]** (label) | Current field **[CURRENT]** | Match |
|---|---|---|
| `serviceArea` (which area) | `CapacityArea` (`ServiceAreas`, `OutsideAreas`) | Weak |
| `requestedOpen`, `requestedClose` | `Application.ServiceHours{Day}Open` and `Close` (weekly) | Weak |
| `startDate`, `extensionStart` | `Application.TempDateFrom` | Direct |
| `endDate`, `extensionEnd` | `Application.TempDateTo` | Direct |
| `reason`, `suspensionReason`, `extensionReason`, `nameReason`, `relocationReason`, `removalReason`, `rationale`, `changeRequested` | none | Missing |
| `suspensionStart` | `Application.TempSuspensionOrPatronParticipationStart` | Direct |
| `suspensionEnd` | `Application.TempSuspensionOrPatronParticipationEnd` | Direct |
| `extensionArea`, `tuaDescription`, `tuaStart`, `tuaEnd`, `tuaCapacity`, `extensionCapacity` | `CapacityArea` with `IsTemporaryExtensionArea`; `TempDateFrom`, `TempDateTo` | Likely |
| `currentTerm`, `requestedChange` | `Application.TermConditionOriginalText`, `TermConditionId` | Likely |
| `changeDescription`, `areaAffected` | none | Missing |
| `currentCapacity`, `newOccupantLoad` | `Establishment.Occupantload` or `CapacityArea.Capacity` | Weak |
| `proposedCapacity`, `endorsementCapacity` | `CapacityArea.Capacity` | Likely |
| `endorsementHours`, `endorsementDescription`, `endorsementArea`, `entertainmentArea`, `entertainmentTypes` | none | Missing |
| `constructionStart`, `constructionComplete` | `Application.statusOfConstruction` (int) | Weak |
| `changeImpact` | none | Missing |
| `effectiveDate` | none | Missing |
| `permanent` | `ApplicationType.IsEndorsement` | Weak |
| `requestType` (varies by flow) | `Application.Cs*` flags (`CsInternalTransferOfShares`, `CsExternalTransferOfShares`, `CsChangeOfDirectorsOrOfficers`, `CsNameChangeLicensee*`, `CsAdditionalReceiverOrExecutor`, `CsTiedHouseDeclaration`) | Likely |

Notes:

- **New:** each flow has its own free-text "reason" or "describe" fields. `requestedOpen` and `requestedClose` are a single pair of times.
- **Current:** `Application` has no field for these free-text reasons. Temporary dates are on `Application` as `TempDateFrom` and `TempDateTo`. Whether Dynamics captures reasons some other way (for example in form-specific fields or documents) is unconfirmed.
- **Map:** several prototype fields collapse onto one current field (all start dates to `TempDateFrom`). A single free-text reason has no field and needs a decision. `constructionStart` and `constructionComplete` are dates while `statusOfConstruction` is an integer, so this match is weak.

#### Transfer, relocation, name change

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match |
|---|---|---|
| `transferType` | none | Missing |
| `proposedTransfereeName` | `TransferAccount.AccountName` (see [Account.cs](../cllc-public-app/ViewModels/Account.cs)) | Likely |
| `transfereeContactName` | `TransferAccount.ContactName` | Likely |
| `transfereeEmail` | none | Missing |
| `proposedDate`, `proposedMoveDate` | none | Missing |
| `purchasePrice` | none | Missing |
| `newAddressLine1`, `newCity`, `newPostalCode` | `Application.EstablishmentAddressStreet`, `City`, `PostalCode` (new address on a relocation application type) | Likely |
| `currentName` | `License.EstablishmentName` | Direct |
| `proposedName` | `Application.EstablishmentName` | Direct |

Notes:

- **New:** `newAddress*` are separate from the establishment address on the same form.
- **Current:** `TransferAccount` identifies the transferee account and contact. On a relocation the same `EstablishmentAddress*` fields carry the proposed new address, not a second set. The current licence address stays on `License`.
- **Map:** the prototype shows current and new addresses side by side, so the current address must be read from `License` and the new one from `Application`.

#### Patio and outdoor areas

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match |
|---|---|---|
| `patioType` | `Application.LocatedAboveDescription`, `CapacityArea.IsPatio` | Weak |
| `patioCapacity` | `CapacityArea.Capacity` | Likely |
| `patioArea` (sq. metres) | none | Missing |
| `patioAdjacent` | `Application.isBoundingSufficientToDefine`, `IsHasPatio` | Weak |
| `patioDescription` | `Application.PatioCompDescription`, `PatioLocationDescription` | Likely |

Notes:

- **New:** `patioType` is a select of area kinds.
- **Current:** the patio fields were added in several steps (see the LCSD ticket comments in `Application.cs`) and use their own yes/no and description fields. `LocatedAboveDescription` is an integer picklist whose options (`PatioLocatedAbove`) are ground surfaces such as grass, gravel and cement sidewalk, not kinds of area.
- **Map:** `patioType` and `LocatedAboveDescription` are different concepts, so this match is weak.

#### Operator, representative, dormancy, storage

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match |
|---|---|---|
| `operatorAction`, `operatorLegalName`, `operatorContact`, `operatorEmail`, `agreementStart` | Third-party operator on the licence (`LicensesController.set-third-party-operator`, `third-party-operator`); payload not reviewed | Weak |
| `repAction` | `PUT licences/{licenceId}/representative` | Likely |
| `repFirstName`, `repLastName` | `License.RepresentativeFullName` | Likely |
| `repPhone` | `License.RepresentativePhoneNumber` | Direct |
| `repEmail` | `License.RepresentativeEmail` | Direct |
| `repAuthority` (level of authority) | `License.RepresentativeCan*` flags | Likely |
| `repConsent` | none | Missing |
| `dormancyStart` | `Application.DormancyStartDate` | Direct |
| `dormancyEnd` | `Application.DormancyEndDate` | Likely |
| `dormancyReason` | `Application.DormancyReasons`, `DormancyNotes` | Likely |
| `storageAddress`, `storageCity`, `storagePostal` | `OffsiteStorage.Street1`, `City`, `PostalCode` | Direct |
| `storageType` | none | Missing |

Notes:

- **New:** `repAuthority` is a single select (level of authority). `dormancyEnd` is an "expected reactivation date".
- **Current:** the representative is a set of yes/no permission flags on the licence (can sign permanent changes, can sign temporary changes, and so on), not one access level. `DormancyEndDate` may be the actual end rather than an expected date; this is unconfirmed.
- **Map:** `repAuthority` has to be expanded into the individual `RepresentativeCan*` flags.

#### Indigenous nation, tied house, renewal

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match |
|---|---|---|
| `nationName` | `Application.IndigenousNation` (`IndigenousNation.Name`) | Direct |
| `relationship` | `TiedHouseConnection.RelationshipToLicence` | Weak |
| `associatedParty`, `associatedManufacturer` | `TiedHouseConnection.LegalEntityName` | Likely |
| `associationType`, `exemptionDetails` | `TiedHouseConnection.OtherDescription` | Weak |
| `infoStatus` (details still accurate) | `Application.Renewal*` (`ValueNotChanged` fields) | Likely |
| `changesNote` | `Application.Description1` | Weak |

Notes:

- **New:** `infoStatus` is one yes/no question for the whole renewal.
- **Current:** renewal asks about 20 separate "has this changed" questions (`RenewalBranding`, `RenewalSignage`, `RenewalZoning`, and so on), each a `ValueNotChanged` value.
- **Map:** one prototype question corresponds to many current answers. Either keep the 20 questions or set them all from the one answer. This needs a product decision.

#### Permanent change to licensee (PCL)

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match |
|---|---|---|
| `directorChanges`, `officerChanges`, `shareholderChanges` (free text) | `LicenseeChangeLog` tree and `LegalEntity` (structured) | Weak |
| `tiedHouseParties` | `TiedHouseConnection` | Weak |
| `otherRequest` | none | Missing |
| `screeningConsent` | `Worker.ConsentToSecurityScreening` (per person) | Weak |

Notes:

- **New:** the three change fields are free-text boxes. `screeningConsent` is a single checkbox.
- **Current:** changes are recorded as a structured tree (`LicenseeChangeLog`, with old and new values per person and role). Consent is recorded per individual.
- **Map:** free text cannot be turned into structured rows automatically. Either the legal entity builder replaces these boxes or staff re-key the data.

#### Licensee events (catered events, temporary use, all-ages events)

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match |
|---|---|---|
| `eventName` | `LicenceEvent.EventName` | Direct |
| `eventDate` | `LicenceEvent.StartDate` | Direct |
| `eventStart`, `eventEnd` | `LicenceEventSchedule` (via `LicenceEvent.Schedules`) | Likely |
| `eventAddress` | `LicenceEvent.Street1` | Likely |
| `expectedGuests`, `expectedAttendance` | `LicenceEvent.MaxAttendance` | Likely |
| `minorsPresent` | `LicenceEvent.MinorsAttending` | Direct |
| `liquorTypes` | none | Missing |
| `eventDescription` | `LicenceEvent.EventTypeDescription` | Weak |
| `serviceAreas`, `minorsControls`, `minorControls`, `minorAreas`, `minorHours`, `facilityType`, `theatreName`, `screeningTypes` | none | Missing |

Notes:

- **New:** the prototype uses one flat set of event fields.
- **Current:** [LicenceEvent](../cllc-public-app/ViewModels/LicenceEvent.cs) is a licensee event record (not a Special Event Permit) with a large set of safety, security and staffing fields, plus child locations and schedules.
- **Map:** it is a best-guess target. Confirm which application types actually create a `LicenceEvent` rather than an `Application`.

#### Cannabis retail store and licensee retail store

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match |
|---|---|---|
| `storeName` | `Application.EstablishmentName` | Direct |
| `storeAddress`, `storeCity`, `storePostal` | `Application.EstablishmentAddressStreet`, `City`, `PostalCode` | Direct |
| `localGovSupport`, `localGovApproval` | `Application.LGApprovalDecision`, `LgNoObjection`, `ResolutionDocsUploaded` | Likely |
| `associatesIdentified`, `numAssociates` | `LegalEntity` rows; `BusinessProfileCompletion.isSecurityAssessmentComplete` | Weak |
| `fundingSourcesDisclosed` | `BusinessProfileCompletion.isFinanceIntegrityComplete` | Weak |
| `storeFloorArea` | none | Missing |
| `distanceToNearestStore` | `Application.IsRlrsAtLeast10kmFromAnotherStore` (yes/no) | Weak |
| `communityPopulation` | `Application.RlrsResortCommunityDescription` | Weak |
| `reportMonth` | `MonthlyReport.reportingPeriodMonth` | Direct |
| `openingInventory`, `unitsSold`, `closingInventory` | `MonthlyReport.inventorySalesReports` (`InventorySalesReport`) | Likely |

Notes:

- **New:** `distanceToNearestStore` and `communityPopulation` are numbers.
- **Current:** the equivalent location-eligibility fields are yes/no answers stored as integer picklist values (`IsRlrsAtLeast10kmFromAnotherStore`) or free-text descriptions. The "associates identified" and "funding disclosed" answers are completion flags on the business profile, not application fields.
- **Map:** the numbers must be reduced to yes/no, or new numeric fields added.

#### Manufacturers, agents, catering, UBrew

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match |
|---|---|---|
| `manufacturerType` | `Application.LicenceSubCategory` (`LicenceSubCategory`: winery, brewery, distillery, co-packer) | Likely |
| `facilityAddress` | `Application.EstablishmentAddressStreet` | Likely |
| `annualProduction`, `estimatedVolume` | `Application.ManufacturerProductionAmountForPrevYear`, `ManufacturerProductionAmountUnit` | Weak |
| `onSiteStore`, `lounge` | `Endorsement` rows; `Application.RelocateOnSiteStore` | Weak |
| `hostManufacturer`, `hostLicenceNumber` | `Application.RelatedLicenceNumber` | Weak |
| `productTypes` | `Application.ProductsListAndDescription` | Weak |
| `manufacturersRepresented` | none | Missing |
| `cateringRegions`, `expectedEvents`, `serviceDescription` | none | Missing |
| `clubType`, `membershipCriteria` | none | Missing |
| `membershipSize` | `LegalEntity.NumberOfMembers` | Likely |
| `equipmentDescription`, `stationCount` | none | Missing |

Notes:

- **New:** `annualProduction` is a number in litres. `productTypes` is a checkbox list.
- **Current:** production amount is for the previous year with a separate unit field, not an estimate. `ProductsListAndDescription` is free text. `NumberOfMembers` is on the legal entity, not the application.
- **Map:** several of these (production, product list) only roughly match in meaning. Confirm with licensing.

## 4. Slice C: Licences and endorsements

Prototype sources **[NEW]**: `lib/data/licences.ts`, `lib/data/business-profile.ts`, `lib/data/section-119.ts`.

### 4.1 `Licence`

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `id` | `License.Id` and `License.LicenseNumber` | Likely | New: the prototype uses the six-digit licence number as the id. Current: `Id` is a separate string from the licence number (`LicenseNumber`). |
| `type` | `License.LicenseType` | Direct | |
| `name` | `License.EstablishmentName` | Direct | |
| `status` | `License.LicenseStatus` | Direct | Map: the value set (`LicenceStatusCodes`) differs from the prototype strings and needs a translation table. |
| `expiryDate` | `License.ExpiryDate` | Direct | |
| `address` | `License.EstablishmentAddress` (also `...Street`, `...City`, `...PostalCode`) | Direct | New: one combined string. Current: also stored in parts. |
| `phone` | `License.EstablishmentPhone` | Direct | |
| `email` | `License.EstablishmentEmail` | Direct | |
| `operatedBy` | none | Missing | Current: the third-party operator relationship lives elsewhere (see `LicensesController.third-party-operator`). |
| `licencedTo` | none | Missing | Current: the licensee is the `Account` that owns the licence (`licencee/{licenceeId}`). |
| `flags[]` | none | Missing | New: examples are "Transfer Requested" and "Renewal Due". Map: derive from transfer state and expiry. |
| `category` (owned or operated) | none | Derived | New: separates licences the user owns from ones they operate. |
| `endorsements[]` | `License.Endorsements` | Direct | Map: see 4.2. |

### 4.2 `Endorsement`

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `name` | `Endorsement.EndorsementName` | Direct | |
| `kind` (permanent or temporary) | `CapacityArea.IsTemporaryExtensionArea` (via `EndorsementId`) | Weak | Current: the temporary flag is on the capacity area, not the endorsement. |
| `granted` | none | Missing | |
| `expires` | none | Missing | New: set only for temporary endorsements. |
| `summary` | none | Missing | New: static text per endorsement type. |
| (not in prototype) | `Endorsement.HoursOfServiceList`, `AreaCapacity`, `ApplicationTypeId` | n/a | Current: hours and capacity the endorsement allows. The prototype shows only a text summary. |

### 4.3 `BusinessProfile`, `BusinessLicence`, `FirstNationProfile`

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `BusinessProfile.legalName` | `Account.name` | Direct | |
| `bceidBusinessNumber` | `Account.businessNumber` (`accountnumber`) | Direct | |
| `legalEntityType` | `Account.businessType` | Direct | New: a canonical string such as "corporation". Map: needs translating to `AdoxioApplicantTypeCodes`. |
| `incorporationNumber` | `Account.bcIncorporationNumber` | Direct | |
| `licences[]` | `License` list from `LicensesController.current` | Direct | |
| `BusinessLicence.number` | `License.LicenseNumber` | Direct | |
| `establishmentName` | `License.EstablishmentName` | Direct | |
| `type` | `License.LicenseType` | Direct | |
| `address` | `License.EstablishmentAddress` | Direct | |
| `firstNation` | `Application.IndigenousNation` | Weak | New: held once per business. Current: chosen per application. |
| `FirstNationProfile.affiliated` | none | Missing | New: self-declared. |
| `nationName` | `IndigenousNation.Name` | Likely | |
| `agreementStatus`, `agreementReference`, `agreementDate` | none | Missing | New: section 119 agreement data. |
| `premisesOnNationLands` | `Establishment.Locatedonfirstnationland`, `Application.IsOnINLand` | Direct | |
| `LicenceHoldings.liquor`, `cannabis` | none | Derived | Map: count licences by type. |

## 5. Slice D: Business structure

### 5.1 Legal entity builder (`lib/data/legal-entity.ts`)

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `LegalEntityBuild.businessLegalName` | `Account.name` | Derived | |
| `businessNumber` | `Account.businessNumber` | Derived | |
| `status` (`LEBuildStatus`) | `LicenseeChangeLog.Statuscode` and the LE review `Application.ApplicationStatus` | Weak | New: draft, submitted, under-review, changes-requested, approved. Current: status is split across two records. |
| `lockedByLCRB` | none | Missing | New: makes the build read-only in the portal. |
| `version` | none | Missing | |
| `nodes[]` | `LegalEntity` tree (`children`) | Direct | |
| `edges[]` | `LegalEntity.parentLegalEntityId`, `shareholderAccountId`; `adoxio_leconnection` | Likely | Map: see the `LEEdge` table below. |
| `suggestions[]` (`LESuggestion`) | `LicenseeChangeLog` rows (new and old values per field) | Likely | Current: the closest existing model for proposed changes. |
| `reviewNotes[]` (`LEReviewNote`) | none | Missing | |
| `lastSyncedAt` | none | Out of scope | New: simulates a Dynamics sync. |
| `dynamicsCaseId` | `LicenseeChangeLog.ApplicationId`, `Application.JobNumber` | Weak | |
| `updatedAt` | none | Missing | |

`LENode`:

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `id` | `LegalEntity.id` | Direct | |
| `type` (`LEEntityType`) | `LegalEntity.legalentitytype` (`AdoxioApplicantTypeCodes`) | Direct | Map: `individual` is `isindividual`, and `applicant` is `isApplicant`. |
| `legalName` | `LegalEntity.name`, or `firstname`, `middlename`, `lastname` for individuals | Direct | |
| `businessNumber` | `LegalEntity.account` (`Account.businessNumber`) | Likely | New: held on the card. Current: held on the linked account, not the legal entity row. |
| `incorporationNumber` | `LegalEntity.account` (`Account.bcIncorporationNumber`) | Likely | Current: same as `businessNumber`. |
| `jurisdiction` | none | Missing | |
| `linkedLicenceIds[]` | none | Derived | Map: derive from the account's licences. |
| `people[]` (`LEPerson`) | child `LegalEntity` rows for individuals | Direct | |
| `position` (x, y) | none | Missing | New: presentation only. |
| `source` (record or portal), `locked` | none | Derived | New: marks whether a card came from Dynamics. |
| `note` | none | Missing | |
| `external` ("tied-house") | `TiedHouseConnection` | Derived | New: a read-only overlay from the tied house declarations. |

`LEPerson`:

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `id` | `LegalEntity.id` | Direct | |
| `name` | `firstname`, `middlename`, `lastname` | Direct | |
| `role` director | `LegalEntity.isDirector` | Direct | Current: roles are separate boolean flags, so one person can hold several. |
| `role` shareholder | `isShareholder` | Direct | |
| `role` key-personnel | `isKeyPersonnel` | Direct | |
| `role` partner | `isPartner` | Direct | |
| `role` trustee | `IsTrustee` | Direct | |
| `role` officer | `isOfficer` | Direct | |
| `role` beneficiary | none | Missing | |
| `locked` | none | Derived | |

`LEEdge`:

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `id` | none | Derived | |
| `source`, `target` | `LegalEntity.parentLegalEntityId`, `shareholderAccountId` | Likely | |
| `type`: shareholding, partnership, trustee, director, key-personnel | role flags on the child `LegalEntity` | Likely | New: a typed relationship between two nodes. Current: a role flag on the child, so the type is implied. |
| `type`: family, business-agreement, dba, connection | `adoxio_leconnection` (`adoxio_leconnectiontypes`) | Weak | Map: confirm which option values exist. |
| `sharePercent` | `LegalEntity.interestpercentage` | Direct | Current: share counts are in `commonvotingshares`, `preferredvotingshares`, `commonnonvotingshares`, `preferrednonvotingshares`. |
| `label` | none | Missing | |
| `locked` | none | Derived | |

`LESuggestion` and `LEReviewNote`:

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `LESuggestion.id`, `at`, `actor` | `LicenseeChangeLog.Id`; created-on and created-by | Weak | Current: the view model has no created-on or created-by field. They would come from Dynamics system columns. |
| `LESuggestion.summary`, `kind` | `LicenseeChangeLog.ChangeType` plus the `*New` and `*Old` fields | Likely | New: a text summary. Current: structured old and new values. |
| `LEReviewNote.*` (`id`, `at`, `actor`, `decision`, `message`) | none | Missing | |

### 5.2 Tied house (`lib/data/tied-house.ts`)

`TiedHouseDeclaration`:

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `id` | `TiedHouseConnection.id` | Direct | |
| `type` (individual or legal-entity) | `TiedHouseConnection.LIQTiedHouseType` | Likely | |
| `status` (draft or saved) | `TiedHouseConnection.StatusCode` (`TiedHouseStatusCode`: New, Ready, Existing, Inactive) | Weak | New: the status of one declaration while editing. Current: `New` and `Ready` are the nearest. |
| `changeType` (add, change, remove) | `StatusCode`, `SupersededById`, `MarkedForRemoval` | Likely | Map: a change is a new row that supersedes the old one. |
| `individual.dateOfBirth` | `TiedHouseConnection.DateOfBirth` | Direct | |
| `individual.firstName`, `middleName`, `lastName` | `FirstName`, `MiddleName`, `LastName` | Direct | |
| `individual.relationship` | `RelationshipToLicence` | Direct | |
| `individual.relationshipOther` | `OtherDescription` | Direct | |
| `entity.businessType` | `BusinessType` | Direct | |
| `entity.legalEntityName` | `LegalEntityName` | Direct | |
| `entity.relationshipType` | `RelationshipToLicense` | Direct | |
| `entity.relationshipOther` | `OtherRelationship` or `OtherDescription` | Likely | Current: two similar fields exist. Map: pick one. |
| `associatedLicenceNumber` | `AssociatedLiquorLicense` (`RelatedLicence`) and [TiedHouseAssociation](../cllc-public-app/ViewModels/TiedHouseAssociation.cs)`.Licencenumber` | Likely | |
| `associatedEstablishmentName` | `TiedHouseAssociation.Businessname` or `Name` | Likely | |
| `domain` (liquor or cannabis) | `TiedHouseConnection.CategoryType` | Direct | |
| `requiredDocs[]` | none | Derived | New: computed from the declaration type and domain. |
| `dateTied` | `TiedHouseConnection.DeclarationDate` | Likely | New: the date the connection began. Current: may be the date the declaration was made. |
| `createdAt` | none | Missing | |

`TiedHouseApplication`, `ExistingRelationship`, fees:

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `TiedHouseApplication.businessLegalName`, `businessNumber` | `Account.name`, `businessNumber` | Derived | |
| `source` (le-review, voluntary, new-application) | `ApplicationExtension.RelatedLeOrPclApplicationId` for le-review | Weak | Current: only the le-review case has a link. |
| `jobNumber` | `Application.JobNumber` | Likely | New: shown on the fee panel as the reference. |
| `status` (draft, awaiting-payment, submitted) | `Application.ApplicationStatus`, `IsPaid` | Weak | |
| `declarations[]` | `Application.TiedHouseConnections` | Direct | |
| `existingRelationships[]` | `TiedHouseConnection` with `StatusCode` Existing | Direct | |
| `liquorReceipt`, `cannabisReceipt` | `Invoice` primary and secondary (`InvoiceId`, `SecondaryInvoiceId`) | Weak | Map: split-stream payment needs confirming. |
| `submittedAt`, `createdAt`, `updatedAt` | `ApplicationSummary.DateApplicationSubmitted`, `Application.CreatedOn`, `ModifiedOn` | Likely | |
| `ExistingRelationship.displayName` | `FirstName`, `LastName`, `DateOfBirth` or `LegalEntityName` | Derived | |
| `ExistingRelationship.relationshipLabel`, `associatedLicenceNumber`, `associatedEstablishmentName`, `domain`, `dateTied` | same as the declaration | Likely | |
| `FeeLine`, `FeeBreakdown`, `FEE_RATE` | `Invoice.totalamount` | Weak | New: fees are computed in the prototype. Current: fees come from Dynamics and are stored as invoice totals. |

### 5.3 Security screening (`lib/services/screening-store.ts`)

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `ScreeningPerson.id` | `Contact.id` or `Worker.id` | Likely | |
| `name` | `Contact.firstname`, `lastname` | Direct | |
| `role` | `LegalEntity` role flags, `jobTitle` | Likely | |
| `email` | `Contact.emailaddress1` | Direct | |
| `category` (liquor or cannabis) | `Worker.isldbworker` (liquor); cannabis is by form | Weak | |
| `formType` | none | Derived | New: PHS or CAS. |
| `status` (`not_started`, `invited`, `submitted`, `synced`) | `Contact.PhsComplete`, `CasComplete`, `PhsDateSubmitted`, `CasDateSubmitted`; `LegalEntity.securityAssessmentEmailSentOn` | Likely | Map: `invited` comes from the email-sent date, `submitted` from the date-submitted fields. |
| `invitedAt` | `LegalEntity.securityAssessmentEmailSentOn` | Likely | |
| `submittedAt` | `Contact.PhsDateSubmitted` or `CasDateSubmitted` | Direct | |
| `syncedAt`, `dynamicsCaseId` | none | Out of scope | New: simulates a Dynamics sync. |

## 6. Slice E: Local and Indigenous government review

Prototype sources **[NEW]**: `lib/data/lg-applications.ts`, `lg-commenting.ts`, `components/providers/lg-store-provider.tsx`.

### 6.1 `LgApplication`

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `jobNumber` | `Application.JobNumber` | Direct | |
| `establishmentName` | `Application.EstablishmentName` | Direct | |
| `address` | `Application.EstablishmentAddressStreet` | Direct | |
| `city` | `Application.EstablishmentAddressCity` | Direct | |
| `postalCode` | `Application.EstablishmentAddressPostalCode` | Direct | |
| `applicantName` | `Application.ApplyingPerson` or `ContactPersonFirstName` and `LastName` | Likely | |
| `businessName` | `Application.Applicant.name` | Direct | |
| `licenceType` | `Application.LicenseType` | Direct | |
| `applicationType` | `ApplicationSummary.ApplicationTypeName` | Direct | |
| `submittedDate` | `ApplicationSummary.DateApplicationSubmitted` | Direct | |
| `commentDueDate` | none | Derived | New: replaces the 90-day clock. Map: `DateApplicantSentToLG` plus the hold period. |
| `status` (`LgReviewStatus`) | `Application.LGApprovalDecision` (`LGDecision`: Approved, OptOut, Rejected, Pending) | Weak | Map: see 6.2. |
| `jurisdiction` | `Application.LgInName`, `IndigenousNation`, `PoliceJurisdiction` | Likely | |
| `riskLevel` | none | Missing | |
| `contraventions`, `incidents` | none | Missing | New: compliance history counts. Map: see companion doc section 9. |
| `description` | `Application.Description1` | Weak | |
| `hours[]` | `Application.ServiceHours{Day}Open` and `Close` | Direct | New: a day and a text range per row. Current: open and close values per day, stored as enum times. |
| `contacts[]` (`LgContact`) | `Application.ContactPerson*` | Likely | |
| `documents[]` (`LgDocument`) | file listing (`FileController`) | Likely | |
| `history[]` (`LgHistoryEntry`) | none | Missing | |
| `lat`, `lng` | none | Derived | Map: geocode the address. |
| `cancellation` | `Application.ApplicationStatus` Cancelled | Weak | Current: only the status is recorded. `priorStatus` and `reason` have no field. |

### 6.2 LG decisions

| Prototype `LgDecision` **[NEW]** | Current **[CURRENT]** (best guess) |
|---|---|
| `no-comment` | `LGDecision.OptOut` |
| `community-review` | `LGDecision.Approved` (weak: unclear whether "community review" means approval) |
| `reject` | `LGDecision.Rejected` |
| `documents-requested` | none (Missing) |
| (pending) | `LGDecision.Pending` |

Current decision details: `LGDecisionComments`, `LGNameOfOfficial`, `LGTitlePosition`, `LGContactPhone`, `LGContactEmail`, `LGDecisionSubmissionDate`. The prototype's `recordDecision(jobNumber, decision, note, actor)` maps onto these.

### 6.3 `LgCommentingPreference`

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `optedOut` | none on `adoxio_localgovindigenousnation` | Missing | New: jurisdiction-wide opt-out of commenting. Current: only a per-application `OptOut` decision exists. |
| `updatedAt`, `updatedBy` | none | Missing | |
| `effectiveFrom` | none | Missing | New: grandfathers referrals received before this date. |
| `COMMENT_HOLD_DAYS` | none | Missing | New: a prototype constant (30 days). Map: a business-rule decision. |

## 7. Slice F: Account, users and notifications

Prototype sources **[NEW]**: `lib/data/account-settings.ts`, `app/account/page.tsx`.

| Prototype field **[NEW]** | Current field **[CURRENT]** | Match | Notes |
|---|---|---|---|
| `AccountConfig.org.fields[]` (business name, number, type) | `Account.name`, `businessNumber`, `businessType`, `bcIncorporationNumber` | Direct | |
| `org.addresses[]` | `Account.mailingAddress*`, `Account.physicalAddress*` | Direct | |
| `identity.username` | `User.name`, `businessname` | Likely | |
| `identity` (BCeID) | `User.isBceidConfirmed` | Likely | |
| Profile (name, email, phone) | `Contact.firstname`, `lastname`, `emailaddress1`, `telephone1`; `User.firstname`, `lastname`, `email` | Direct | |
| `Delegate.name` | `Contact.name` | Likely | |
| `Delegate.email` | `Contact.emailaddress1` | Likely | |
| `Delegate.role` / access level (`full-access`, `view-only`, `submit-only`) | `Contact.adoxio_cansignpermanentchangeapplications`, `adoxio_cansigntemporarychangeapplications` | Weak | New: one access level per delegate. Current: several yes/no signing permissions. Map: different concepts, so no clean translation. |
| Delegate invite and revoke | `AccountsController.current/contacts`, `ContactController` | Weak | |
| `NotificationTypeDef`, per-user email and SMS settings | none | Missing | |
| `isAccountOwner` | `User.UserType` | Weak | New: set to true by default in the prototype. Current: unconfirmed whether `UserType` marks the account owner. |
| `showCommenting`, LG commenting opt-out | see 6.3 | Missing | |
| `showSepAutoApproval` | none | Out of scope | |
| Police agency user | `User.isPoliceRepresentative`, `PoliceJurisdiction.id`, `name` | Likely | |

## 8. Fields with no current counterpart: summary

Candidates for new fields or new entities, or decisions to drop.

| Area | Missing prototype data |
|---|---|
| SEP | `hasSecurity`, `securityStaffCount`, `acknowledgeFalseInfo`, `lat` and `lng`, `history[]`, `referralReasons[]`, changes-requested state, auto-approval rules (the `AutoReviewed` status value exists, the rule does not) |
| Applications | `currentStep`, `progress`, `expectedCompletion`, `reviewNote`, `paymentMethod`, `payerEmail`, `timeline[]`, free-text change reasons, `addressLine2`, `province`, transfer details (`transferType`, `transfereeEmail`, `purchasePrice`, `proposedDate`), `patioArea`, `storageType`, `repConsent`, catering, club and UBrew fields |
| Licences | `operatedBy`, `licencedTo`, `flags[]`, endorsement `granted`, `expires`, `kind` and `summary`, First Nation `affiliated`, `agreementStatus`, `agreementReference`, `agreementDate` |
| Legal entity | `lockedByLCRB`, `version`, `reviewNotes[]`, node `jurisdiction`, `note`, `position`, edge `label`, `beneficiary` role |
| LG review | `riskLevel`, `contraventions`, `incidents`, `history[]`, `documents-requested` decision, commenting opt-out and its audit |
| Account | notification preferences, delegate access levels (Full, View-only, Submit-only) |

## 9. Next steps

1. Confirm "Weak" and "Likely" rows with the Dynamics team, starting with slice A.
2. Read the actual enum values for `LicenceStatusCodes` to finalise 4.1. The SEP table (2.4) and application table (3.6) already use the real enum values. `LGDecision` values are listed in 6.2.
3. Decide each Missing item: add to Dynamics, keep portal-side, or drop. Record decisions in the companion doc.

# Azure Document Storage: SharePoint Discovery and Draft Design Inputs

Draft input for technical deliverable T02 ("Design the Azure document solution and integration contract", tasks 2.15, 4.1, 4.2, 4.3).

Scope: this repository only. Dynamics-side customisations (plugins, workflows, native SharePoint document management settings) are not in the repo and are listed as items to confirm.

## 1. Current-state inventory

### 1.1 Components that touch SharePoint

| Component | Role | Key files |
|---|---|---|
| Portal API (`cllc-public-app`) | Upload, download, list and delete endpoints, plus access checks. Creates folders and document locations at login and record creation. Stores generated PDFs. | [FileController.cs](../cllc-public-app/Controllers/FileController.cs), [FileManagerClient.cs](../cllc-public-app/Extensions/FileManagerClient.cs), [SiteminderAuthenticationHandler.cs](../cllc-public-app/Authentication/SiteminderAuthenticationHandler.cs) |
| Portal UI (Angular) | Shared upload component and file-name prefix parsing. | [file-uploader.component.ts](../cllc-public-app/ClientApp/src/app/shared/components/file-uploader/file-uploader.component.ts), [file-data.service.ts](../cllc-public-app/ClientApp/src/app/services/file-data.service.ts), [file-utils.ts](../cllc-public-app/ClientApp/src/app/shared/file-utils.ts) |
| File Manager Service (`file-manager-service`) | The only service that talks to SharePoint. Exposes a gRPC API ([fileManager.proto](../file-manager-service/Protos/fileManager.proto)) and an HTTP wrapper. | [FileManagerService.cs](../file-manager-service/Services/FileManagerService.cs), [FileManagerService.extension.cs](../file-manager-service/Services/FileManagerService.extension.cs) |
| SharePoint library (`cllc-interfaces/SharePoint`) | `ISharePointFileManager` with two implementations: on-prem REST and cloud Microsoft Graph. A factory picks one using `SHAREPOINT_TYPE`. | [ISharePointFileManager.cs](../cllc-interfaces/SharePoint/ISharePointFileManager.cs), [SharePointFileManager.cs](../cllc-interfaces/SharePoint/SharePointFileManager.cs), [CloudSharePointFileManager.cs](../cllc-interfaces/SharePoint/CloudSharePointFileManager.cs), [OnPremSharePointFileManager.cs](../cllc-interfaces/SharePoint/OnPremSharePointFileManager.cs), [SharePointConstants.cs](../cllc-interfaces/SharePoint/SharePointConstants.cs) |
| Dataverse client | Reads and writes `sharepointdocumentlocation` link records. Derives folder names. | [DataverseClient.cs](../cllc-interfaces/Dynamics-Dataverse/DataverseClient.cs) |
| Federal Reporting Service | Writes the monthly CSV export into SharePoint through File Manager. Also creates the document location. | [FederalReportingController.cs](../federal-reporting-service/FederalReportingController.cs) |
| SharePoint Sync Tool | One-off repair tool. Finds SharePoint folders with no Dynamics document location and creates the missing records. | [README.md](../sharepoint-sync-tool/README.md), [SyncService.cs](../sharepoint-sync-tool/SyncService.cs) |
| SPICE sync service | Registers `ISharePointFileManager` in [Startup.cs](../carla-spice-sync-service/Startup.cs), but no calls to it were found. Likely dead wiring; confirm. | |
| Watchdog | Health-checks the File Manager service. | |

### 1.2 How Dynamics is linked to the files

- Dynamics holds a `sharepointdocumentlocation` record for each folder. Its `regardingobjectid` points to the business record and `relativeurl` holds the folder name. Its parent is a library-level location (for example `adoxio_application`) under the "Default Site" `sharepointsite`.
- Files are not stored in Dynamics. The link is the folder name. See `GetFolderNameAsync` and `CreateEntitySharePointDocumentLocationAsync` in [DataverseClient.cs](../cllc-interfaces/Dynamics-Dataverse/DataverseClient.cs).
- Folder naming:
  - Most entities use `{Name}_{GUID uppercase, no dashes}`.
  - Contacts use `contact_{GUID}`.
  - Characters SharePoint rejects are replaced with `-`.
- One library per entity type, listed in [SharePointConstants.cs](../cllc-interfaces/SharePoint/SharePointConstants.cs). Active in portal code: account, application, contact, worker, event, licence, special event, federal report export.
- `SharePointConstants` also defines libraries for enforcement action, complaint, contravention, investigation, endorsement, legal entity (association), establishment incident, incident (inspection) and more. Staff probably use these in Dynamics directly.
- Contraventions, enforcement actions and incidents are nested under the Account library (per the sync tool README).
- Dynamics entities with document-location relationships include account, contact, application, worker, licence, event, special event, endorsement, LDB order, federal report export, personal history summary, lead and list (see `cllc-interfaces/Dynamics-Dataverse/Generated/Entities`).
- Portal upload behaviour Dynamics relies on: uploads set `adoxio_FileUploadedFromPortal = Yes` on the application and bump `modifiedon`. A code comment says this triggers a Dynamics workflow that clears "incompleteness". See `UpdateEntityModifiedOnDate` in [FileController.cs](../cllc-public-app/Controllers/FileController.cs). Downloads deliberately do not set it.
- Document-type completeness checks: `GET /api/file/{entity}/{id}/documentStatus/{formId}` reads `adoxio_formelementuploadfield` and checks whether a file with each prefix exists.

### 1.3 Operations the code uses (surface a replacement must cover)

| Operation | Where | Notes |
|---|---|---|
| Create folder (idempotent) | `CreateFolder` | Auto-creates the library if missing. Tolerates a 403 as "already exists". |
| Upload | `UploadFile` | Stores the file as `{documentType}__{sanitizedName}`. Large files use a Graph upload session above 4 MB. |
| List files in folder | `FolderFiles` | Filters by filename prefix. Returns name, size, created and modified dates, `serverRelativeUrl`. |
| Download | `DownloadFile` | Addressed by `serverRelativeUrl`. |
| Delete | `DeleteFile` | Addressed by `serverRelativeUrl`. |
| File exists | `FileExists` | Compares `serverRelativeUrl` after URL-decoding. |
| Truncated filename | `GetTruncatedFilename` | Works around SharePoint path-length limits (160 on-prem, 260 cloud, 128 per segment). |
| Nested folder path, find folder, upload with path | `EnsureFolderPath`, `FindFolder`, `UploadFileWithFolderPath` | Added for the period when cloud Dynamics talked to on-prem SharePoint. |
| Delete folder, rename file, list folders modified after a date | `DeleteFolder`, `RenameFile`, `GetFoldersInDocumentLibraryAfterDate` | Used by the sync tool and legacy code. |

### 1.4 Behaviours the app depends on (implicit contract)

- **File type metadata is encoded in the filename.** The prefix before `__` is the document type. The UI strips it with `parseFileName`; completeness checks and filtering depend on it. There is no real metadata column.
- **A file is identified by its `serverRelativeUrl`.** This is a SharePoint path, used by the UI, delete and download. It is not a stable ID.
- **Uploads accept only PNG, JPEG and PDF**, checked by content sniffing in `UploadAttachmentInternal`. File names are sanitised.
- **Whole files are held in memory.** The portal reads the file into a `MemoryStream` and sends it as one gRPC `bytes` message. The request size limit is disabled.
- **Downloads return `application/octet-stream`.**
- **Federal report CSVs** use `UploadFile` with entity `federal_report`; the library name for that entity is hardcoded.
- **Folder existence is checked before every upload**, and the document location is created if missing.
- **Generated PDFs are uploaded "if changed"** (`UploadPdfIfChanged` compares a hash). Document types include `Licence`, `EventAuthorization`, `WorkerQualification` and special-event PDFs.
- **Stub mode.** `DISABLE_SHAREPOINT_INTEGRATION=true` makes File Manager return fake success and a fake file listing. Local dev depends on this.

### 1.5 Authorisation and identity flows

- **User to portal.** Siteminder or BCeID login. `UserSettings` holds account and contact IDs.
- **Portal access checks.** `CanAccessEntity` in [FileController.cs](../cllc-public-app/Controllers/FileController.cs):
  - Only account, application, contact, worker and event are supported.
  - The user's account or contact must match the record's owner.
  - Local-government users get read access to applications, but not delete.
  - The requested `serverRelativeUrl` must start with the record's folder name.
  - Download and delete additionally confirm the file exists.
- **Anonymous "public" endpoints.** Use an encrypted token carrying a contact ID; limited to `contact`. `public-covid-application` only checks that the application exists, with no further access check.
- **Portal to File Manager.** The portal exchanges `FILE_MANAGER_SECRET` for a JWT (symmetric key, validated by `JWT_VALID_ISSUER` and `JWT_VALID_AUDIENCE`), then uses it for gRPC calls.
- **File Manager to SharePoint.**
  - On-prem: ADFS WS-Trust username and password.
  - Cloud: Entra app registration with a client secret (`SHAREPOINT_CLIENT_ID`, `SHAREPOINT_CLIENT_SECRET`, `SHAREPOINT_AAD_TENANTID`) via Graph.
  - OpenShift templates still reference a certificate (`SHAREPOINT_CERTIFICATE_*`).
- **Dynamics to SharePoint** is not visible in this repo. It is native server-side document management using `sharepointsite` and `sharepointdocumentlocation`. Confirm how it is configured.

### 1.6 Configuration and environment parameters

`SHAREPOINT_TYPE`, `SHAREPOINT_ODATA_URI`, `SHAREPOINT_ODATA_URI_CLOUD`, `SHAREPOINT_NATIVE_BASE_URI`, `SHAREPOINT_WEBNAME`, `SHAREPOINT_USERNAME`, `SHAREPOINT_PASSWORD`, `SHAREPOINT_STS_TOKEN_URI`, `SHAREPOINT_RELYING_PARTY_IDENTIFIER`, `SHAREPOINT_AAD_TENANTID`, `SHAREPOINT_CLIENT_ID`, `SHAREPOINT_CLIENT_SECRET`, `SHAREPOINT_CERTIFICATE_*`, `FILE_MANAGER_URI`, `FILE_MANAGER_SECRET`, `FILE_MANAGER_HEALTH_URI`, `JWT_VALID_ISSUER`, `JWT_VALID_AUDIENCE`, `DISABLE_SHAREPOINT_INTEGRATION`, `ENCRYPTION_KEY`.

## 2. Findings that affect the design

1. **A cloud SharePoint (Graph) implementation exists but is incomplete.** `CloudSharePointFileManager.Legacy.cs` throws `NotImplementedException` for `FindFolderOne`, `FindFolderTwo`, `FindFolderThree`, `CreateFolder2`, `UploadFile2` and `GetFoldersInDocumentLibraryAfterDate`. This breaks `EnsureFolderPath`, `FindFolder`, `UploadFileWithFolderPath` and the sync tool in cloud mode. Native SharePoint integration should not be assumed to work by changing an endpoint.
2. **The storage layer is hardcoded to SharePoint concepts** (libraries, `serverRelativeUrl`, list titles, 160/260 path limits). An Azure blob-based design needs its own addressing scheme. The current URL is the only file ID the UI holds.
3. **Dynamics linkage depends on `sharepointdocumentlocation`.** If staff view documents inside Dynamics (Documents tab or subgrid), that depends on the native SharePoint integration. Azure Blob Storage has no native equivalent. This is the biggest open decision.
4. **Document type and "metadata" live in filenames.** A real design would store these as blob tags or index metadata and migrate old files, while still reading the old naming convention.
5. **Silent-failure risk.** The comment on `GetOrCreateParentDocumentLibraryAsync` describes an earlier bug where uploads reached SharePoint but no document location was created in Dynamics. That is why the sync tool exists. The new design should make the file write and the link record atomic, or at least reconcilable.
6. **Deleting an account removes document-location records but not the files** ([AccountsController.cs](../cllc-public-app/Controllers/AccountsController.cs)). Retention and deletion rules are a client decision.
7. **Access control is scattered.** Each entity type is handled in a `switch` in `CanAccessEntity`. Many entities in `SharePointConstants` have no portal access rule. Folder-name prefix matching is the only guard against cross-record access.
8. **Memory and size.** Whole-file buffering and a single gRPC message mean the design should consider streaming or direct upload with a short-lived token.
9. **SPICE sync registers a SharePoint file manager that nothing calls.** Likely a cleanup candidate.

## 3. Draft retain / replace / exclude decisions

Proposals for review, not decisions.

| Feature | Proposed | Why |
|---|---|---|
| Portal upload, list, download, delete | Retain behaviour, replace storage | Core user stories. Keep the REST shape the UI uses; move storage behind a document API. |
| Per-record folder structure | Retain concept (prefix path) | `{entity}/{Name}_{GUID}/` maps naturally to blob prefixes. Confirm business folder intent (2.17). |
| Doc type as filename prefix | Replace with metadata, with a compatibility read | Keep `parseFileName` working for migrated files. |
| `serverRelativeUrl` as ID | Replace with a document ID or blob path | Requires UI changes in the uploader and data services. |
| `sharepointdocumentlocation` records | Open decision | Keep as plain link records or replace with a custom table. Depends on how staff access files in Dynamics. |
| Staff access to files in Dynamics | Open decision (client) | Native integration not assumed. Options include a custom link or viewer. |
| MIME allow-list (PNG/JPEG/PDF) | Retain | Add size limits and malware scanning if required. |
| Filename truncation and path-length logic | Exclude | Blob names do not have the 160/260 limit. Keep sanitising. |
| Generated PDF "upload if changed" | Retain | Use a hash in blob metadata instead of listing and comparing. |
| Federal report CSV export | Retain, repoint | Reuse the common API. |
| Folder and library auto-creation | Exclude | Blob containers or prefixes need no pre-creation. |
| `adoxio_FileUploadedFromPortal` and `modifiedon` bump | Retain | Dynamics workflow depends on it. |
| `documentStatus` completeness checks | Retain | Query by metadata rather than by file prefix. |
| Sync tool | Replace with a one-time migration/reconcile tool | Must also cover migrating existing files and reconciling doc-location records. |
| Public token endpoints | Retain pending review | Re-check `public-covid-application`. |
| `DISABLE_SHAREPOINT_INTEGRATION` stub | Retain as a local-dev mode (Azurite or a fake) | |
| Cold storage, retention, history, previews, recovery tiers | Open (client) | No cold storage initially; other requirements remain client decisions. |

## 4. Proposed common document API (sketch)

Shared by the portal, Dynamics-facing tools and migration tooling:

- `POST /documents`: create a document with `entityType`, `entityId`, `documentType`, filename and content. Returns a document ID.
- `GET /documents?entityType=&entityId=&documentType=`: list documents.
- `GET /documents/{id}/content`: download.
- `DELETE /documents/{id}`: delete.
- `GET /documents/{id}` or `HEAD`: exists check.
- Bulk import and reconcile endpoints for migration.
- Errors: 400 (invalid type, size or name), 401, 403 (not the owner), 404, 409 (duplicate), 413, 502 (storage unavailable). Include a correlation ID.
- Authorisation: service-to-service Entra token for the API; caller identity and entity-level checks enforced by each channel (portal, staff) as today.
- Environment parameters: storage account and container, API base URL, token authority and audience, size limit, allowed types, retention flags.

## 5. Open questions and items to confirm

1. How do staff view and manage files today inside Dynamics? What breaks if the native integration is removed?
2. Are any Dynamics plugins, workflows or Power Automate flows keyed to `sharepointdocumentlocation` or `adoxio_FileUploadedFromPortal`? These are not in this repo.
3. Which SharePoint libraries listed in `SharePointConstants` are used by staff only?
4. Volume of existing files, and whether all of them migrate.
5. Retention, versioning and history, preview and recovery needs (client).
6. Is the SPICE sync SharePoint wiring dead code?
7. OpenShift templates reference certificate-based SharePoint auth, while the Cloud implementation uses a client secret. Which is current?

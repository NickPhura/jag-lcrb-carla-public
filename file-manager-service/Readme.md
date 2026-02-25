# File Manager Service

The purpose of the file manager service is to act as an interface to SharePoint.

The FileManagerService calls the underlying SharePointFileManager which performs the actual requests to SharePoint.

## Development

File manager is a .NET 6.0 application. As such you can use an IDE such as Visual Studio or VS Code to edit the files.

## gRPC

The file-manager-service uses gRPC, not HTTP.

## Cloud vs On-Premise

The underlying sharepoint file manager supports both on-premise and cloud SharePoint.

It will automatically toggle between the two versions based on which environment variables (secrets) are present.

It will default to Cloud SharePoint, if all 4 Cloud specific secrets are present and not-null or empty. If any of them are null or empty, it will fallback to the On-Premise SharePoint. See `SharePointFileManager.cs`.

## Cloud Secrets

See `file-manager-service.csproj` -> `user secrets`.

```JSON
{
  "ASPNETCORE_ENVIRONMENT": "Development",

  "JWT_TOKEN_KEY": "<token_key>",
  "JWT_VALID_AUDIENCE": "http://localhost:8080",
  "JWT_VALID_ISSUER": "http://localhost:8080",

  "SHAREPOINT_ODATA_URI": "https://bcgov.sharepoint.com/sites/lcrb-cllceDEV",
  "SHAREPOINT_WEBNAME": "lcrb-cllceDEV",
  "SHAREPOINT_AAD_TENANTID": "6fdb5200-3d0d-4a8a-b036-d3685e359adc",
  "SHAREPOINT_CLIENT_ID": "0b3c889c-04c4-43ed-9a26-88befb9ced0f",
  "SHAREPOINT_CLIENT_SECRET": "<SHAREPOINT_CLIENT_SECRET>",

  // Optional, stubs the file-manager-service to skip calling SharePOint and return hardcoded happy-path responses
  "DISABLE_SHAREPOINT_INTEGRATION": "false"
}
```

## On-Premise Secrets

```JSON
{
  "ASPNETCORE_ENVIRONMENT": "Development",

  "JWT_TOKEN_KEY": "<token_key>",
  "JWT_VALID_AUDIENCE": "http://localhost:8080",
  "JWT_VALID_ISSUER": "http://localhost:8080",

  "SHAREPOINT_ODATA_URI": "https://lcrb-cllce-sp.dev.jag.gov.bc.ca/",
  "SHAREPOINT_WEBNAME": "",
  "SHAREPOINT_NATIVE_BASE_URI": "https://lcrb-cllce-sp.dev.jag.gov.bc.ca/",
  "SHAREPOINT_RELYING_PARTY_IDENTIFIER": "urn:spcrm:lcrb:cllce",
  "SHAREPOINT_STS_TOKEN_URI": "https://ststest.gov.bc.ca/adfs/services/trust/2005/UsernameMixed",
  "SHAREPOINT_USERNAME": "CLLCDEV@IDIR",
  "SHAREPOINT_PASSWORD": "<SHAREPOINT_PASSWORD>",

  // Optional, stubs the file-manager-service to skip calling SharePOint and return hardcoded happy-path responses
  "DISABLE_SHAREPOINT_INTEGRATION": "false",

  // Optional, bypasses STS Certificate errors
  "DISABLE_SHAREPOINT_INTEGRATION": "false"
}
```

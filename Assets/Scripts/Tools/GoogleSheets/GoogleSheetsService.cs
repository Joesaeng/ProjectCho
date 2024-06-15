using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public static class GoogleSheetsService
{
    static string[] Scopes = { SheetsService.Scope.Spreadsheets };
    static string ApplicationName = "Unity Google Sheets Integration";

    public static SheetsService GetSheetsService()
    {
        UserCredential credential;

        using (var stream =
            new FileStream("Assets/Resources/GoogleAPI/credentials.json", FileMode.Open, FileAccess.Read))
        {
            string credPath = "token.json";
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
            Debug.Log("Credential file saved to: " + credPath);
        }

        return new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }

    public static void CreateSheet(SheetsService service, string spreadsheetId, string sheetName)
    {
        var addSheetRequest = new AddSheetRequest
        {
            Properties = new SheetProperties
            {
                Title = sheetName
            }
        };

        var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests = new List<Request>
            {
                new Request
                {
                    AddSheet = addSheetRequest
                }
            }
        };

        service.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, spreadsheetId).Execute();
    }
}
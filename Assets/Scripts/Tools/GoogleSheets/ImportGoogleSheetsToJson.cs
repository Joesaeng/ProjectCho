using UnityEditor;
using UnityEngine;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;
using Newtonsoft.Json;

public class ImportGoogleSheetsToJson : EditorWindow
{
    private string spreadsheetId = "13FxaHFa2dqualC039L4zP9r4CmmMfoGEL4gQHZLw2iw";
    private string jsonFilePath = "Assets/Resources/Data/yourdata.json";
    private SheetsService service;

    [MenuItem("Tools/Import Google Sheets to JSON")]
    public static void ShowWindow()
    {
        GetWindow<ImportGoogleSheetsToJson>("Import Google Sheets to JSON");
    }

    private void OnGUI()
    {
        GUILayout.Label("Import Google Sheets to JSON", EditorStyles.boldLabel);
        jsonFilePath = EditorGUILayout.TextField("JSON File Path", jsonFilePath);
        spreadsheetId = EditorGUILayout.TextField("Spreadsheet ID", spreadsheetId);

        if (GUILayout.Button("Import"))
        {
            ImportFromSheets();
        }

        if (GUILayout.Button("Import All Sheets to JSON"))
        {
            ImportAllSheetsToJsonFiles();
        }
    }

    private void ImportFromSheets()
    {
        string sheetName = Path.GetFileNameWithoutExtension(jsonFilePath);

        ImportSheetToJson(sheetName);
    }

    private void ImportAllSheetsToJsonFiles()
    {
        try
        {
            service = GoogleSheetsService.GetSheetsService();
            if (service == null)
            {
                Debug.LogError("Failed to get Google Sheets service.");
                return;
            }

            var spreadsheet = service.Spreadsheets.Get(spreadsheetId).Execute();
            foreach (var sheet in spreadsheet.Sheets)
            {
                string sheetName = sheet.Properties.Title;
                ImportSheetToJson(sheetName);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception occurred: {ex.Message}");
        }
    }

    private void ImportSheetToJson(string sheetName)
    {
        string jsonFilePath = $"Assets/Resources/Data/{sheetName}.json";
        File.WriteAllText(jsonFilePath, ConvertData.ConvertSheetDataToJson(sheetName).ToString());
        Debug.Log($"Imported Google Sheets {sheetName} to {jsonFilePath}");
    }
}

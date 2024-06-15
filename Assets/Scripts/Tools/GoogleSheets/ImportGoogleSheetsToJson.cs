using UnityEditor;
using UnityEngine;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;

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
        service = GoogleSheetsService.GetSheetsService();

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
        try
        {
            var request = service.Spreadsheets.Values.Get(spreadsheetId, $"{sheetName}!A1:Z1000");
            var response = request.Execute();
            var values = response.Values;

            if (values != null && values.Count > 0)
            {
                var headers = values[0];
                var jsonArray = new JArray();
                for (int i = 1; i < values.Count; i++) // 첫 행은 헤더이므로 제외
                {
                    var row = values[i];
                    var jsonObject = new JObject();
                    for (int j = 0; j < headers.Count; j++)
                    {
                        if (row.Count > j)
                        {
                            var value = row[j]?.ToString();
                            if (IsJson(value))
                            {
                                jsonObject[headers[j].ToString()] = JToken.Parse(value);
                            }
                            else if (IsBool(value))
                            {
                                jsonObject[headers[j].ToString()] = bool.Parse(value);
                            }
                            else
                            {
                                jsonObject[headers[j].ToString()] = value;
                            }
                        }
                        else
                        {
                            jsonObject[headers[j].ToString()] = null;
                        }
                    }
                    jsonArray.Add(jsonObject);
                }

                var finalJson = new JObject
                {
                    ["datas"] = jsonArray
                };

                string jsonFilePath = $"Assets/Resources/Data/{sheetName}.json";
                File.WriteAllText(jsonFilePath, finalJson.ToString());
                Debug.Log($"Imported Google Sheets {sheetName} to {jsonFilePath}");
            }
            else
            {
                Debug.Log($"No data found in Google Sheets for {sheetName}.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception occurred: {ex.Message}");
        }
    }

    private bool IsJson(string input)
    {
        input = input?.Trim();
        if (string.IsNullOrEmpty(input))
            return false;
        return (input.StartsWith("{") && input.EndsWith("}")) || (input.StartsWith("[") && input.EndsWith("]"));
    }

    private bool IsBool(string input)
    {
        return input.ToLower() == "true" || input.ToLower() == "false";
    }
}

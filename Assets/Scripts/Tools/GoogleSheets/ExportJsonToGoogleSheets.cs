using UnityEditor;
using UnityEngine;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
#if UNITY_EDITOR
public class ExportJsonToGoogleSheets : EditorWindow
{
    private string spreadsheetId = "13FxaHFa2dqualC039L4zP9r4CmmMfoGEL4gQHZLw2iw";
    private string jsonFilePath = "Assets/Resources/Data/yourdata.json";
    private SheetsService service;
    private bool overwriteConfirmed = false;

    [MenuItem("Tools/Export JSON to Google Sheets")]
    public static void ShowWindow()
    {
        GetWindow<ExportJsonToGoogleSheets>("Export JSON to Google Sheets");
    }

    private void OnGUI()
    {
        GUILayout.Label("Export JSON to Google Sheets", EditorStyles.boldLabel);
        jsonFilePath = EditorGUILayout.TextField("JSON File Path", jsonFilePath);
        spreadsheetId = EditorGUILayout.TextField("Spreadsheet ID", spreadsheetId);

        if (GUILayout.Button("Export"))
        {
            ExportToSheets();
        }

        if (GUILayout.Button("Export All JSON in Directory"))
        {
            ExportAllJsonFilesToSheets();
        }
    }

    private void ExportToSheets()
    {
        try
        {
            service = GoogleSheetsService.GetSheetsService();
            if (service == null)
            {
                Debug.LogError("Failed to get Google Sheets service.");
                return;
            }

            string sheetName = Path.GetFileNameWithoutExtension(jsonFilePath);
            if (!overwriteConfirmed && SheetExists(sheetName))
            {
                ShowOverwriteDialog(sheetName);
            }
            else
            {
                CreateAndExportToSheet(sheetName, jsonFilePath);
                overwriteConfirmed = false; // 초기화
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception occurred: {ex.Message}");
        }
    }

    private void ExportAllJsonFilesToSheets()
    {
        try
        {
            service = GoogleSheetsService.GetSheetsService();
            if (service == null)
            {
                Debug.LogError("Failed to get Google Sheets service.");
                return;
            }

            string[] jsonFiles = Directory.GetFiles("Assets/Resources/Data", "*.json");
            foreach (var jsonFilePath in jsonFiles)
            {
                string sheetName = Path.GetFileNameWithoutExtension(jsonFilePath);
                if (SheetExists(sheetName))
                {
                    if (!overwriteConfirmed)
                    {
                        if (EditorUtility.DisplayDialog("Sheet Exists",
                            $"A sheet named '{sheetName}' already exists. Do you want to overwrite it?",
                            "Overwrite", "Cancel"))
                        {
                            overwriteConfirmed = true;
                            ClearSheet(sheetName);
                        }
                        else
                        {
                            overwriteConfirmed = false;
                            continue;
                        }
                    }
                    else
                    {
                        ClearSheet(sheetName);
                    }
                }
                else
                {
                    GoogleSheetsService.CreateSheet(service, spreadsheetId, sheetName);
                }
                CreateAndExportToSheet(sheetName, jsonFilePath);
                overwriteConfirmed = false; // 초기화
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception occurred: {ex.Message}");
        }
    }

    private bool SheetExists(string sheetName)
    {
        var spreadsheet = service.Spreadsheets.Get(spreadsheetId).Execute();
        foreach (var sheet in spreadsheet.Sheets)
        {
            if (sheet.Properties.Title == sheetName)
            {
                return true;
            }
        }
        return false;
    }

    private void ShowOverwriteDialog(string sheetName)
    {
        if (EditorUtility.DisplayDialog("Sheet Exists",
            $"A sheet named '{sheetName}' already exists. Do you want to overwrite it?",
            "Overwrite", "Cancel"))
        {
            overwriteConfirmed = true;
            ExportToSheets();
        }
    }

    private void CreateAndExportToSheet(string sheetName, string jsonFilePath)
    {
        if (!overwriteConfirmed)
        {
            GoogleSheetsService.CreateSheet(service, spreadsheetId, sheetName);
        }
        else
        {
            // Clear the sheet if overwriting
            ClearSheet(sheetName);
        }

        string json = File.ReadAllText(jsonFilePath);
        JObject jsonObject = JObject.Parse(json);
        JArray jsonArray = jsonObject["datas"] as JArray;

        if (jsonArray == null)
        {
            Debug.LogError("JSON data array is null.");
            return;
        }

        var valueRange = new ValueRange();
        var values = new List<IList<object>>();

        // 컬럼 헤더 추가
        var headers = jsonArray.First.Children<JProperty>().Select(p => p.Name).ToList();
        values.Add(headers.Cast<object>().ToList());

        foreach (var item in jsonArray)
        {
            var row = new List<object>();
            foreach (var header in headers)
            {
                var value = item[header];
                row.Add(value?.ToString() ?? string.Empty);
            }
            values.Add(row);
        }

        valueRange.Values = values;

        var appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadsheetId, $"{sheetName}!A1");
        appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
        appendRequest.Execute();

        Debug.Log($"Exported {jsonFilePath} to Google Sheets as {sheetName}");
    }

    private void ClearSheet(string sheetName)
    {
        var clearRequest = new ClearValuesRequest();
        var clearRequestCall = service.Spreadsheets.Values.Clear(clearRequest, spreadsheetId, $"{sheetName}!A1:Z1000");
        clearRequestCall.Execute();
    }
}
#endif
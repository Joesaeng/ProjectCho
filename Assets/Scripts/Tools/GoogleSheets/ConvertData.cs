using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConvertData
{
    const string spreadsheetId = "13FxaHFa2dqualC039L4zP9r4CmmMfoGEL4gQHZLw2iw";

    public static string ConvertSheetDataToJson(string sheetName)
    {
        try
        {
            var request = GoogleSheetsService.GetSheetsService().Spreadsheets.Values.Get(spreadsheetId, $"{sheetName}!A1:Z1000");
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
                        var header = headers[j].ToString();
                        var value = row.Count > j ? row[j]?.ToString() : null;
                        try
                        {
                            if (!string.IsNullOrEmpty(value))
                            {
                                // 배열이나 중첩된 JSON 구조가 포함된 데이터를 문자열로 변환하지 않음
                                if (IsJson(value))
                                {
                                    jsonObject[header] = JToken.Parse(value);
                                }
                                else if (IsBool(value))
                                {
                                    jsonObject[header] = bool.Parse(value);
                                }
                                else if (IsNumeric(value))
                                {
                                    if (int.TryParse(value, out int intValue))
                                    {
                                        jsonObject[header] = intValue;
                                    }
                                    else if (float.TryParse(value, out float floatValue))
                                    {
                                        jsonObject[header] = floatValue;
                                    }
                                }
                                else
                                {
                                    jsonObject[header] = value;
                                }
                            }
                            else
                            {
                                jsonObject[header] = null;
                            }

                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error parsing value '{value}' for header '{header}' in row {i}: {ex.Message}");
                        }
                    }
                    jsonArray.Add(jsonObject);
                }

                var finalJson = new JObject
                {
                    ["datas"] = jsonArray
                };

                return finalJson.ToString();
            }
            else
            {
                Debug.Log($"No data found in Google Sheets for {sheetName}.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception occurred: {ex.Message}");
            return null;
        }
    }

    private static bool IsJson(string input)
    {
        input = input?.Trim();
        if (string.IsNullOrEmpty(input))
            return false;

        // JSON 객체와 배열의 시작과 끝을 체크
        if ((input.StartsWith("{") && input.EndsWith("}")) ||
            (input.StartsWith("[") && input.EndsWith("]")))
        {
            try
            {
                // 내부 구조를 제대로 인식하기 위해 JToken으로 파싱 시도
                JToken.Parse(input);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }
        return false;
    }

    private static bool IsBool(string input)
    {
        return input.ToLower() == "true" || input.ToLower() == "false";
    }

    private static bool IsNumeric(string input)
    {
        return double.TryParse(input, out _);
    }
}

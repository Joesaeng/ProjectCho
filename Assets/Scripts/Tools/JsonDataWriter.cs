using Define;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class JsonDataWriter
{
    static JsonSerializerSettings settings = new JsonSerializerSettings()
    {
        Formatting = Formatting.Indented,
        Converters = new List<JsonConverter>{new Newtonsoft.Json.Converters.StringEnumConverter() }
    };
    private static readonly object filelock = new();
    public static void WriteData(string jsonpath, string jArrayName, string newData)
    {
        lock (filelock)
        {
            // Resources 폴더에서 JSON 파일을 텍스트로 로드합니다.
            string filePath = Path.Combine(Application.dataPath, jsonpath);
            if (File.Exists(filePath))
            {
                // JSON 파일을 문자열로 로드합니다.
                string jsonString = File.ReadAllText(filePath);

                // JSON 문자열을 JObject로 파싱합니다.
                JObject jsonObject = JObject.Parse(jsonString);

                // JSON 데이터의 "baseEquipmentDatas" 배열을 가져옵니다.
                JArray jArray = (JArray)jsonObject[jArrayName];

                int newId = 0;
                if (jArray.Count > 0)
                {
                    // 마지막 아이템의 id를 가져옵니다.
                    JObject lastItem = (JObject)jArray[jArray.Count - 1];
                    int lastId = (int)lastItem["id"];

                    // 새로운 아이템의 id를 이전 아이템의 id보다 1 증가된 값으로 설정합니다.
                    newId = lastId + 1;
                }
                // 새로운 데이터를 JArray에 추가합니다.
                JObject newJObject = JObject.Parse(newData);
                newJObject["id"] = newId;
                jArray.Add(newJObject);

                // 수정된 JSON 객체를 문자열로 직렬화합니다.
                string updatedJsonString = jsonObject.ToString();
                Debug.Log("Updated JSON Content: " + updatedJsonString);

                // JSON 파일을 파일 시스템에 저장합니다.
                File.WriteAllText(filePath, updatedJsonString);
                Debug.Log("Updated JSON file saved at: " + filePath);
            }
            else
            {
                Debug.LogError("Failed to load JSON file from Resources folder.");
            }
        }
    }

    public static void WriteData(string jsonpath, IData data)
    {
        lock (filelock)
        {
            // Resources 폴더에서 JSON 파일을 텍스트로 로드합니다.
            string filePath = Path.Combine(Application.dataPath, jsonpath);
            if (File.Exists(filePath))
            {
                // JSON 파일을 문자열로 로드합니다.
                string jsonString = File.ReadAllText(filePath);

                // JSON 문자열을 JObject로 파싱합니다.
                JObject jsonObject = JObject.Parse(jsonString);

                // JSON 데이터의 "baseEquipmentDatas" 배열을 가져옵니다.
                JArray jArray = (JArray)jsonObject["datas"];

                int newId = 0;
                if (jArray.Count > 0)
                {
                    // 마지막 아이템의 id를 가져옵니다.
                    JObject lastItem = (JObject)jArray[jArray.Count - 1];
                    int lastId = (int)lastItem["id"];

                    // 새로운 아이템의 id를 이전 아이템의 id보다 1 증가된 값으로 설정합니다.
                    newId = lastId + 1;
                }
                string newData = JsonConvert.SerializeObject(data, settings);
                // 새로운 데이터를 JArray에 추가합니다.
                JObject newJObject = JObject.Parse(newData);
                newJObject["id"] = newId;
                jArray.Add(newJObject);

                // 수정된 JSON 객체를 문자열로 직렬화합니다.
                string updatedJsonString = jsonObject.ToString();
                Debug.Log("Updated JSON Content: " + newData);

                // JSON 파일을 파일 시스템에 저장합니다.
                File.WriteAllText(filePath, updatedJsonString);
            }
            else
            {
                Debug.LogError("Failed to load JSON file from Resources folder.");
            }
        }
    }
}
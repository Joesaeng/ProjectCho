using Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public class ItemDataConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(ItemData).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        ItemType itemType = jo["itemType"].ToObject<ItemType>();

        ItemData item;
        switch (itemType)
        {
            case ItemType.Equipment:
                item = new EquipmentData();
                break;
            default:
                item = new ItemData();
                break;
        }
        serializer.Populate(jo.CreateReader(), item);
        return item;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        JObject jo = JObject.FromObject(value, serializer);
        ItemType itemType = (value as ItemData).itemType;

        jo.AddFirst(new JProperty("itemType", itemType));
        jo.WriteTo(writer);
    }
}
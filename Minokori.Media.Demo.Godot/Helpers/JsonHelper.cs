using Newtonsoft.Json;

namespace Minokori.Media.Demo.Godot.Helpers;

public class CharacterConverter : JsonConverter<Character>
    {
    public override Character ReadJson(JsonReader reader, Type objectType, Character existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
        var jsonObject = Newtonsoft.Json.Linq.JObject.Load(reader);
        Character character = new(jsonObject["name"].ToString(), jsonObject["path"].ToString());
        return character;
        }
    public override void WriteJson(JsonWriter writer, Character value, JsonSerializer serializer) => throw new NotImplementedException();
    }
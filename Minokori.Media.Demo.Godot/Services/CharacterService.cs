using Newtonsoft.Json;

namespace Minokori.Media.Demo.Godot.Services;

internal class CharacterService
    {
    /// <summary>
    /// TODO 后期修改为可配置
    /// </summary>
    private const string CharacterDataPath = "./Assets/";

    private readonly Dictionary<string, Character> _characters = [];



    public void AddCharacter(string name, Character character) => _characters.Add(name, character);

    public void RemoveCharacter(string name) => _characters.Remove(name);

    public void LoadCharacters(string directory = CharacterDataPath)
        {
        DirectoryInfo info = new(directory);
        foreach (var file in info.GetFiles("*.json, *.jsonc"))
            {
            string json = File.ReadAllText($"{directory}{file.FullName}");

            Character character = JsonConvert.DeserializeObject<Character>(json);
            if (character != null)
                {
                _characters.Add(character.Name, character);
                }
            }
        }
    }

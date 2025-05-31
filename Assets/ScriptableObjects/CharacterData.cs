// CharacterData.cs
[CreateAssetMenu(menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    public string id;
    public string displayName;
    public int baseHealth;

    public List<ItemData> startingInventory;
}

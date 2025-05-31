// GameDatabase.cs
[CreateAssetMenu(menuName = "Game/Game Database")]
public class GameDatabase : ScriptableObject
{
    public List<CharacterData> characters;
    public List<ItemData> items;
    public List<EnemyData> enemies;
    public List<AbilityData> abilities;
    public List<WeaponData> weapons;
    public List<ArmorData> armors;


    public CharacterData GetCharacterById(string id) => characters.Find(c => c.id == id);
    public ItemData GetItemById(string id) => items.Find(i => i.id == id);
    public EnemyData GetEnemyById(string id) => enemies.Find(e => e.id == id);
    public AbilityData GetAbilityById(string id) => abilities.Find(a => a.id == id);
    public WeaponData GetWeaponById(string id) => weapons.Find(w => w.id == id);
    public ArmorData GetArmorById(string id) => armors.Find(a => a.id == id);
}

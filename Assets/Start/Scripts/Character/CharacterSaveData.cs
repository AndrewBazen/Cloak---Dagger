using System.Collections.Generic;
// CharacterSaveData.cs
[System.Serializable]
public class CharacterSaveData
{
    public string characterId;
    public int currentHealth;
    public List<string> inventoryItemIds;
    public float[] position;
    public int level;
    public int experience;
    public int gold;
    public int[] stats;
    public int[] inventory; 
    
}

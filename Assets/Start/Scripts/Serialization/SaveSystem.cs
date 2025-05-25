using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Start.Scripts.Enemy;
using Start.Scripts.Party;
using Start.Scripts.Character;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Start.Scripts.Serialization
{
    public class SaveSystem
    {
        public void SaveGameData(string saveName, List<CharacterInfoData> party, List<EnemyData> enemies)
        {
            Debug.Log(Application.persistentDataPath);
            BinaryFormatter formatter = GetBinaryFormatter();

            if (!Directory.Exists(Application.persistentDataPath + "/saves"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/saves");
            }
            string path = Application.persistentDataPath + "/saves/" + saveName + ".sfl";

            FileStream stream = new FileStream(path, FileMode.Create);
            GameData gameData = new GameData(party, enemies);
            formatter.Serialize(stream, gameData);
            stream.Close();
        }

        public GameData LoadGameData(string saveName)
        {
            string path = Application.persistentDataPath + "/saves/" + saveName + ".sfl";
            if (!File.Exists(path))
            {
                Debug.LogError("Save File not Found in " + path);
                return null;
            }

            BinaryFormatter formatter = GetBinaryFormatter();

            FileStream stream = new FileStream(path, FileMode.Open);

            try
            {
                GameData gameData = formatter.Deserialize(stream) as GameData;
                stream.Close();
                return gameData;
            }
            catch
            {
                Debug.LogErrorFormat("Failed to load file at {0}", path);
                stream.Close();
                return null;
            }
        }

        private BinaryFormatter GetBinaryFormatter()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            SurrogateSelector selector = new SurrogateSelector();

            Vector3SerializationSurrogate vector3Surrogate = new Vector3SerializationSurrogate();
            ListSerializationSurrogate<CharacterInfoData> partyListSurrogate = new ListSerializationSurrogate<CharacterInfoData>();
            ListSerializationSurrogate<EnemyData> enemyListSurrogate = new ListSerializationSurrogate<EnemyData>();

            selector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3Surrogate);
            selector.AddSurrogate(typeof(List<CharacterInfoData>), new StreamingContext(StreamingContextStates.All), partyListSurrogate);
            selector.AddSurrogate(typeof(List<EnemyData>), new StreamingContext(StreamingContextStates.All), enemyListSurrogate);

            formatter.SurrogateSelector = selector;

            return formatter;
        }
    }
}

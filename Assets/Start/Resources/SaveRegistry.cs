using System;
using System.Collections.Generic;
using Start.Scripts.BaseClasses;
namespace Start.Resources
{
    public static class SaveRegistry
    {
        public static event Action<string> OnSaveUnregistered;
        public static event Action<string> OnSaveRegistered;
        public static event Action<List<SaveData>> OnSavesUpdated;

        public static void Register(string id)
        {
            OnSaveRegistered?.Invoke(id);
        }

        public static void Unregister(string id)
        {
            OnSaveUnregistered?.Invoke(id);
        }   

        public static void UpdateSaves(List<SaveData> saves)
        {
            OnSavesUpdated?.Invoke(saves);
        }
    }
}

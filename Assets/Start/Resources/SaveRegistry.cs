
using System;
using Start.Scripts.BaseClasses;
namespace Start.Resources
{
    public static class SaveRegistry
    {
        public static event Action<string> OnSaveUnregistered;
        public static event Action<string> OnSaveRegistered;

        public static void Register(string id)
        {
            OnSaveRegistered?.Invoke(id);
        }

        public static void Unregister(string id)
        {
            OnSaveUnregistered?.Invoke(id);
        }
    }
}

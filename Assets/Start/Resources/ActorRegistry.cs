using System;
using Start.Scripts.BaseClasses;
namespace Start.Resources
{
    public static class ActorRegistry
    {
        public static event Action<Actor> OnActorUnregistered;
        public static event Action<Actor> OnActorRegistered;

        public static void Register(Actor actor)
        {
            OnActorRegistered?.Invoke(actor);
        }

        public static void Unregister(Actor actor)
        {
            OnActorUnregistered?.Invoke(actor);
        }
    }
}


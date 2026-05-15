using UnityEngine;

namespace Capybrawlers.Core
{
    // Registers all core services before any scene loads, regardless of the
    // entry-point scene. This lets you press Play from Battle.unity during
    // development without needing to go through Bootstrap.unity first.
    internal static class AppBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            ServiceLocator.Clear();

            var events = new EventBus();
            ServiceLocator.Register<IEventBus>(events);

            var save = new LocalJsonSaveProvider();
            ServiceLocator.Register<ISaveProvider>(save);

            new OverlordInitializer(save).EnsureProfileExists();

            var bp = new BPManager(save, events);
            ServiceLocator.Register<IBPManager>(bp);

            ServiceLocator.Register<ISceneLoader>(new SceneLoader());
        }
    }
}

using UnityEngine;

namespace Capybrawlers.Core
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Bootstrap();
        }

        private void Bootstrap()
        {
            var events = new EventBus();
            ServiceLocator.Register<IEventBus>(events);

            var save = new LocalJsonSaveProvider();
            ServiceLocator.Register<ISaveProvider>(save);

            // Ensure a profile exists on disk before other services try to read it.
            new OverlordInitializer(save).EnsureProfileExists();

            var bp = new BPManager(save, events);
            ServiceLocator.Register<IBPManager>(bp);

            var scenes = new SceneLoader();
            ServiceLocator.Register<ISceneLoader>(scenes);

            scenes.LoadAdditive(SceneId.MainMenu);
        }
    }
}

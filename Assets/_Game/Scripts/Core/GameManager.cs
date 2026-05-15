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
            // Services are already registered by AppBootstrap.Init() before
            // any scene loads. GameManager only handles the additive scene load.
            ServiceLocator.Get<ISceneLoader>().LoadAdditive(SceneId.MainMenu);
        }
    }
}

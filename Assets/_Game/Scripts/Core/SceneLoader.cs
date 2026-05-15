using UnityEngine.SceneManagement;

namespace Capybrawlers.Core
{
    public interface ISceneLoader
    {
        void Load(SceneId id);
        void LoadAdditive(SceneId id);
        void Unload(SceneId id);
    }

    public class SceneLoader : ISceneLoader
    {
        public void Load(SceneId id)         => SceneManager.LoadScene((int)id);
        public void LoadAdditive(SceneId id) => SceneManager.LoadScene((int)id, LoadSceneMode.Additive);
        public void Unload(SceneId id)       => SceneManager.UnloadSceneAsync((int)id);
    }
}

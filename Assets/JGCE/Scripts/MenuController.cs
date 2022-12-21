using UnityEngine;
using UnityEngine.SceneManagement;

namespace JGCE.Scripts
{
    public class MenuController : MonoBehaviour
    {
        public void ChangeScene(string scene)
        {
            SceneManager.LoadScene(scene);
        }
    }
}
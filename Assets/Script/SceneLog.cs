using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLog : MonoBehaviour
{
    void Start()
    {
        var s = SceneManager.GetActiveScene();
        Debug.Log($"[SCENE] Active Scene = {s.name} ({s.path}), buildIndex={s.buildIndex}");
    }
}

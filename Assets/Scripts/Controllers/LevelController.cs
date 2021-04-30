using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    [SerializeField] Animator anim;
   
    // Start is called before the first frame update
    void Start()
    {
        SoundManager.Initialize();
        SceneIndex.currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    IEnumerator corountine;
    public void LoadScene(int sceneIndex)
    {
        SceneIndex.previousSceneIndex = SceneIndex.currentSceneIndex;
        SceneIndex.currentSceneIndex = sceneIndex;
        corountine = EndScene(sceneIndex);
        StartCoroutine(corountine);
    }

    public void LoadNextScene()
    {
        int sceneIndex = (SceneIndex.currentSceneIndex++ >= SceneManager.sceneCountInBuildSettings) ? 0 : SceneIndex.currentSceneIndex++;
        LoadScene(sceneIndex);
    }

    public void LoadPreviousScene()
    {
        LoadScene(SceneIndex.previousSceneIndex);
    }

    private IEnumerator EndScene(int sceneIndex)
    {
        while (true)
        {
            anim.SetBool("LevelEnding", true);
            yield return new WaitForSeconds(anim.GetCurrentAnimatorClipInfo(0).Length + 0.5f);
            print("Loading Scene " + sceneIndex);
            SceneManager.LoadScene(sceneIndex);
        }
    }
}

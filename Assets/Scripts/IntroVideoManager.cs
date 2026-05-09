using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroVideoManager : MonoBehaviour
{
    [Header("Dependencies")]
    public VideoPlayer videoPlayer;
    
    [Header("Settings")]
    [Tooltip("The exact name of the scene to load after the video.")]
    public string nextSceneName = "SampleScene"; // Change this to your actual scene name
    
    [Tooltip("Can the player press a key to skip the video?")]
    public bool canSkip = true;

    private bool _isLoading = false;

    private void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        // Subscribe to the built-in event that fires when the video reaches the end
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void Update()
    {
        // Allow skipping with Escape, Space, or Enter
        if (canSkip && !_isLoading)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || 
                Input.GetKeyDown(KeyCode.Space) || 
                Input.GetKeyDown(KeyCode.Return))
            {
                SkipVideo();
            }
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        LoadNextScene();
    }

    private void SkipVideo()
    {
        videoPlayer.Stop();
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (_isLoading) return;
        _isLoading = true;

        // Optional: If you want to use your FadeManager here, you could trigger a fade 
        // and load the scene in a coroutine instead of an instant load.
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        // Always unsubscribe from events to prevent memory leaks
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}
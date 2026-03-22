using UnityEngine;
using UnityEngine.Video; // Needed for VideoPlayer
using UnityEngine.UI; // Needed for UI elements if manipulating them
using TMPro;

public class VideoManager : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer; // Reference to your Video Player object
    [SerializeField] private Button playButton; // Button that triggers playback
    [SerializeField] private RawImage outputRawImage; // Optional UI target
    [SerializeField] private RenderTexture outputTexture; // Optional render target
    [SerializeField] private VideoClip fallbackClip; // Optional fallback clip
    [SerializeField] private string fallbackUrl; // Optional fallback URL
    [SerializeField] private bool hideOutputUntilPlay = true; // Keep video hidden until button click
    [SerializeField] private GameObject videoContainer; // Optional parent object to hide/show
    [SerializeField] private TMP_Text textToHide; // Text (TMP)
    [SerializeField] private TMP_Text textToShow; // Legacy fallback for Wifi Strength Text
    [SerializeField] private TMP_Text wifiStrengthText; // "Wifi Strength Text"
    [SerializeField] private TMP_Text strongWifiText; // "Strong Wifi Text"
    [SerializeField] private TMP_Text veryStrongWifiText; // "Very Strong Wifi Text"
    [SerializeField] private float strongWifiDurationSeconds = 10f;

    [Header("Background Test Consent Menu")]
    [SerializeField] private GameObject consentMenuPanel;
    [SerializeField] private TMP_Text consentPromptText;
    [SerializeField] private Button optInButton;
    [SerializeField] private Button optOutButton;

    [Header("Reminder UI Panel")]
    [SerializeField] private GameObject reminderMenuPanel;
    [SerializeField] private TMP_Text reminderPromptText;
    [SerializeField] private Button reminderYesButton;
    [SerializeField] private Button reminderNoButton;
    [SerializeField] private float veryStrongBeforeReminderSeconds = 10f;

    [Header("Check Wifi Panel")]
    [SerializeField] private GameObject checkWifiPanel;
    [SerializeField] private Button dontRunButton;
    [SerializeField] private Button rerunButton;
    [SerializeField] private float checkWifiDelaySeconds = 10f;

    private bool isListenerRegistered;
    private bool isOptInListenerRegistered;
    private bool isOptOutListenerRegistered;
    private bool isReminderYesListenerRegistered;
    private bool isReminderNoListenerRegistered;
    private bool isDontRunListenerRegistered;
    private bool isRerunListenerRegistered;
    private Coroutine playbackTimerCoroutine;
    private Coroutine wifiTextTimerCoroutine;
    private Coroutine reminderMenuTimerCoroutine;
    private Coroutine checkWifiDelayCoroutine;

    void Awake()
    {
        // Try to auto-assign if missing to reduce setup issues.
        if (videoPlayer == null)
        {
            videoPlayer = FindObjectOfType<VideoPlayer>();
        }

        if (playButton == null)
        {
            playButton = GetComponent<Button>();
            if (playButton == null)
            {
                playButton = FindObjectOfType<Button>();
            }
        }
    }
    
    void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer reference is not set in the inspector.");
            return;
        }

        // Ensure video does not auto-play on scene start.
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.Stop();

        videoPlayer.errorReceived -= OnVideoError;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.started -= OnVideoStarted;
        videoPlayer.started += OnVideoStarted;
        videoPlayer.prepareCompleted -= OnVideoPrepared;

        ConfigureOutputTarget();
        if (wifiStrengthText == null)
        {
            wifiStrengthText = textToShow;
        }
        InitializeUIState();

        if (hideOutputUntilPlay)
        {
            HideVideoOutput();
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(PlayVideo);
            isListenerRegistered = true;
            Debug.Log("Play button listener registered.");
        }
        else
        {
            Debug.LogWarning("Play Button reference is not set. Assign a Button or wire PlayVideo() in the Button OnClick event.");
        }

        if (optInButton != null)
        {
            optInButton.onClick.AddListener(OnOptInClicked);
            isOptInListenerRegistered = true;
        }

        if (optOutButton != null)
        {
            optOutButton.onClick.AddListener(OnOptOutClicked);
            isOptOutListenerRegistered = true;
        }

        if (consentPromptText != null)
        {
            consentPromptText.text = "To ensure that wifi strength information is always updated, wifi speed tests need to be run in the background. Do you opt-in to run wifi strength tests in the background?";
        }

        if (reminderYesButton != null)
        {
            reminderYesButton.onClick.AddListener(OnReminderYesClicked);
            isReminderYesListenerRegistered = true;
        }

        if (reminderNoButton != null)
        {
            reminderNoButton.onClick.AddListener(OnReminderNoClicked);
            isReminderNoListenerRegistered = true;
        }

        if (reminderPromptText != null)
        {
            reminderPromptText.text = "Wifi strength tests have been running in the background for 3 days and 17 hours. Would you like to continue running Wifi tests in the background?";
        }

        if (dontRunButton != null)
        {
            dontRunButton.onClick.AddListener(OnDontRunClicked);
            isDontRunListenerRegistered = true;
        }

        if (rerunButton != null)
        {
            rerunButton.onClick.AddListener(OnRerunClicked);
            isRerunListenerRegistered = true;
        }
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.started -= OnVideoStarted;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }

        if (wifiTextTimerCoroutine != null)
        {
            StopCoroutine(wifiTextTimerCoroutine);
            wifiTextTimerCoroutine = null;
        }

        if (reminderMenuTimerCoroutine != null)
        {
            StopCoroutine(reminderMenuTimerCoroutine);
            reminderMenuTimerCoroutine = null;
        }

        if (checkWifiDelayCoroutine != null)
        {
            StopCoroutine(checkWifiDelayCoroutine);
            checkWifiDelayCoroutine = null;
        }

        if (playButton != null && isListenerRegistered)
        {
            playButton.onClick.RemoveListener(PlayVideo);
        }

        if (optInButton != null && isOptInListenerRegistered)
        {
            optInButton.onClick.RemoveListener(OnOptInClicked);
        }

        if (optOutButton != null && isOptOutListenerRegistered)
        {
            optOutButton.onClick.RemoveListener(OnOptOutClicked);
        }

        if (reminderYesButton != null && isReminderYesListenerRegistered)
        {
            reminderYesButton.onClick.RemoveListener(OnReminderYesClicked);
        }

        if (reminderNoButton != null && isReminderNoListenerRegistered)
        {
            reminderNoButton.onClick.RemoveListener(OnReminderNoClicked);
        }
        if (dontRunButton != null && isDontRunListenerRegistered)
        {
            dontRunButton.onClick.RemoveListener(OnDontRunClicked);
        }

        if (rerunButton != null && isRerunListenerRegistered)
        {
            rerunButton.onClick.RemoveListener(OnRerunClicked);
        }    }

    // Public function to be called by the button
    public void PlayVideo()
    {
        Debug.Log("PlayVideo() called.");

        if (videoPlayer != null)
        {
            if (!EnsureSourceConfigured())
            {
                return;
            }

            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }

            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.prepareCompleted += OnVideoPrepared;

            // Always prepare first to avoid silent failures on some platforms/codecs.
            videoPlayer.Prepare();
            Debug.Log($"Preparing video. source={videoPlayer.source}, clip={(videoPlayer.clip != null ? videoPlayer.clip.name : "null")}, url='{videoPlayer.url}', renderMode={videoPlayer.renderMode}");
        }
        else
        {
            Debug.LogError("VideoPlayer reference is not set. Cannot play video.");
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnVideoPrepared;
        vp.time = 0;
        ShowVideoOutput();
        vp.Play();
        Debug.Log("Video prepared and playback started.");
    }

    private void OnVideoStarted(VideoPlayer vp)
    {
        HideWelcomeUI();
        ShowConsentMenu();
        Debug.Log($"Video started. isPlaying={vp.isPlaying}, frame={vp.frame}, time={vp.time:F2}");
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"VideoPlayer error: {message}");
    }

    private bool EnsureSourceConfigured()
    {
        if (videoPlayer.source == VideoSource.VideoClip)
        {
            if (videoPlayer.clip == null)
            {
                if (fallbackClip != null)
                {
                    videoPlayer.clip = fallbackClip;
                    Debug.Log($"Assigned fallback clip: {fallbackClip.name}");
                }
                else
                {
                    Debug.LogError("Video source is VideoClip but no clip is assigned. Assign VideoPlayer.clip or fallbackClip.");
                    return false;
                }
            }

            return true;
        }

        if (videoPlayer.source == VideoSource.Url)
        {
            if (string.IsNullOrWhiteSpace(videoPlayer.url))
            {
                if (!string.IsNullOrWhiteSpace(fallbackUrl))
                {
                    videoPlayer.url = fallbackUrl;
                    Debug.Log($"Assigned fallback URL: {fallbackUrl}");
                }
                else
                {
                    Debug.LogError("Video source is Url but url is empty. Assign VideoPlayer.url or fallbackUrl.");
                    return false;
                }
            }

            return true;
        }

        Debug.LogError("Unsupported video source configuration.");
        return false;
    }

    private void ConfigureOutputTarget()
    {
        if (outputTexture != null)
        {
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = outputTexture;

            if (outputRawImage != null)
            {
                outputRawImage.texture = outputTexture;
            }

            Debug.Log("Video output set to provided RenderTexture.");
            return;
        }

        if (outputRawImage != null)
        {
            RenderTexture rtFromUI = outputRawImage.texture as RenderTexture;
            if (rtFromUI != null)
            {
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                videoPlayer.targetTexture = rtFromUI;
                Debug.Log("Video output set to RawImage RenderTexture.");
            }
            else
            {
                Debug.LogWarning("Output RawImage texture is not a RenderTexture. Assign outputTexture or use Camera/FarPlane render mode.");
            }
        }
    }

    private void HideVideoOutput()
    {
        if (videoContainer != null)
        {
            videoContainer.SetActive(false);
        }

        if (outputRawImage != null)
        {
            outputRawImage.enabled = false;
        }
    }

    private void ShowVideoOutput()
    {
        if (videoContainer != null)
        {
            videoContainer.SetActive(true);
        }

        if (outputRawImage != null)
        {
            outputRawImage.enabled = true;
        }
    }

    private void InitializeUIState()
    {
        if (textToHide != null)
        {
            textToHide.gameObject.SetActive(true);
        }

        if (playButton != null)
        {
            playButton.gameObject.SetActive(true);
        }

        if (textToShow != null)
        {
            textToShow.gameObject.SetActive(false);
        }

        if (wifiStrengthText != null)
        {
            wifiStrengthText.gameObject.SetActive(false);
        }

        if (strongWifiText != null)
        {
            strongWifiText.gameObject.SetActive(false);
        }

        if (veryStrongWifiText != null)
        {
            veryStrongWifiText.gameObject.SetActive(false);
        }

        HideConsentMenu();
        HideReminderMenu();
        HideCheckWifiPanel();
    }

    private void HideWelcomeUI()
    {
        if (textToHide != null)
        {
            textToHide.gameObject.SetActive(false);
        }

        if (playButton != null)
        {
            playButton.gameObject.SetActive(false);
        }
    }

    private void OnOptInClicked()
    {
        Debug.Log("User selected Yes, opt-in.");
        CompleteWifiTestFlow();
    }

    private void OnOptOutClicked()
    {
        Debug.Log("User selected No, opt-out.");
        CompleteWifiTestFlow();
    }

    private void CompleteWifiTestFlow()
    {
        HideConsentMenu();
        HideReminderMenu();
        HideCheckWifiPanel();

        if (reminderMenuTimerCoroutine != null)
        {
            StopCoroutine(reminderMenuTimerCoroutine);
            reminderMenuTimerCoroutine = null;
        }

        if (checkWifiDelayCoroutine != null)
        {
            StopCoroutine(checkWifiDelayCoroutine);
            checkWifiDelayCoroutine = null;
        }

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        HideVideoOutput();

        ShowPostVideoWifiTexts();
        Debug.Log("Wifi test flow completed. Showing wifi strength texts.");
    }

    private void ShowPostVideoWifiTexts()
    {
        if (wifiStrengthText != null)
        {
            wifiStrengthText.gameObject.SetActive(true);
        }
        else if (textToShow != null)
        {
            // Backward compatibility if only the old field is assigned.
            textToShow.gameObject.SetActive(true);
        }

        if (strongWifiText != null)
        {
            strongWifiText.gameObject.SetActive(true);
        }

        if (veryStrongWifiText != null)
        {
            veryStrongWifiText.gameObject.SetActive(false);
        }

        if (wifiTextTimerCoroutine != null)
        {
            StopCoroutine(wifiTextTimerCoroutine);
        }

        if (reminderMenuTimerCoroutine != null)
        {
            StopCoroutine(reminderMenuTimerCoroutine);
            reminderMenuTimerCoroutine = null;
        }

        if (checkWifiDelayCoroutine != null)
        {
            StopCoroutine(checkWifiDelayCoroutine);
            checkWifiDelayCoroutine = null;
        }

        HideReminderMenu();
        HideCheckWifiPanel();

        wifiTextTimerCoroutine = StartCoroutine(TransitionToVeryStrongWifiText());
    }

    private System.Collections.IEnumerator TransitionToVeryStrongWifiText()
    {
        yield return new WaitForSeconds(strongWifiDurationSeconds);

        if (strongWifiText != null)
        {
            strongWifiText.gameObject.SetActive(false);
        }

        if (veryStrongWifiText != null)
        {
            veryStrongWifiText.gameObject.SetActive(true);
        }

        if (reminderMenuTimerCoroutine != null)
        {
            StopCoroutine(reminderMenuTimerCoroutine);
        }

        reminderMenuTimerCoroutine = StartCoroutine(ShowReminderMenuAfterDelay());

        wifiTextTimerCoroutine = null;
        Debug.Log("Strong Wifi Text duration finished. Very Strong Wifi Text is now shown.");
    }

    private System.Collections.IEnumerator ShowReminderMenuAfterDelay()
    {
        yield return new WaitForSeconds(veryStrongBeforeReminderSeconds);
        ShowReminderMenu();
        reminderMenuTimerCoroutine = null;
    }

    private void ShowConsentMenu()
    {
        if (consentMenuPanel != null)
        {
            consentMenuPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Consent menu panel is not assigned.");
        }
    }

    private void HideConsentMenu()
    {
        if (consentMenuPanel != null)
        {
            consentMenuPanel.SetActive(false);
        }
    }

    private void ShowReminderMenu()
    {
        if (reminderMenuPanel != null)
        {
            reminderMenuPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Reminder menu panel is not assigned.");
        }
    }

    private void HideReminderMenu()
    {
        if (reminderMenuPanel != null)
        {
            reminderMenuPanel.SetActive(false);
        }
    }

    private void OnReminderYesClicked()
    {
        Debug.Log("User selected Yes, continue on reminder menu.");
        HideReminderMenu();

        if (reminderMenuTimerCoroutine != null)
        {
            StopCoroutine(reminderMenuTimerCoroutine);
        }

        reminderMenuTimerCoroutine = StartCoroutine(ShowReminderMenuAfterDelay());
    }

    private void OnReminderNoClicked()
    {
        Debug.Log("User selected No, discontinue on reminder menu.");
        HideReminderMenu();

        if (reminderMenuTimerCoroutine != null)
        {
            StopCoroutine(reminderMenuTimerCoroutine);
            reminderMenuTimerCoroutine = null;
        }

        if (checkWifiDelayCoroutine != null)
        {
            StopCoroutine(checkWifiDelayCoroutine);
        }

        checkWifiDelayCoroutine = StartCoroutine(ShowCheckWifiPanelAfterDelay());
    }

    private System.Collections.IEnumerator ShowCheckWifiPanelAfterDelay()
    {
        yield return new WaitForSeconds(checkWifiDelaySeconds);
        ShowCheckWifiPanel();
        checkWifiDelayCoroutine = null;
    }

    private void ShowCheckWifiPanel()
    {
        if (checkWifiPanel != null)
        {
            checkWifiPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Check Wifi panel is not assigned.");
        }
    }

    private void HideCheckWifiPanel()
    {
        if (checkWifiPanel != null)
        {
            checkWifiPanel.SetActive(false);
        }
    }

    private void OnDontRunClicked()
    {
        Debug.Log("User selected Don't Run.");
        HideCheckWifiPanel();

        if (checkWifiDelayCoroutine != null)
        {
            StopCoroutine(checkWifiDelayCoroutine);
        }

        checkWifiDelayCoroutine = StartCoroutine(ShowCheckWifiPanelAfterDelay());
    }

    private void OnRerunClicked()
    {
        Debug.Log("User selected Rerun. Resetting to start.");
        ResetToStart();
    }

    private void ResetToStart()
    {
        // Stop all coroutines
        if (playbackTimerCoroutine != null)
        {
            StopCoroutine(playbackTimerCoroutine);
            playbackTimerCoroutine = null;
        }

        if (wifiTextTimerCoroutine != null)
        {
            StopCoroutine(wifiTextTimerCoroutine);
            wifiTextTimerCoroutine = null;
        }

        if (reminderMenuTimerCoroutine != null)
        {
            StopCoroutine(reminderMenuTimerCoroutine);
            reminderMenuTimerCoroutine = null;
        }

        if (checkWifiDelayCoroutine != null)
        {
            StopCoroutine(checkWifiDelayCoroutine);
            checkWifiDelayCoroutine = null;
        }

        // Hide all panels and texts
        HideVideoOutput();
        HideConsentMenu();
        HideReminderMenu();
        HideCheckWifiPanel();

        if (wifiStrengthText != null)
        {
            wifiStrengthText.gameObject.SetActive(false);
        }

        if (strongWifiText != null)
        {
            strongWifiText.gameObject.SetActive(false);
        }

        if (veryStrongWifiText != null)
        {
            veryStrongWifiText.gameObject.SetActive(false);
        }

        // Show welcome UI
        if (textToHide != null)
        {
            textToHide.gameObject.SetActive(true);
        }

        if (playButton != null)
        {
            playButton.gameObject.SetActive(true);
        }

        // Stop video if playing
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        Debug.Log("Reset to start state.");
    }
}

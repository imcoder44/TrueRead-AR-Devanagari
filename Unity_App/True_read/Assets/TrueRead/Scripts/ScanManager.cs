using UnityEngine;
using TMPro;
using System.Collections;

public class ScanManager : MonoBehaviour
{
    [Header("Scene References")]
    public CameraManager cameraManager;
    public ModelDisplayManager modelDisplayManager;
    public SentisInferenceManager sentisManager; // (Assumes your teammate has this script in the project)

    [Header("UI — Scan Guide")]
    [Tooltip("The text at the top that says 'Hold character...'")]
    public TextMeshProUGUI scanGuideTMP;

    [Header("Settings")]
    public float scanInterval = 1.2f;
    [Range(0f, 1f)] public float confidenceThreshold = 0.70f;
    public float autoDismissSeconds = 3.5f;

    private static readonly string[] INDEX_TO_CHAR = new string[]
    {
        "क","ख","ग","घ","ङ","च","छ","ज","झ","ञ",
        "ट","ठ","ड","ढ","ण","त","थ","द","ध","न",
        "प","फ","ब","भ","म","य","र","ल","व","श",
        "ष","स","ह","क्ष","त्र","ज्ञ","०","१","२","३",
        "४","५","६","७","८","९"
    };

    private bool _isScanning = false;
    private bool _usingCamera = true;
    private bool _modelVisible = false;
    private float _noDetectSeconds = 0f;
    private Coroutine _scanCoroutine;

    void Start()
    {
        if (scanGuideTMP) scanGuideTMP.text = "Hold character in front of camera";
        _scanCoroutine = StartCoroutine(ScanLoop());
    }

    void OnDestroy()
    {
        if (_scanCoroutine != null) StopCoroutine(_scanCoroutine);
    }

    IEnumerator ScanLoop()
    {
        while (cameraManager == null || !cameraManager.IsCameraReady)
            yield return new WaitForSeconds(0.3f);

        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            if (!_isScanning && _usingCamera)
                yield return StartCoroutine(CaptureAndPredict());

            yield return new WaitForSeconds(scanInterval);
        }
    }

    IEnumerator CaptureAndPredict()
    {
        _isScanning = true;

        if (!cameraManager.IsCameraReady || sentisManager == null)
        {
            _isScanning = false; yield break;
        }

        Texture2D snapshot = cameraManager.CaptureSnapshot();
        if (snapshot == null) { _isScanning = false; yield break; }

        yield return null; // Wait a frame so the UI doesn't freeze

        var (idx, conf) = sentisManager.RunInference(snapshot);
        Destroy(snapshot); // Prevent memory leaks!

        if (idx >= 0 && conf >= confidenceThreshold)
        {
            _noDetectSeconds = 0f;
            ShowResult(idx);
        }
        else
        {
            _noDetectSeconds += scanInterval;

            if (_modelVisible && _noDetectSeconds >= autoDismissSeconds)
            {
                AutoDismiss();
            }
            else if (scanGuideTMP && !_modelVisible)
            {
                scanGuideTMP.text = conf < 0.3f ? "No character detected" : "Hold steady...";
            }
        }

        _isScanning = false;
    }

    void AutoDismiss()
    {
        _modelVisible = false;
        _noDetectSeconds = 0f;
        modelDisplayManager?.DismissCurrentPackage();
        if (scanGuideTMP) scanGuideTMP.text = "Hold character in front of camera";
    }

    void ShowResult(int index)
    {
        _modelVisible = true;
        if (scanGuideTMP) scanGuideTMP.text = ""; // Clear text to show pure AR

        if (modelDisplayManager != null)
        {
            modelDisplayManager.ShowCharacter(index);
        }
    }

    // ════════════════════════════════════════════════════════════════════
    // PUBLIC BUTTON METHODS (Wire these in the Inspector!)
    // ════════════════════════════════════════════════════════════════════
    public void OnGalleryPressed()
    {
        _usingCamera = false;
        cameraManager?.StopCamera();

        // Note: Requires NativeGallery plugin to actually open the phone gallery
#if UNITY_ANDROID
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path == null) { ResumeCamera(); return; }
            StartCoroutine(ProcessGalleryImage(path));
        }, "Select a character image", "image/*");
#else
        // If testing in Unity Editor, just simulate a random scan
        ShowResult(Random.Range(0, 46)); 
#endif
    }

    IEnumerator ProcessGalleryImage(string imagePath)
    {
        Texture2D tex = NativeGallery.LoadImageAtPath(imagePath, 512, false);
        if (tex == null) { ResumeCamera(); yield break; }

        yield return null;
        if (sentisManager == null) { Destroy(tex); ResumeCamera(); yield break; }

        var (idx, conf) = sentisManager.RunInference(tex);
        Destroy(tex);

        if (idx >= 0 && conf >= confidenceThreshold)
            ShowResult(idx);
        else
        {
            if (scanGuideTMP) scanGuideTMP.text = "Could not recognise image";
            ResumeCamera();
        }
    }

    public void ResumeCamera()
    {
        _usingCamera = true;
        cameraManager?.StartCamera();
        if (scanGuideTMP) scanGuideTMP.text = "Hold character in front of camera";
    }

    public void OnBackPressed()
    {
        cameraManager?.StopCamera();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
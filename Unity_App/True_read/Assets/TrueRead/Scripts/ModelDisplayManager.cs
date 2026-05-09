// ModelDisplayManager.cs
// Attach to: ModelDisplayManager GameObject in ScanScene
//
// CHANGES FROM PREVIOUS — additions only, zero logic changed:
//   + Start() calls ValidateSetup() which prints a full slot audit to Console
//   + spawnPoint null-guard added before Instantiate (was a silent NullRef crash)
//   + [ContextMenu] "Test Spawn Index 0" — click in Inspector during Play mode
//     to spawn index 0 directly without needing a scan result
//   + [ContextMenu] "Validate All Slots" — run audit any time from Inspector
//   + ShowCharacter now logs WHICH CharacterData slot was null and at what index
//   + ShowCharacter now logs WHICH prefab was null and in which CharData asset

using UnityEngine;

public class ModelDisplayManager : MonoBehaviour
{
    [Header("All 46 CharacterData assets — index 0 to 45")]
    [Tooltip("Slot 0=क(ka), 1=ख(kha) ... 32=ह(ha) ... 35=ज्ञ(gya), 36=०, ... 45=९\n" +
             "Order must match class_mapping.json exactly.\n" +
             "Run 'Validate All Slots' from the gear menu to check for nulls.")]
    public CharacterData[] characters = new CharacterData[46];

    [Header("Where to spawn the 3D package")]
    [Tooltip("Create an empty GameObject in your scene at the position where the\n" +
             "3D model should appear, then drag it here.")]
    public Transform spawnPoint;

    // ─── Private state (unchanged) ────────────────────────────────────────────
    private GameObject _current;
    private int        _lastIndex = -1;

    // ─── ADDITION: Validate on Start ─────────────────────────────────────────
    // Runs immediately when the scene loads and prints a full audit.
    // Check Console for lines starting with [ModelDisplay] to see exactly
    // which slots are null before you even attempt a scan.
    void Start()
    {
        ValidateSetup();
    }

    // ─── ShowCharacter (logic unchanged, error messages improved) ─────────────
    public void ShowCharacter(int modelOutputIndex)
    {
        Debug.Log("[ModelDisplay] Received model index: " + modelOutputIndex);

        if (modelOutputIndex < 0 || modelOutputIndex >= 46)
        {
            Debug.LogError("[ModelDisplay] Index out of range: " + modelOutputIndex +
                           " (valid range: 0–45)");
            return;
        }

        if (modelOutputIndex == _lastIndex)
        {
            Debug.Log("[ModelDisplay] Same character as last scan — skipping reload.");
            return;
        }

        _lastIndex = modelOutputIndex;

        // --- COIN REWARD SYSTEM ---
        int currentCoins = PlayerPrefs.GetInt("total_coins", 0);
        PlayerPrefs.SetInt("total_coins", currentCoins + 1);
        PlayerPrefs.Save();
        Debug.Log($"[Coins] Earned 1 coin! Total coins: {currentCoins + 1}");
        // --------------------------

        // ── ADDITION: spawnPoint null guard ───────────────────────────────────
        // Previously this would throw a NullReferenceException silently if
        // spawnPoint was not assigned in the Inspector.
        if (spawnPoint == null)
        {
            Debug.LogError("[ModelDisplay] spawnPoint is NULL. " +
                           "Create an empty GameObject in your ScanScene at the " +
                           "position where the 3D model should appear, then drag " +
                           "it into the 'Spawn Point' slot in this component's Inspector.");
            return;
        }

        CharacterData data = characters[modelOutputIndex];

        // IMPROVED: now tells you exactly which index slot is null
        if (data == null)
        {
            Debug.LogError($"[ModelDisplay] CharacterData slot [{modelOutputIndex}] is NULL. " +
                           $"Open ModelDisplayManager in Inspector, find slot {modelOutputIndex} " +
                           $"in the Characters array, and drag the correct CharData asset into it. " +
                           $"Run 'Validate All Slots' from the gear menu for a full null report.");
            return;
        }

        // IMPROVED: now tells you the asset name and index together
        if (data.packagePrefab == null)
        {
            Debug.LogError($"[ModelDisplay] packagePrefab is NULL in CharacterData asset " +
                           $"'{data.name}' (slot {modelOutputIndex}, char '{data.devanagariChar}'). " +
                           $"Open that CharData asset and assign the 3D prefab to packagePrefab.");
            return;
        }

        // Destroy previous package (unchanged)
        if (_current != null)
        {
            Destroy(_current);
            _current = null;
        }

        // Spawn (unchanged)
        _current = Instantiate(
            data.packagePrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        Debug.Log($"[ModelDisplay] ✅ Spawned: '{data.devanagariChar}' ({data.phoneticName}) " +
                  $"at index {modelOutputIndex} — prefab: {data.packagePrefab.name}");

        // Wire PackageController audio (unchanged)
        PackageController pc = _current.GetComponent<PackageController>();
        if (pc != null)
        {
            pc.charAudio  = data.charAudio;
            pc.word1Audio = data.word1Audio;
            pc.word2Audio = data.word2Audio;
            Debug.Log("[ModelDisplay] PackageController audio wired.");
        }

        // Wire DigitController audio (unchanged)
        DigitController dc = _current.GetComponent<DigitController>();
        if (dc != null)
        {
            dc.digitAudio = data.charAudio;
            Debug.Log("[ModelDisplay] DigitController audio wired.");
        }
    }

    // ─── DismissCurrentPackage (unchanged) ────────────────────────────────────
    public void DismissCurrentPackage()
    {
        if (_current != null)
        {
            Destroy(_current);
            _current = null;
        }
        _lastIndex = -1;
        Debug.Log("[ModelDisplay] Package dismissed.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ADDITIONS BELOW — validation and test tools only
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Audits all 46 slots and prints a clear report to Console.
    /// Called automatically at Start() and from the Inspector gear menu.
    ///
    /// HOW TO READ THE OUTPUT:
    ///   ✅ lines = slot is fully configured (CharData + prefab both assigned)
    ///   ⚠️ lines = CharData assigned but packagePrefab is null inside it
    ///   ❌ lines = the entire CharData slot is null in the array
    /// </summary>
    [ContextMenu("Validate All Slots")]
    public void ValidateSetup()
    {
        int nullSlots    = 0;
        int nullPrefabs  = 0;
        int okSlots      = 0;

        Debug.Log("══════════════════════════════════════════════════════");
        Debug.Log("[ModelDisplay] SLOT AUDIT — checking all 46 CharacterData slots");
        Debug.Log("══════════════════════════════════════════════════════");

        // Check spawnPoint first
        if (spawnPoint == null)
            Debug.LogError("[ModelDisplay] ❌ CRITICAL: spawnPoint is NULL — no 3D models will spawn!");
        else
            Debug.Log($"[ModelDisplay] ✅ spawnPoint = '{spawnPoint.name}' at {spawnPoint.position}");

        // Check characters array length
        if (characters == null || characters.Length != 46)
        {
            Debug.LogError($"[ModelDisplay] ❌ CRITICAL: characters array has " +
                           $"{(characters == null ? 0 : characters.Length)} slots — expected 46!");
        }

        int count = characters == null ? 0 : characters.Length;
        for (int i = 0; i < count; i++)
        {
            CharacterData d = characters[i];
            if (d == null)
            {
                Debug.LogError($"[ModelDisplay] ❌ Slot [{i:D2}] — CharacterData is NULL");
                nullSlots++;
            }
            else if (d.packagePrefab == null)
            {
                Debug.LogWarning($"[ModelDisplay] ⚠️  Slot [{i:D2}] '{d.devanagariChar}' ({d.phoneticName}) " +
                                 $"— CharData OK but packagePrefab is NULL in asset '{d.name}'");
                nullPrefabs++;
            }
            else
            {
                Debug.Log($"[ModelDisplay] ✅ Slot [{i:D2}] '{d.devanagariChar}' ({d.phoneticName}) " +
                          $"— prefab: {d.packagePrefab.name}");
                okSlots++;
            }
        }

        Debug.Log("══════════════════════════════════════════════════════");
        Debug.Log($"[ModelDisplay] AUDIT RESULT: " +
                  $"✅ {okSlots} OK  |  " +
                  $"⚠️  {nullPrefabs} missing prefab  |  " +
                  $"❌ {nullSlots} null slots");

        if (nullSlots == 0 && nullPrefabs == 0 && spawnPoint != null)
            Debug.Log("[ModelDisplay] 🎉 All 46 slots are fully configured. 3D display should work.");
        else
            Debug.LogError("[ModelDisplay] Fix the ❌/⚠️ issues above then re-run 'Validate All Slots'.");

        Debug.Log("══════════════════════════════════════════════════════");
    }

    /// <summary>
    /// Test-spawns a specific index directly from the Inspector.
    /// Right-click the ModelDisplayManager component gear icon → "Test Spawn Index 0"
    /// Use this to confirm spawning works without needing a scan result.
    /// Change the testIndex value below to test a different character.
    /// </summary>
    [ContextMenu("Test Spawn Index 0")]
    void TestSpawnIndex0() => TestSpawn(0);

    [ContextMenu("Test Spawn Index 22 (Ba)")]
    void TestSpawnBa()     => TestSpawn(22);

    [ContextMenu("Test Spawn Index 32 (Ha)")]
    void TestSpawnHa()     => TestSpawn(32);

    [ContextMenu("Dismiss Current Package")]
    void TestDismiss()     => DismissCurrentPackage();

    void TestSpawn(int index)
    {
        Debug.Log($"[ModelDisplay] Test spawning index {index}...");
        _lastIndex = -1; // force reload even if same index
        ShowCharacter(index);
    }
}
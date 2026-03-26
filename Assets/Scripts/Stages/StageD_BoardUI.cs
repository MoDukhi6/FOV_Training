using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Stage D Board UI:
/// - Shows a background (e.g., fridge)
/// - Shows N item sprites inside fixed slot Images
/// - No motion (Stage E will add motion later)
/// </summary>
public class StageD_BoardUI : MonoBehaviour
{
    [Header("Background")]
    public Image backgroundImg;                 // Img_Background (optional)
    public Sprite backgroundSprite;             // fridge sprite (optional if already set on Image)

    [Header("Slots (fixed positions in the fridge)")]
    public Image[] slotImages;                  // Slot_1..Slot_N Images

    [Header("Tuning")]
    [Tooltip("How many pixels represent 1 degree (used if you want sizeDeg -> px scaling).")]
    public float pixelsPerDeg = 40f;

    [Tooltip("If true, the UI object is activated/deactivated by the stage scripts.")]
    public bool autoActivate = false;

    private void Awake()
    {
        if (backgroundImg != null && backgroundSprite != null)
            backgroundImg.sprite = backgroundSprite;
    }

    /// <summary>Enable the board (optional convenience).</summary>
    public void ShowBoard()
    {
        if (autoActivate) gameObject.SetActive(true);
    }

    /// <summary>Disable the board (optional convenience).</summary>
    public void HideBoard()
    {
        if (autoActivate) gameObject.SetActive(false);
    }

    /// <summary>Hide all item slots (keeps background visible).</summary>
    public void ClearSlots()
    {
        if (slotImages == null) return;

        for (int i = 0; i < slotImages.Length; i++)
        {
            if (!slotImages[i]) continue;
            slotImages[i].enabled = false;
            slotImages[i].sprite = null;
        }
    }

    /// <summary>Hide everything (background + items).</summary>
    public void HideAll()
    {
        if (backgroundImg) backgroundImg.enabled = false;
        ClearSlots();
    }

    /// <summary>Show background + clear items.</summary>
    public void ShowBackground()
    {
        if (backgroundImg) backgroundImg.enabled = true;
        ClearSlots();
    }

    /// <summary>
    /// Set items into slots. If items.Length < slotImages.Length, remaining slots are hidden.
    /// If items.Length > slotImages.Length, extra items are ignored.
    /// </summary>
    public void ShowItems(Sprite[] items, Color? tint = null)
    {
        if (backgroundImg) backgroundImg.enabled = true;

        if (slotImages == null || slotImages.Length == 0)
        {
            Debug.LogError("StageD_BoardUI: slotImages[] is empty.");
            return;
        }

        // Clear first
        ClearSlots();

        if (items == null) return;

        int n = Mathf.Min(items.Length, slotImages.Length);
        Color c = tint ?? Color.white;

        for (int i = 0; i < n; i++)
        {
            var img = slotImages[i];
            if (!img) continue;

            img.sprite = items[i];
            img.color = c;
            img.enabled = (items[i] != null);
        }
    }

    /// <summary>
    /// Optional: set slot size in degrees (applies to all slots).
    /// Use if you want Stage D items to change size like A/B.
    /// </summary>
    public void SetSlotSizeDeg(float sizeDeg, float minPx = 24f)
    {
        if (slotImages == null) return;

        float px = Mathf.Max(minPx, sizeDeg * pixelsPerDeg);
        Vector2 sd = new Vector2(px, px);

        for (int i = 0; i < slotImages.Length; i++)
        {
            if (!slotImages[i]) continue;
            slotImages[i].rectTransform.sizeDelta = sd;
        }
    }

    /// <summary>Get slot count safely.</summary>
    public int SlotCount => (slotImages == null) ? 0 : slotImages.Length;
}

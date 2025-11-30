using System.Collections;
using UnityEngine;

public class ButtonSlideAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Direction from which the button will slide in")]
    public SlideDirection slideDirection = SlideDirection.Left;
    
    [Tooltip("Distance offset from the final position (in pixels for UI elements)")]
    public float offsetAmount = 300f;
    
    [Tooltip("Duration of the slide animation in seconds")]
    public float animationDuration = 0.5f;
    
    [Tooltip("Delay before starting the animation")]
    public float startDelay = 0f;
    
    [Header("Easing")]
    [Tooltip("Animation curve for easing (leave default for smooth ease-out)")]
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Options")]
    [Tooltip("Animate on Enable (useful for UI panels that are activated later)")]
    public bool animateOnEnable = true;

    private RectTransform rectTransform;
    private Vector2 finalPosition;
    private bool hasAnimated = false;

    public enum SlideDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform == null)
        {
            Debug.LogError($"ButtonSlideAnimation requires a RectTransform component on {gameObject.name}");
            enabled = false;
            return;
        }
        
        // Store the final position (the position set in the editor)
        finalPosition = rectTransform.anchoredPosition;
    }

    private void Start()
    {
        if (animateOnEnable)
        {
            PlayAnimation();
        }
    }

    private void OnEnable()
    {
        // Only animate on enable if the flag is set and we've already animated once
        // (to handle UI elements that get disabled and re-enabled)
        if (animateOnEnable && hasAnimated && rectTransform != null)
        {
            PlayAnimation();
        }
    }

    /// <summary>
    /// Manually trigger the animation (useful for scripted animations)
    /// </summary>
    public void PlayAnimation()
    {
        if (rectTransform == null) return;
        
        StopAllCoroutines();
        StartCoroutine(SlideInCoroutine());
    }

    /// <summary>
    /// Reset the button to its starting position without animating
    /// </summary>
    public void ResetToStartPosition()
    {
        if (rectTransform == null) return;
        
        Vector2 startPosition = GetStartPosition();
        rectTransform.anchoredPosition = startPosition;
    }

    private Vector2 GetStartPosition()
    {
        Vector2 startPosition = finalPosition;
        
        switch (slideDirection)
        {
            case SlideDirection.Left:
                startPosition.x -= offsetAmount;
                break;
            case SlideDirection.Right:
                startPosition.x += offsetAmount;
                break;
            case SlideDirection.Top:
                startPosition.y += offsetAmount;
                break;
            case SlideDirection.Bottom:
                startPosition.y -= offsetAmount;
                break;
        }
        
        return startPosition;
    }

    private IEnumerator SlideInCoroutine()
    {
        // Wait for start delay
        if (startDelay > 0)
        {
            yield return new WaitForSeconds(startDelay);
        }

        // Set initial position
        Vector2 startPosition = GetStartPosition();
        rectTransform.anchoredPosition = startPosition;

        // Animate from start position to final position
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / animationDuration);
            
            // Apply easing curve
            float easedTime = easingCurve.Evaluate(normalizedTime);
            
            // Lerp between start and final position
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, finalPosition, easedTime);
            
            yield return null;
        }
        
        // Ensure we end exactly at the final position
        rectTransform.anchoredPosition = finalPosition;
        hasAnimated = true;
    }

    // Editor helper - visualize the animation path in the Scene view
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;
        }

        // Store current position
        Vector2 currentPos = rectTransform.anchoredPosition;
        
        // Calculate start position based on current settings
        Vector2 startPos = currentPos;
        switch (slideDirection)
        {
            case SlideDirection.Left:
                startPos.x -= offsetAmount;
                break;
            case SlideDirection.Right:
                startPos.x += offsetAmount;
                break;
            case SlideDirection.Top:
                startPos.y += offsetAmount;
                break;
            case SlideDirection.Bottom:
                startPos.y -= offsetAmount;
                break;
        }

        // Draw a line showing the animation path
        Vector3 worldCurrent = rectTransform.TransformPoint(currentPos);
        Vector3 worldStart = rectTransform.TransformPoint(startPos);
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(worldStart, worldCurrent);
        Gizmos.DrawWireSphere(worldStart, 5f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(worldCurrent, 5f);
    }
#endif
}


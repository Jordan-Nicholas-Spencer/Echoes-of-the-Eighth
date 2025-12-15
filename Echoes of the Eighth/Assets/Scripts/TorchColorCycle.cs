using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchColorCycle : MonoBehaviour
{
    public int torchIndex;  // 0, 1, 2
    
    public TorchColorPuzzle puzzleManager;

    [Header("Torch Colors")]
    public Color[] colors = new Color[5];  // Assign 5 colors in Inspector

    [Header("Light Reference")]
    public Light fireLight;                // The flame light

    public int CurrentColorIndex { get; private set; } = 0;

    private void Start()
    {
        if (fireLight == null)
            fireLight = GetComponentInChildren<Light>();

        if (puzzleManager == null)
            puzzleManager = GetComponentInParent<TorchColorPuzzle>();

        puzzleManager.RegisterTorch(this);
        UpdateLight();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fireball"))
        {
            Debug.Log("JO");
            CycleColor();
            Destroy(other.gameObject);

            puzzleManager.CheckCombination();
        }
        else {
            Debug.Log("JIO");
        }
    }

    private void CycleColor()
    {
        CurrentColorIndex = (CurrentColorIndex + 1) % colors.Length;
        UpdateLight();
    }

    private void UpdateLight()
    {
        if (fireLight != null)
            fireLight.color = colors[CurrentColorIndex];
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchColorCycle : MonoBehaviour {
    public int torchIndex;  // 0, 1, 2
    
    public TorchColorPuzzle puzzleManager;

    [Header("Torch Colors")]
    public Color[] colors = new Color[6];  // 5 colors in inspector

    [Header("Light Reference")]
    public Light fireLight;                // child light object from torch

    public int CurrentColorIndex { get; private set; } = 0;

    private void Start() {
        if (fireLight == null)
            fireLight = GetComponentInChildren<Light>();

        if (puzzleManager == null)
            puzzleManager = GetComponentInParent<TorchColorPuzzle>();

        puzzleManager.RegisterTorch(this);
        UpdateLight();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Fireball")) {
            Debug.Log("Changing color");
            CycleColor();
            Destroy(other.gameObject);

            puzzleManager.CheckCombination();
        }
    }

    private void CycleColor() {
        CurrentColorIndex = (CurrentColorIndex + 1) % colors.Length;
        UpdateLight();
    }

    private void UpdateLight() {
        if (fireLight != null)
            fireLight.color = colors[CurrentColorIndex];
    }
}

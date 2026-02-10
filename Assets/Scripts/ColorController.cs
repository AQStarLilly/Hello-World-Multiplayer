using UnityEngine;

public class ColorController : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color taggerColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;

    private MaterialPropertyBlock block;

    private void Awake()
    {
        block = new MaterialPropertyBlock();
        SetColor(false);
    }

    public void SetColor(bool isTagger)
    {
        if (targetRenderer == null)
            return;

        targetRenderer.GetPropertyBlock(block);

        block.SetColor("_BaseColor", isTagger ? taggerColor : normalColor);

        targetRenderer.SetPropertyBlock(block);
    }

}

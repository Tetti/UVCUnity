using UnityEngine;
using UnityEngine.UI;

namespace Tetti.UVCUnity.UI
{
    [DisallowMultipleComponent]
    public sealed class RawImagePreviewView : MonoBehaviour
    {
        [SerializeField] private RawImage rawImage;
        [SerializeField] private Color placeholderColor = new Color(0.16f, 0.16f, 0.16f, 1f);

        private Texture2D placeholderTexture;

        public void BindTexture(Texture texture)
        {
            EnsureRawImage();
            rawImage.texture = texture;
            rawImage.color = Color.white;
        }

        public void ShowPlaceholder(string label)
        {
            EnsureRawImage();
            EnsurePlaceholderTexture();
            rawImage.texture = placeholderTexture;
            rawImage.color = Color.white;
            Debug.Log($"[UVCUnity] Showing placeholder preview for {label}.");
        }

        public void Clear()
        {
            EnsureRawImage();
            EnsurePlaceholderTexture();
            rawImage.texture = placeholderTexture;
            rawImage.color = Color.white;
        }

        private void EnsureRawImage()
        {
            if (rawImage == null)
            {
                rawImage = GetComponent<RawImage>();
            }
        }

        private void EnsurePlaceholderTexture()
        {
            if (placeholderTexture != null)
            {
                return;
            }

            placeholderTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                name = "UvcPreviewPlaceholder",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };

            var pixels = new[]
            {
                placeholderColor, placeholderColor,
                placeholderColor, placeholderColor,
            };
            placeholderTexture.SetPixels(pixels);
            placeholderTexture.Apply(false, true);
        }
    }
}

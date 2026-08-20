using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TUFHelper.Utils
{
    public static class ImageUtils
    {
        private static readonly HashSet<Sprite> CreatedSprites = new();

        public static async Task LoadImageToUIAsync(string url, Image targetImage, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(url) || targetImage == null) return;

            Sprite newSprite = await DownloadSpriteAsync(url, token);

            if (newSprite == null || targetImage == null) return;

            if (targetImage.sprite != null && CreatedSprites.Contains(targetImage.sprite))
            {
                CreatedSprites.Remove(targetImage.sprite);

                Texture2D oldTexture = targetImage.sprite.texture;
                Sprite oldSprite = targetImage.sprite;

                targetImage.sprite = null;

                if (oldTexture != null) UnityEngine.Object.Destroy(oldTexture);
                if (oldSprite != null) UnityEngine.Object.Destroy(oldSprite);
            }
            CreatedSprites.Add(newSprite);
            targetImage.sprite = newSprite;
        }

        public static async Task<Sprite> DownloadSpriteAsync(string url, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(url)) return null;

            try
            {
                HttpResponseMessage response = await Main.Client.GetAsync(url, HttpCompletionOption.ResponseContentRead, token);
                response.EnsureSuccessStatusCode();

                byte[] imageData = await response.Content.ReadAsByteArrayAsync();
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

                if (texture.LoadImage(imageData))
                {
                    Rect rect = new Rect(0, 0, texture.width, texture.height);
                    Vector2 pivot = new Vector2(0.5f, 0.5f);
                    return Sprite.Create(texture, rect, pivot);
                }

                UnityEngine.Object.Destroy(texture);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Main.Logger.Error($"[ImageUtils] Failed to load image from {url}: {ex.Message}");
            }

            return null;
        }
    }
}
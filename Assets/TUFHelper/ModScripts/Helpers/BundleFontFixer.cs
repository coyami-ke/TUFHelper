using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TUFHelper
{
    public static class BundleFontFixer
    {
        private static bool initialized;
        private static TMP_FontAsset runtimeFontAsset;
        private static TMP_SpriteAsset defaultSpriteAsset;
        private static readonly HashSet<int> failedTextObjects = new();

        public static void Init()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public static void FixFontsIn(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            TMP_FontAsset font = GetRuntimeFontAsset();
            if (font == null)
            {
                return;
            }

            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                FixText(text, font);
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!scene.name.Contains("TUF", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                FixFontsIn(root);
            }
        }

        private static void FixText(TMP_Text text, TMP_FontAsset font)
        {
            if (text == null)
            {
                return;
            }

            try
            {
                text.font = font;
                if (font.material != null)
                {
                    text.fontSharedMaterial = font.material;
                }

                if (defaultSpriteAsset != null && text.spriteAsset == null)
                {
                    text.spriteAsset = defaultSpriteAsset;
                }

                text.SetAllDirty();
            }
            catch (Exception ex)
            {
                int id = text.GetInstanceID();
                if (failedTextObjects.Add(id))
                {
                    Main.Logger?.Error($"Failed to fix TMP font on '{text.name}': {ex.GetType().Name} - {ex.Message}");
                }
            }
        }

        private static TMP_FontAsset GetRuntimeFontAsset()
        {
            if (runtimeFontAsset != null)
            {
                return runtimeFontAsset;
            }

            runtimeFontAsset = FindBundledTmpFont() ?? CreateFontAssetFromFont();
            if (runtimeFontAsset != null)
            {
                SanitizeFontAsset(runtimeFontAsset);
            }

            defaultSpriteAsset = TMP_Settings.defaultSpriteAsset
                ?? Resources.FindObjectsOfTypeAll<TMP_SpriteAsset>().FirstOrDefault();

            return runtimeFontAsset;
        }

        private static TMP_FontAsset FindBundledTmpFont()
        {
            IEnumerable<TMP_FontAsset> fonts = Main.assets != null
                ? Main.assets.LoadAllAssets<TMP_FontAsset>()
                : Enumerable.Empty<TMP_FontAsset>();

            return fonts.FirstOrDefault(IsGMarketFont)
                ?? fonts.FirstOrDefault(font => font != null && font.material != null);
        }

        private static TMP_FontAsset CreateFontAssetFromFont()
        {
            Font sourceFont = FindSourceFont();
            if (sourceFont == null)
            {
                return TMP_Settings.defaultFontAsset;
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
            if (fontAsset != null)
            {
                fontAsset.name = "TUFHelper_Runtime_GMarket";
            }

            return fontAsset;
        }

        private static Font FindSourceFont()
        {
            IEnumerable<Font> bundledFonts = Main.assets != null
                ? Main.assets.LoadAllAssets<Font>()
                : Enumerable.Empty<Font>();

            return bundledFonts.FirstOrDefault(IsGMarketFont)
                ?? Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault(IsGMarketFont)
                ?? bundledFonts.FirstOrDefault()
                ?? Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault();
        }

        private static bool IsGMarketFont(UnityEngine.Object obj)
        {
            return obj != null && obj.name.IndexOf("Gmarket", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void SanitizeFontAsset(TMP_FontAsset fontAsset)
        {
            if (fontAsset == null)
            {
                return;
            }

            if (fontAsset.material == null)
            {
                Font sourceFont = FindSourceFont();
                if (sourceFont != null)
                {
                    TMP_FontAsset replacement = TMP_FontAsset.CreateFontAsset(sourceFont);
                    if (replacement != null)
                    {
                        runtimeFontAsset = replacement;
                        fontAsset = replacement;
                    }
                }
            }

            fontAsset.fallbackFontAssetTable ??= new List<TMP_FontAsset>();
            fontAsset.fallbackFontAssetTable.RemoveAll(font => font == null || font.material == null);
        }
    }
}

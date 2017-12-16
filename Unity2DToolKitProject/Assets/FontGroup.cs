using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Bomber Crew/Font Group")]
public class FontGroup : ScriptableObject
{
    // Fields
    [SerializeField]
    private tk2dFontData m_baseFont;
    private tk2dFontData m_cached;
    private bool m_hasCached;
    private ControlPromptDisplayHelpers.ControllerMapSpriteType m_lastControllerType;
    private int m_lastLanguageFlag = -1;
    [SerializeField]
    private FontTextureReplacement[] m_replacements;

    // Methods
    public tk2dFontData GetReplacementData()
    {
        int num = 1;
        ControlPromptDisplayHelpers.ControllerMapSpriteType mapSpriteType = ControlPromptDisplayHelpers.GetMapSpriteType();
        if (this.m_hasCached && ((this.m_lastLanguageFlag != num) || (mapSpriteType != this.m_lastControllerType)))
        {
            this.m_hasCached = false;
        }
        if (!this.m_hasCached)
        {
            this.m_cached = this.m_baseFont;
            foreach (FontTextureReplacement replacement in this.m_replacements)
            {
                if ((replacement.GetLanguageFlag() & num) != 0)
                {
                    this.m_cached = replacement.GetFont(mapSpriteType);
                    replacement.SetControllerTexture(this.m_cached, mapSpriteType);
                }
            }
            this.m_hasCached = true;
            this.m_lastLanguageFlag = num;
            this.m_lastControllerType = mapSpriteType;
        }
        return this.m_cached;
    }

    // Nested Types
    [System.Serializable]
    public class ControllerTypeReplacement
    {
        // Fields
        [SerializeField]
        public tk2dFontData m_font;
        [SerializeField]
        public Texture2D m_replacementTexture;
        [SerializeField]
        public ControlPromptDisplayHelpers.ControllerMapSpriteType m_spriteType;
    }

    [System.Serializable]
    public class FontTextureReplacement
    {
        // Fields
        [SerializeField]
        private tk2dFontData m_font;
        [SerializeField]
        private int m_forLanguageFlags;
        [SerializeField]
        private FontGroup.ControllerTypeReplacement[] m_replacement;

        // Methods
        public tk2dFontData GetFont(ControlPromptDisplayHelpers.ControllerMapSpriteType cType)
        {
            tk2dFontData font = this.m_font;
            foreach (FontGroup.ControllerTypeReplacement replacement in this.m_replacement)
            {
                if (replacement.m_spriteType == cType)
                {
                    if (replacement.m_font != null)
                    {
                        font = replacement.m_font;
                    }
                    return font;
                }
            }
            return font;
        }

        public int GetLanguageFlag()
        {
            return this.m_forLanguageFlags;
        }

        public void SetControllerTexture(tk2dFontData font, ControlPromptDisplayHelpers.ControllerMapSpriteType cType)
        {
            foreach (FontGroup.ControllerTypeReplacement replacement in this.m_replacement)
            {
                if (replacement.m_spriteType == cType)
                {
                    if (replacement.m_replacementTexture != null)
                    {
                        font.materialInst.mainTexture = replacement.m_replacementTexture;
                    }
                    break;
                }
            }
        }
    }
}

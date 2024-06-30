using System.Collections.Generic;
using UnityEngine;

namespace SMC
{
    /// <summary>
    /// Utility class providing texture combination functionality by creating a fixed-size texture atlas.
    /// </summary>
    public static class TextureCombiner
    {
        /// <summary>
        /// Combines given textures into a single texture based on the given array of textures.
        /// </summary>
        /// <param name="texturesToCombine">Array of textures to be combined</param>
        /// <param name="combinedTexture">Combined texture will be assigned to this variable</param>
        /// <param name="textureCoordinates">A dictionary of atlas coordinates using the textures as a key</param>
        /// <returns>bool Texture combination operation success</returns>
        public static bool CombineTextures(Texture2D[] texturesToCombine, out Texture2D combinedTexture, out Dictionary<Texture2D, Vector2Int> textureCoordinates, out int atlasResolution)
        {
            atlasResolution = 0;

            if (texturesToCombine?.Length > 1)
            {
                textureCoordinates = new Dictionary<Texture2D, Vector2Int>();

                /*
                    The texture resolution is defined by the first texture, and is expected to be uniform
                    ex. 256x256, 512x512...
                */
                int textureResolution = texturesToCombine[0].width;
                atlasResolution = Mathf.Max(2, Mathf.NextPowerOfTwo(texturesToCombine.Length) / 4);
                int atlasSize = atlasResolution * textureResolution;

                Color32[] cols = new Color32[atlasSize * atlasSize];
                for (int x = 0; x < atlasSize; x++)
                {
                    for (int y = 0; y < atlasSize; y++)
                    {
                        int currentCellX = x / textureResolution, currentCellY = y / textureResolution;

                        int i = (currentCellY * atlasResolution) + currentCellX;
                        if (i < texturesToCombine.Length)
                        {
                            Texture2D tex = texturesToCombine[i];
                            textureCoordinates[tex] = new Vector2Int(currentCellX, currentCellY);

                            int currentPixelX = x - (currentCellX * textureResolution), currentPixelY = y - (currentCellY * textureResolution);
                            cols[(atlasSize * y) + x] = tex.GetPixel(currentPixelX + 1, currentPixelY + 1);
                        }
                        else
                        {
                            cols[(atlasSize * y) + x] = Color.black;
                        }
                    }
                }

                combinedTexture = new Texture2D(atlasSize, atlasSize);
                combinedTexture.SetPixels32(cols, 0);
                combinedTexture.Apply();

                return true;
            }

            combinedTexture = null;
            textureCoordinates = null;

            return false;
        }
    }
}

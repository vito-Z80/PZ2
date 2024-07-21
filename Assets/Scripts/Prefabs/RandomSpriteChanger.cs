using System.Linq;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Prefabs
{
    [ExecuteInEditMode]
    public class RandomSpriteChanger : MonoBehaviour
    {
        SpriteResolver m_spriteResolver;

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                m_spriteResolver = GetComponent<SpriteResolver>();
                if (m_spriteResolver != null)
                {
                    ChangeSpriteRandomly();
                }
            }
        }

        [ContextMenu("Change Sprite Randomly")]
        private void ChangeSpriteRandomly()
        {
            var category = m_spriteResolver.GetCategory();
            var spriteLabels = m_spriteResolver.spriteLibrary.spriteLibraryAsset.GetCategoryLabelNames(category).ToList();
            if (spriteLabels.Count > 0)
            {
                var randomLabel = spriteLabels[Random.Range(0, spriteLabels.Count)];
                m_spriteResolver.SetCategoryAndLabel(category, randomLabel);
            }
        }
    }
}
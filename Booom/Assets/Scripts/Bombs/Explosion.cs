using UnityEngine;

public class Explosion : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    public void initializeExplosion(Color explosionColor)
    {
        Destroy(gameObject, 1f);

        ApplyExplosionColor(explosionColor);

        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform child = transform.GetChild(i);
            var particleSystem = child.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.Play();
            }
        }
    }

    private void ApplyExplosionColor(Color explosionColor)
    {
        Color hdrExplosionColor = explosionColor * 5f;

        foreach (Renderer explosionRenderer in GetComponentsInChildren<Renderer>(true))
        {
            foreach (Material material in explosionRenderer.materials)
            {
                if (material.HasProperty(BaseColorId))
                {
                    material.SetColor(BaseColorId, explosionColor);
                }

                if (material.HasProperty(ColorId))
                {
                    material.SetColor(ColorId, explosionColor);
                }

                if (!material.HasProperty(EmissionColorId))
                {
                    continue;
                }

                material.EnableKeyword("_EMISSION");
                material.SetColor(EmissionColorId, hdrExplosionColor);
            }
        }
    }
}

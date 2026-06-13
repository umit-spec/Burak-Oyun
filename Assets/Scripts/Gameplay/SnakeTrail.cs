using UnityEngine;
using UnityEngine.Rendering;

namespace BurakOyun.Gameplay
{
    public class SnakeTrail : MonoBehaviour
    {
        private void Awake()
        {
            TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();

            trail.time = 0.5f;
            trail.startWidth = 0.7f;
            trail.endWidth = 0.01f;

            trail.shadowCastingMode = ShadowCastingMode.Off;
            trail.receiveShadows = false;

            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                         ?? Shader.Find("Sprites/Default");
            Material mat = new Material(shader);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.renderQueue = 3000;
            trail.material = mat;

            Gradient gradient = new Gradient();

            GradientColorKey[] colorKeys = new GradientColorKey[5];
            colorKeys[0] = new GradientColorKey(Color.red, 0f);
            colorKeys[1] = new GradientColorKey(new Color(1f, 0.5f, 0f), 0.25f);
            colorKeys[2] = new GradientColorKey(Color.green, 0.5f);
            colorKeys[3] = new GradientColorKey(Color.cyan, 0.75f);
            colorKeys[4] = new GradientColorKey(Color.blue, 1f);

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(0.8f, 0.5f);
            alphaKeys[2] = new GradientAlphaKey(0f, 1f);

            gradient.SetKeys(colorKeys, alphaKeys);
            trail.colorGradient = gradient;

            trail.textureMode = LineTextureMode.Tile;
        }
    }
}

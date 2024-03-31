using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AnimationBundles : MonoBehaviour
{
    public List<AnimationSpriteBundle> bundles;

    public void CreateDefaultAnimationBundle()
    {
        AnimationSpriteBundle bundle = new AnimationSpriteBundle
        {
            ID = bundles.Count,
            texturePacks = new AnimationPacks[]
            {
                new AnimationPacks("idle_01"),
                new AnimationPacks("idle_02"),
                new AnimationPacks("walk_01"),
                new AnimationPacks("walk_02"),
                new AnimationPacks("run_01"),
                new AnimationPacks("run_02"),
                new AnimationPacks("frenar_01"),
                new AnimationPacks("jumpUp_01"),
                new AnimationPacks("jumpDown_01"),
                new AnimationPacks("glideStart_01"),
                new AnimationPacks("glideStart_02"),
                new AnimationPacks("glideStart_03"),
                new AnimationPacks("glideStart_04"),
                new AnimationPacks("glideStart_05"),
                new AnimationPacks("glideStart_06"),
                new AnimationPacks("glideStart_07"),
                new AnimationPacks("glideIdle_01"),
                new AnimationPacks("menuIdle_01"),
                new AnimationPacks("menuIdle_02"),
            }
        };

        bundles.Add(bundle);
    }

    public void CreateMashAnimationBundle()
    {
        AnimationSpriteBundle bundle = new AnimationSpriteBundle
        {
            ID = bundles.Count,
            texturePacks = new AnimationPacks[]
            {
                new AnimationPacks("mash_01"),
                new AnimationPacks("mash_02")
            }
        };

        bundles.Add(bundle);
    }
}

[System.Serializable]
public struct AnimationSpriteBundle
{
    public string Name;
    public int ID;
    public AnimationPacks[] texturePacks; 
}

#if UNITY_EDITOR_WIN
[CustomEditor(typeof(AnimationBundles))]
class AnimationBundleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        AnimationBundles myscript = (AnimationBundles)target;

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Add default SALA ESPERA pack", GUILayout.Height(30)))
        {
            myscript.CreateDefaultAnimationBundle();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Add MASH Minigame pack", GUILayout.Height(30)))
        {
            myscript.CreateMashAnimationBundle();
        }
        GUILayout.EndHorizontal();
    }
}

#endif

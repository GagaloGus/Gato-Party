using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum SkinNames
{
    Sko, Beeko, Misheow, News, test1, test2
}

public class AnimationBundles : MonoBehaviour
{
    public List<AnimationSpriteBundle> bundles;
#if UNITY_EDITOR_WIN
    [MenuItem("GameObject/Custom Objects/Animation Bundles")]
    static void CreateAnimationBundleObject(MenuCommand menuCommand)
    {
        // Crea un nuevo GameObject y añade el componente MyCustomObject
        GameObject newObject = new GameObject("MyCustomObject");
        newObject.name = "Animation Bundles";
        newObject.AddComponent<AnimationBundles>();

        // Registra el nuevo GameObject en la escena
        GameObjectUtility.SetParentAndAlign(newObject, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(newObject, "Create " + newObject.name);
        Selection.activeObject = newObject;
    }
#endif
    void CreatePack(AnimationPacks[] newPack)
    {
        AnimationSpriteBundle bundle = new AnimationSpriteBundle
        {
            Name = Enum.GetName(typeof(SkinNames), bundles.Count).ToString(),
            ID = bundles.Count,
            texturePacks = newPack
        };

        bundles.Add(bundle);
    }

    public void CreateDefaultAnimationBundle()
    {
        AnimationPacks[] texturePacks = new AnimationPacks[]
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
        };

        CreatePack(texturePacks);
    }

    public void CreateCommandAnimationBundle()
    {
        AnimationPacks[] texturePacks = new AnimationPacks[]
        {
            new AnimationPacks("recieve_01"),
            new AnimationPacks("recieve_02"),
            new AnimationPacks("idle_01"),
            new AnimationPacks("idle_02"),
            new AnimationPacks("throw_01"),
            new AnimationPacks("death_01"),
            new AnimationPacks("death_02")
        };

        CreatePack(texturePacks);
    }

    public void CreateMashAnimationBundle()
    {
        AnimationPacks[] texturePacks = new AnimationPacks[]
        {
            new AnimationPacks("mash_01"),
            new AnimationPacks("mash_02")
        };

        CreatePack(texturePacks);
    }

    public void CreateLastSecAnimationBundle()
    {
        AnimationPacks[] texturePacks = new AnimationPacks[]
        {
            new AnimationPacks("lookUp_01"),
            new AnimationPacks("idleScared_01"),
            new AnimationPacks("idleScared_02"),
            new AnimationPacks("press_01"),
            new AnimationPacks("bonked_01"),
        };

        CreatePack(texturePacks);
    }

    public void CreateFinalScoreAnimationBundle()
    {
        AnimationPacks[] texturePacks = new AnimationPacks[]
        {
            new AnimationPacks("idle_01"),
            new AnimationPacks("idle_02"),
            new AnimationPacks("jumpDown_01"),
            new AnimationPacks("fall_01"),
            new AnimationPacks("fall_02"),
            new AnimationPacks("victory_01"),
            new AnimationPacks("victory_02")

        };

        CreatePack(texturePacks);
    }

    public void CopyFirstTextureToSecond()
    {
        foreach (AnimationSpriteBundle bundle in bundles)
        {
            foreach (AnimationPacks pack in bundle.texturePacks)
            {
                pack.back = pack.front;
            }
        }
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
        if (GUILayout.Button("Add SALA ESPERA pack", GUILayout.Height(30)))
        {
            myscript.CreateDefaultAnimationBundle();
        }
        GUILayout.EndHorizontal();

        //Minijuegos
        int height = 30;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add MASH Minigame pack", GUILayout.Height(height)))
        {
            myscript.CreateMashAnimationBundle();
        }
        if (GUILayout.Button("Add COMMAND Minigame pack", GUILayout.Height(height)))
        {
            myscript.CreateCommandAnimationBundle();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add LAST SEC Minigame pack", GUILayout.Height(height)))
        {
            myscript.CreateLastSecAnimationBundle();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add FINAL SCORE pack", GUILayout.Height(30)))
        {
            myscript.CreateFinalScoreAnimationBundle();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy FIRST texture to SECOND texture", GUILayout.Height(20)))
        {
            myscript.CopyFirstTextureToSecond();
        }
        GUILayout.EndHorizontal();
    }
}

#endif

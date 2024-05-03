using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AutoHingeBone : MonoBehaviour
{
    public Transform boneParent;

    List<GameObject> bones = new List<GameObject>();

    public void AutoHingeRBs()
    {
        bones.Clear();
        GetChild(boneParent);

        for (int i = bones.Count-1; i > 0; i--) 
        {
            if(i < bones.Count)
            {
                GameObject bone = bones[i], parentBone = bones[i-1];

                HingeJoint hingeJoint = bone.GetComponent<HingeJoint>();
                Rigidbody parentRigidbody = parentBone.GetComponent<Rigidbody>();

                if (hingeJoint != null && parentRigidbody != null)
                {
                    hingeJoint.connectedBody = parentRigidbody;
                }
                else
                {
                    Debug.LogError("HingeJoint or Rigidbody component is missing on bone or parent bone.");
                }
            }
        }

        Debug.Log("hinged!");
    }

    void GetChild(Transform parent)
    {
        if(parent.childCount > 0)
        {
            GameObject child = parent.GetChild(0).gameObject;

            bones.Add(child);

            GetChild(child.transform);
        }
    }
}

#if UNITY_EDITOR_WIN
[CustomEditor(typeof(AutoHingeBone))]
class AutoHingeBoneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AutoHingeBone myscript = (AutoHingeBone)target;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Hinge all childs", GUILayout.Height(30)))
        {
            myscript.AutoHingeRBs();
        }
        GUILayout.EndHorizontal();
    }
}
#endif
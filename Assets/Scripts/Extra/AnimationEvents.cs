using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public void DisableAnimator()
    {
        GetComponent<Animator>().enabled = false;
    }

    public void DeactivateObject()
    {
        gameObject.SetActive(false);
    }

    public void DestroyGameObj()
    {
        Destroy(gameObject);
    }

    public void PlaySFX3D(AudioClip clip)
    {
        AudioManager.instance.PlaySFX3D(clip, transform.position);
    }

    public void PlaySFX2D(AudioClip clip)
    {
        AudioManager.instance.PlaySFX2D(clip);
    }
}

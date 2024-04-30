using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGFindItem_Cofre : MonoBehaviour
{
    public enum LataState { None, Normal, Golden}
    public LataState lataState;

    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void UpdateLata(LataState lata)
    {
        lataState = lata;

        GameObject lataNormal = transform.Find("Lata").Find("lata_normal").gameObject;
        GameObject lataGolden = transform.Find("Lata").Find("lata_golden").gameObject;

        lataNormal.SetActive(false); 
        lataGolden.SetActive(false);

        switch (lataState) 
        {
            case LataState.None: 
                break;
        
            case LataState.Normal:
                lataNormal.SetActive(true);
                break; 
            
            case LataState.Golden:
                lataGolden.SetActive(true);
                break;
        }

        animator.SetInteger("lata", (int)lataState);
    }

    public void OpenChest()
    {
        animator.SetTrigger("open");
    }
}

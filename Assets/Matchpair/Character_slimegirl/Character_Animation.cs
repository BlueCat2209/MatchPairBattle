using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Animation : MonoBehaviour
{
    public Animator animator;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CastSkill();
        SkillHited();

    }
    
    public void CastSkill()
    {
        if(Input.GetKeyUp(KeyCode.D))
        {
            animator.SetTrigger("Lighted");
        }    

        if(Input.GetKeyUp(KeyCode.F))
        {
            animator.SetTrigger("Icefire");
        }

        if (Input.GetKeyUp(KeyCode.G))
        {
            animator.SetTrigger("Steal");
        }
    }    
    public void SkillHited()
    {

        if (Input.GetKeyUp(KeyCode.A))
        {
            animator.SetTrigger("Blinded");
            StartCoroutine(StartCooldown());
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            animator.SetTrigger("Freezed");
            StartCoroutine(StartCooldown());
        }

        if (Input.GetKeyUp(KeyCode.H))
        {
            animator.SetTrigger("Stealed");
            StartCoroutine(StartCooldown());
        }

    }

    public IEnumerator StartCooldown()
    {        
        yield return new WaitForSeconds(5);        
        animator.SetTrigger("Untrap");
    }    
}

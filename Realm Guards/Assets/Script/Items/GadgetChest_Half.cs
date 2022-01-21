using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GadgetChest_Half : MonoBehaviour, IInteractable
{
    [SerializeField] GadgetChest chest;
    [SerializeField] bool isLeft;
    [SerializeField] BoxCollider boxCollider;
    [Space]
    [SerializeField] ParticleSystem par_start;
    [SerializeField] ParticleSystem par_loop;
    [Space]
    [SerializeField] ParticleSystem par_shape;
    [SerializeField] ParticleSystem par_shape_back;
    [SerializeField] ParticleSystem par_shape2;
    [SerializeField] ParticleSystem par_shape2_back;
    [Space]
    [SerializeField] ParticleSystemRenderer par_shape_ren;
    [SerializeField] ParticleSystemRenderer par_shape_back_ren;
    [SerializeField] ParticleSystemRenderer par_shape2_ren;
    [SerializeField] ParticleSystemRenderer par_shape2_back_ren;

    [HideInInspector] public string interactText;
    public string GetInteractText() => interactText;

    public void Interact(PlayerController p) => chest.InteractHalf(p, isLeft);

    public void OpenHalf(int gadgetIndex)
    {
        Debug.Log("Open chest");
        IGadget gadget = GameManager.gadgetPrefabs[gadgetIndex];

        par_shape.textureSheetAnimation.SetSprite(0, gadget.iconSprite);
        par_shape_ren.material.color = gadget.GetValueColour();
        par_shape_back.textureSheetAnimation.SetSprite(0, gadget.iconBack);
        par_shape_ren.material.color = gadget.GetValueColour();
        par_shape2.textureSheetAnimation.SetSprite(0, gadget.iconSprite);
        par_shape_ren.material.color = gadget.GetValueColour();
        par_shape2_back.textureSheetAnimation.SetSprite(0, gadget.iconBack);
        par_shape_ren.material.color = gadget.GetValueColour();

        boxCollider.enabled = true;

        StartCoroutine(OpenHalfAnim());
    }
    IEnumerator OpenHalfAnim()
    {
        yield return new WaitForSeconds(1.5f);
        par_start.Play(true);
        yield return new WaitForSeconds(0.42f);
        par_start.Pause(true);
        par_start.gameObject.SetActive(false);
        par_loop.Play(true);
        yield return null;
    }

    public void CloseHalf()
    {
        Debug.Log("Close chest");

        boxCollider.enabled = false;
        par_loop.Stop(true);
        par_loop.gameObject.SetActive(false);
        par_start.gameObject.SetActive(true);
        par_start.Play(true);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


[RequireComponent(typeof(BoxCollider))]
public class GadgetBox : NetworkBehaviour, IInteractable
{
    [SerializeField] protected string interactText = " to open the gadget box.";
    [SerializeField] protected Animator anim;
    [SerializeField] protected BoxCollider boxCollider;
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
    [Space]
    [SerializeField] protected AudioSource snd_start;
    [SerializeField] protected AudioSource snd_end;

    [HideInInspector, SyncVar] public int gadget;
    protected bool isOpen = false;

    public virtual string GetInteractText()
    {
        if (gadget == -1)
            return string.Empty;
        else if (isOpen)
            return GameManager.gadgetPrefabs[gadget].interactText;
        else
            return interactText;
    }

    public virtual void Interact(PlayerController p)
    {
        Debug.Log("Interact. " + isOpen + " : " + gadget);

        if (gadget == -1)
            return;

        if (isOpen)
        {
            GameManager.gadgetPrefabs[gadget].ApplyEffect(p);
            p.manager.Cmd_CloseChest(netId, false);
        }
        else
        {
            p.manager.Cmd_OpenChest(netId);
        }
    }

    [ClientRpc] public virtual void Rpc_OpenChest()
    {
        Debug.Log("Open chest");
        isOpen = true;
        anim.SetTrigger("Open");
        snd_start.Play();

        IGadget g = GameManager.gadgetPrefabs[gadget];

        par_shape.textureSheetAnimation.SetSprite(0, g.iconSprite);
        par_shape_ren.material.color = g.GetValueColour();
        par_shape_back.textureSheetAnimation.SetSprite(0, g.iconBack);
        par_shape_back_ren.material.color = g.GetValueColour();
        par_shape2.textureSheetAnimation.SetSprite(0, g.iconSprite);
        par_shape2_ren.material.color = g.GetValueColour();
        par_shape2_back.textureSheetAnimation.SetSprite(0, g.iconBack);
        par_shape2_back_ren.material.color = g.GetValueColour();

        StartCoroutine(OpenChestAnim());
    }
    IEnumerator OpenChestAnim()
    {
        yield return new WaitForSeconds(1.5f);
        par_start.Play(true);
        yield return new WaitForSeconds(0.42f);
        par_start.Pause(true);
        par_start.gameObject.SetActive(false);
        par_loop.Play(true);
        yield return null;
    }

    [ClientRpc] public virtual void Rpc_CloseChest(bool isLeft)
    {
        Debug.Log("Close chest");

        gadget = -1;
        boxCollider.enabled = false;
        snd_end.Play();

        par_loop.Stop(true);
        par_loop.gameObject.SetActive(false);
        par_start.gameObject.SetActive(true);
        par_start.Play(true);
    }
}

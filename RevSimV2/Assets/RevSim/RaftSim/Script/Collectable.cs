using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

[RequireComponent(typeof(XRSimpleInteractable))]
public class Collectable : MonoBehaviour, IUseable
{
    public int score = 1;

    public void Interact()
    {
        Destroy(this.gameObject);
    }

    public void AddScore()
    {

    }
}

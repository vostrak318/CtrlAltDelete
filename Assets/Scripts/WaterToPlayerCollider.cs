using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterToPlayerCollider : MonoBehaviour
{
    public bool isUnderWater = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            MainMovement player = other.gameObject.GetComponentInParent<MainMovement>();
            player.UpdateDeathUI();
            player.SetRagdollState(true);
            isUnderWater = true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAnimationTrigger : MonoBehaviour
{
    private Zombie zombie;

    private void Start()
    {
        zombie = GetComponent<Zombie>();
    }

}

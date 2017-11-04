using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerReporter : MonoBehaviour {

    public Collider subject;

    bool triggerCollision = false;

    public bool TriggeredCollision
    {
        get
        {
            return triggerCollision;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (subject == null)
            return;

        if (subject == other)
            triggerCollision = true;
    }

}

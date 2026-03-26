using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTimer : MonoBehaviour
{
    public string hiddenTag = "Hidden";
    public KeyCode activationKey = KeyCode.E; 
    public float activeDuration = 5f;

    private List<Collider2D> objectsInTrigger = new List<Collider2D>();

    void Update()
    {
        if (Input.GetKeyDown(activationKey))
        {
            foreach (var col in objectsInTrigger)
            {
                ActivateObject(col);
                StartCoroutine(DeactivateAfterDelay(col, activeDuration));
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(hiddenTag) && !objectsInTrigger.Contains(other))
        {
            objectsInTrigger.Add(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(hiddenTag))
        {
            objectsInTrigger.Remove(other);
            DeactivateObject(other); 
        }
    }

    private void ActivateObject(Collider2D col)
    {
        HiddenObject hiddenObj = col.GetComponent<HiddenObject>();
        HiddenPushable hiddenPush = col.GetComponent<HiddenPushable>();

        if (hiddenObj != null) hiddenObj.SetPhysicsActive(true);
        if (hiddenPush != null) hiddenPush.SetPhysicsActive(true);
    }

    private void DeactivateObject(Collider2D col)
    {
        HiddenObject hiddenObj = col.GetComponent<HiddenObject>();
        HiddenPushable hiddenPush = col.GetComponent<HiddenPushable>();

        if (hiddenObj != null) hiddenObj.SetPhysicsActive(false);
        if (hiddenPush != null) hiddenPush.SetPhysicsActive(false);
    }

    private IEnumerator DeactivateAfterDelay(Collider2D col, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (objectsInTrigger.Contains(col))
        {
            DeactivateObject(col);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DandelionTop : MonoBehaviour
{
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.speed = Random.Range(.8f, 1.2f);
        float size = Random.Range(.8f, 2.5f);
        Vector3 newSize = new Vector3 (size, size, 1);
        transform.localScale = newSize;

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NodeMaker : MonoBehaviour {
    public Node nodeClone;
    Node node;
	GameObject nodeBox;
    public float bottomBuffer = 2;
    public float nodeRedundancy;

    Collider2D thisCollider;
	// Use this for initialization
	void Awake () {
        thisCollider = GetComponent<Collider2D>();
		nodeBox = new GameObject ();
		nodeBox.transform.parent = gameObject.transform;
        CreateNodes();
	}

    void CreateNodes() {
        int numberOfNodes = (int)Mathf.Ceil((gameObject.transform.lossyScale.x + nodeClone.transform.localScale.x) / nodeClone.neighborDistance * nodeRedundancy);
        for (int i = 0; i < numberOfNodes + 1; i++) {
            node = Instantiate(
                nodeClone, 
                new Vector3(
                    (thisCollider.bounds.min.x - nodeClone.transform.localScale.x / 2) + i * (gameObject.transform.lossyScale.x + nodeClone.transform.localScale.x)/numberOfNodes,
                    thisCollider.bounds.max.y + nodeClone.transform.localScale.y / 2, 
                    0
                ), 
                transform.rotation
            );
            node.creator = gameObject;
			node.transform.parent = nodeBox.transform;
            
        }
        numberOfNodes = (int)Mathf.Ceil((gameObject.transform.lossyScale.y + nodeClone.transform.localScale.y/2 - bottomBuffer) / nodeClone.neighborDistance * nodeRedundancy);
        for (int i = 0; i < numberOfNodes; i++) {
            node = Instantiate(
                nodeClone,
                new Vector3(
                    thisCollider.bounds.min.x - nodeClone.transform.localScale.x / 2,
                    thisCollider.bounds.min.y + bottomBuffer + i * (gameObject.transform.lossyScale.y + nodeClone.transform.localScale.y/2) / numberOfNodes,
                    0
                ),
                transform.rotation
            );
            node.creator = gameObject;
			node.transform.parent = nodeBox.transform;
        }
        for (int i = 0; i < numberOfNodes; i++) {
            node = Instantiate(
                nodeClone,
                new Vector3(
                    thisCollider.bounds.max.x + nodeClone.transform.localScale.x / 2,
                    thisCollider.bounds.min.y + bottomBuffer + i * (gameObject.transform.lossyScale.y + nodeClone.transform.localScale.y / 2) / numberOfNodes,
                    0
                ),
                transform.rotation
            );
            node.creator = gameObject;
			node.transform.parent = nodeBox.transform;
        }
    }
}

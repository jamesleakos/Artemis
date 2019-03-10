using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Node : MonoBehaviour {
    public LayerMask collisionMask;

    // some lists
    public List<Node> neighbors;

    // methods for PathManager
    public Node previous {
        get;
        set;
    }
    public float cost {
        get;
        set;
    }

    // settings to find neighbors
    public float neighborDistance;
    public float tooCloseNeighborDistance;
    public GameObject creator;

    // node type
    public enum NodeType { trv_down, trv_right, trv_left, edge_right, edge_left, air};
    public NodeType nodeType;
    public float sensorBuffer = 0.1f;

    void Awake () {
        CheckType();
    }
    void Start () {
        CheckNeighbors();
    }

    public void CheckNeighbors () {
        neighbors.Clear();

        //foreach (GameObject geonode in GameObject.FindGameObjectsWithTag("Node")) {
        foreach (Node geonode in FindObjectsOfType<Node>()) {
            //Node otherNode = geonode.GetComponent<Node>();
            CheckNeighbor(geonode);
        }
    }
    private void CheckNeighbor(Node otherNode) {
        if (nodeType == NodeType.air && otherNode.nodeType == NodeType.air) {
            return;
        }
        Vector3 distance = gameObject.transform.position - otherNode.gameObject.transform.position;
        if (distance.magnitude < tooCloseNeighborDistance)
            return;
        if (distance.y < 0 && distance.magnitude > neighborDistance)
            return;
        if (distance.magnitude < neighborDistance || (distance.y > 0 && distance.y > Mathf.Abs(distance.x))) {
            RaycastHit2D hit = Physics2D.Linecast(gameObject.transform.position, otherNode.gameObject.transform.position, collisionMask);
            if (!hit) {
                neighbors.Add(otherNode);
            }
        }
    }

    public void CheckType() {
        nodeType = NodeType.air;
        RaycastHit2D hit = Physics2D.Raycast(gameObject.transform.position, Vector2.down, gameObject.transform.lossyScale.y / 2 + sensorBuffer, collisionMask);
        if (hit) {
            nodeType = NodeType.trv_down;
            return;
        }
        hit = Physics2D.Raycast(gameObject.transform.position, Vector2.right, gameObject.transform.lossyScale.x / 2 + sensorBuffer, collisionMask);
        if (hit) {
            nodeType = NodeType.trv_right;
            return;
        }
        hit = Physics2D.Raycast(gameObject.transform.position, Vector2.left, gameObject.transform.lossyScale.x / 2 + sensorBuffer, collisionMask);
        if (hit) {
            nodeType = NodeType.trv_left;
            return;
        }
        hit = Physics2D.Raycast(gameObject.transform.position, Vector2.right + Vector2.down, new Vector2(gameObject.transform.lossyScale.x, gameObject.transform.lossyScale.y).magnitude / 2 + sensorBuffer, collisionMask);
        if (hit) {
            nodeType = NodeType.edge_left;
            return;
        }
        hit = Physics2D.Raycast(gameObject.transform.position, Vector2.left + Vector2.down, new Vector2(gameObject.transform.lossyScale.x, gameObject.transform.lossyScale.y).magnitude / 2 + sensorBuffer, collisionMask);
        if (hit) {
            nodeType = NodeType.edge_right;
            return;
        }
    }

    void OnDrawGizmos() {
        if (nodeType == NodeType.edge_left || nodeType == NodeType.edge_right) {
            Gizmos.color = Color.white;
        }else if (nodeType == NodeType.air) {
            Gizmos.color = Color.red;
        } else {
            Gizmos.color = Color.green;
        }
        Gizmos.DrawWireCube(gameObject.transform.position, gameObject.transform.lossyScale);

        //if (neighbors == null)
        //    return;
        //Gizmos.color = new Vector4(0, 1, 0, 0.2f);
        //foreach (var neighbor in neighbors) {
        //    if (neighbor != null)
        //        Gizmos.DrawLine(transform.position, neighbor.transform.position);
        //}
    }
}

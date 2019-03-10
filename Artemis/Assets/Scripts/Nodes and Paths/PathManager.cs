using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    public List<Node> pathList;
	private Vector3 currentNodePosition;
    Node personalNode;
    Node currentNode;
    Node endNode;

    int count;

    public LayerMask collisionMask;

    void Awake() {
        personalNode = gameObject.transform.Find("Node").GetComponent<Node>();
    }

    void Start () {
        personalNode.CheckNeighbors();
    }

    public void NavigateTo(Vector3 destination)
	{
        pathList = new List<Node>();
        UpdatePersonalNode();
		currentNode = FindClosestNode(gameObject.transform.position);
        endNode = FindClosestNode (destination);
		if (currentNode == null || endNode == null || currentNode == endNode)
			return;
        var openList = new SortedList<float, Node>();
        var closedList = new List<Node>();
        openList.Add(0, currentNode);
        currentNode.previous = null;
        currentNode.cost = 0f;
        while (openList.Count > 0) {
            currentNode = openList.Values[0];
            openList.RemoveAt(0);
            var cost = currentNode.cost;
            closedList.Add(currentNode);
            if (currentNode == endNode) {
                break;
            }
            foreach (var neighbor in currentNode.neighbors) {
                if (neighbor) {
                    if (closedList.Contains(neighbor) || openList.ContainsValue(neighbor))
                        continue;
                    neighbor.previous = currentNode;
                    neighbor.cost = cost + (neighbor.transform.position - currentNode.transform.position).magnitude + 0.5f;
                    var distanceToTarget = (neighbor.transform.position - endNode.transform.position).magnitude;
                    float key = neighbor.cost + distanceToTarget;
                    while (openList.ContainsKey(key)) {
                        key = key + 0.001f;
                    }
                    openList.Add(key, neighbor);
                }
            }
        }
        if (currentNode == endNode) {
            while (currentNode.previous != null) {
                pathList.Insert(0, currentNode);
                currentNode = currentNode.previous;
            }
        }
    }

	private Node FindClosestNode(Vector3 target)
	{
		Node closest = null;
		float closestDist = Mathf.Infinity;
		foreach (Node node in FindObjectsOfType<Node>())
		{
			var dist = (node.gameObject.transform.position - target).magnitude;
            if (dist < closestDist) {
                closest = node;
                closestDist = dist;
            }
		}
		if (closest != null)
		{
			return closest;
		}
		return null;
	}

    private void UpdatePersonalNode () {
        personalNode.CheckType();
        personalNode.CheckNeighbors();
    }

    void OnDrawGizmos () {
        if (Application.isPlaying && pathList.Count > 0) {
            Gizmos.color = Color.red;

            for (int i = 0; i < pathList.Count - 1; i++) {
                Gizmos.DrawLine(pathList[i].transform.position, pathList[i + 1].transform.position);
            }
        }
    }

}

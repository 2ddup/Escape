using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PairNode : IEquatable<PairNode>
{
    public BaseChar m_player;
    public BaseChar m_Destination;

    public bool Equals(PairNode other)
    {
        if(other.m_player == null || other.m_Destination == null)
            return false;
        else if(m_player == null || m_Destination == null)
            return false;
        bool state = other.m_player.GetInstanceID() == m_player.GetInstanceID();

        return state;
    }   
}

public class PathFinding : MonoBehaviour
{
    private Grid m_grid;
    private List<Node> m_closedList = new List<Node>();
    private List<Node> m_openList = new List<Node>();
    private NodeComparer m_nodeComparer = new NodeComparer();

    private Node m_currNode;
    private Node m_startNode;
    private Node m_targetNode;
    private Node m_prevNode;
    private List<Node> m_pathNode;
    private List<Node> m_currNeighbours = new List<Node>();
    List<PairNode> m_orderList = new List<PairNode>();

    private bool m_execute = false;
    public Vector3 targetNodePos = new Vector3(6.5f, 6.25f, -13.3f);
    // Start is called before the first frame update
    private void Awake()
    {
        m_grid = GameObject.FindObjectOfType<Grid>();
    }

    public int GetDistance(Node a, Node b)
    {
        int x = Mathf.Abs(a.Col - b.Col);
        int y = Mathf.Abs(a.Row - b.Row);

        return 14 * Mathf.Min(x, y) + 10 * Mathf.Abs(x - y);
    }

    public List<Node> RetracePath(Node currNode)
    {
        List<Node> nodes = new List<Node>();

        while (currNode != null)
        {
            nodes.Add(currNode);
            currNode = currNode.Parent;
        }
        nodes.Reverse();

        return nodes;
    }

    public void ResetNode()
    {
        m_currNode = null;
        m_startNode = null;
        m_targetNode = null;
        m_prevNode = null;
        
        if(m_pathNode != null)
            m_pathNode.Clear() ;
        if(m_currNeighbours != null)
            m_currNeighbours.Clear();
        if(m_openList != null)
            m_openList.Clear();
        if(m_closedList != null)
            m_closedList.Clear();

        m_grid.ResetNode();
    }

    public void Ready(Vector3 player, Vector3 target)
    {
        m_execute = true;

        m_openList.Clear();
        m_closedList.Clear();

        m_startNode = m_grid.FindNode(player);
        m_targetNode = m_grid.FindNode(targetNodePos);

        m_targetNode.SetParent(null);
        m_startNode.SetParent(null);

        m_currNode = m_startNode;
        m_startNode.SetGCost(0);
        m_startNode.SetHCost(GetDistance(m_startNode, m_targetNode));
    }

    public void FindPathCoroutine(BaseChar Player, BaseChar Destination )
    {
        if(!m_execute)
        {
                                                                           //Debug.Log("!!!!!!!!!!!!!");
            Ready(Player.transform.position, targetNodePos);
            StartCoroutine(IEStep(Player));
        }
        else
        {
            PairNode pairNode = new PairNode();
            pairNode.m_player = Player;
            pairNode.m_Destination = Destination;
            if( !m_orderList.Contains( pairNode ) )
            {
                m_orderList.Add(pairNode);
            }
        }
    }

    public IEnumerator IEStep(BaseChar player)
    {
                                                                            //Debug.Log("################");
        Node[] neighbours = m_grid.Neighbours(m_currNode);
        m_currNeighbours.Clear();
        m_currNeighbours.AddRange(neighbours);

        for(int i =0; i < neighbours.Length; ++i)
        {
            if(m_closedList.Contains(neighbours[i]))
                continue;
            
            if(neighbours[i].NType == NodeType.Wall)
                continue;
            int gCost = m_currNode.GCost + GetDistance(neighbours[i], m_currNode);

            if(m_openList.Contains(neighbours[i]) == false || gCost < neighbours[i].GCost)
            {
                int hCost = GetDistance(neighbours[i], m_targetNode);
                neighbours[i].SetGCost(gCost);
                neighbours[i].SetHCost(hCost);
                neighbours[i].SetParent(m_currNode);

                if (!m_openList.Contains(neighbours[i]))
                    m_openList.Add(neighbours[i]);
            }
        }

        m_closedList.Add(m_currNode);

        if(m_openList.Contains(m_currNode))
            m_openList.Remove(m_currNode);
        
        if(m_openList.Count>0)
        {
            m_openList.Sort(m_nodeComparer);
            if(m_currNode != null)
            {
                m_prevNode = m_currNode;
            }
            m_currNode = m_openList[0];
        }

        yield return null;

        if (m_currNode == m_targetNode)
        {
            List<Node> nodes = RetracePath(m_currNode);
            m_pathNode = nodes;

                                                                                        Debug.Log("찾음.!");

            m_execute = false;

            player.SetPath(m_pathNode);
            Debug.Log(m_pathNode.Count);
        }
        else
            StartCoroutine(IEStep(player));
    }
}

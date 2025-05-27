using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum NodeType
{
    None,
    Wall
}

public class Node : MonoBehaviour
{
    public NodeType m_nodeType = NodeType.None;
    private int m_fcost = 0;
    private int m_gcost = 0;
    private int m_hcost = 0;
    private int m_row;
    private int m_col;

    private Node m_parent;
    private BoxCollider m_collider;
    
    public int Col
    {
        get {return m_col;}
    }

    public int Row
    {
        get {return m_row;}
    }

    public Node Parent
    {
        get {return m_parent;}
    }
    public Vector3 Pos
    {
        set 
        {
            transform.position = value;
        }
        get {return transform.position;}
    }
    public NodeType NType
    {
        get { return m_nodeType; }
    }
    public void Reset()
    {
        m_nodeType = NodeType.None;
        m_parent = null;
    }

    public void Awake()
    {
        m_collider = GetComponent<BoxCollider>();
    }
    public bool Contains( Vector3 position )
    {
                                                                    // Debug.Log(m_collider.bounds.ToString());
                                                                    // Debug.Log($"Node Position: {transform.position}, Collider Bounds: {m_collider.bounds}");                                                            
        return m_collider.bounds.Contains(position);
    }
    public void SetParent( Node parent )
    {
        m_parent = parent;
    }
    public void SetNode(int row, int col)
    {
        m_row = row;
        m_col = col;
    }

    public int FCost
    {
        get {return m_gcost + m_hcost;}
    }

    public int GCost
    {
        get {return m_gcost;}
    }

    public int HCost
    {
        get {return m_hcost;}
    }

    public void SetHCost(int cost)
    {
        m_hcost = cost;
    }
    public void SetGCost(int cost)
    {
        m_gcost = cost;
    }

    public void MakeNodeWall()
    {
        Collider[] col = Physics.OverlapBox(m_collider.bounds.center, m_collider.bounds.extents, Quaternion.identity, LayerMask.GetMask("Wall"));
        foreach(Collider c in col)
        {
            if(c != null)
            {
                m_nodeType = NodeType.Wall;
            }
        }
    }

    void OnDrawGizmos()
    {
        Vector3 boxCenter = m_collider.bounds.center;
        Vector3 boxHalfExtents = m_collider.bounds.extents;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCenter, boxHalfExtents * 2); // 실제 크기로 그리기
    }

}

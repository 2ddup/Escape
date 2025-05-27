using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public float offsetX = 11.5f;
    public float offsetY = 6.5f;
    public int m_nodeCount = 21;
    public Node m_nodePrefab;
    public Node[,] m_nodeArr;
    public Transform m_root;
    private List<Node> m_neighbours = new List<Node>();
    
    void Awake()
    {
        m_root = transform.Find("Root");
        m_nodePrefab = Resources.Load<Node>("Node");
    }

    // public Node ClickNode()
    // {
    //     return FindNode(Input.mousePosition);
    // }

    public void CreateGrid(int nodecount)
    {
        m_nodeCount = nodecount;
        int count = 0;
        m_nodeArr = new Node[nodecount, nodecount];
        for(int row = 0 ; row < nodecount ; ++row )
        {
            for(int col = 0 ; col < nodecount ; ++col)
            {
                Node node = Instantiate(m_nodePrefab, Vector3.zero, Quaternion.identity, m_root);
                m_nodeArr[row, col] = node;
                node.name = "Node " + ++count;
                node.SetNode(row, col);
                node.transform.localPosition = new Vector3 (offsetX - col * 0.25f, offsetY - row * 0.25f, -13.3f);
            }
        }
    }

    private bool CheckNode(int row, int col)
    {
        if(row < 0 || row >= m_nodeCount)
            return false;
        if(col < 0 || col >= m_nodeCount)
            return false;
        
        return true;
    }

    public Node[] Neighbours(Vector3 position)
    {
        for(int row = 0; row < m_nodeCount; ++row)
        {
            for(int col = 0; col < m_nodeCount; ++col)
            {
                if(m_nodeArr[row, col].Contains(position))
                {
                    return Neighbours(m_nodeArr[row, col]);
                }
            }
        }
        return null;
    }

    public Node[] Neighbours(Node node)
    {
        m_neighbours.Clear();

        // 좌측 상단
        // if (CheckNode(node.Row - 1, node.Col - 1))
        //     m_neighbours.Add(m_nodeArr[node.Row - 1, node.Col - 1]);
        
        //상단
        if (CheckNode(node.Row - 1, node.Col))
            m_neighbours.Add(m_nodeArr[node.Row - 1, node.Col]);

        // 우측 상단
        // if (CheckNode(node.Row - 1, node.Col + 1))
        //     m_neighbours.Add(m_nodeArr[node.Row - 1, node.Col + 1]);

        // 좌측 
        if (CheckNode(node.Row, node.Col - 1))
            m_neighbours.Add(m_nodeArr[node.Row, node.Col - 1]);


        // 우측
        if (CheckNode(node.Row, node.Col + 1))
            m_neighbours.Add(m_nodeArr[node.Row, node.Col + 1]);


        // 좌측 하단
        // if (CheckNode(node.Row + 1, node.Col - 1))
        //     m_neighbours.Add(m_nodeArr[node.Row + 1, node.Col - 1]);

        // 하단
        if (CheckNode(node.Row + 1, node.Col))
            m_neighbours.Add(m_nodeArr[node.Row + 1, node.Col]);

        // 우측 하단
        // if (CheckNode(node.Row + 1, node.Col + 1))
        //     m_neighbours.Add(m_nodeArr[node.Row + 1, node.Col + 1]);

        return m_neighbours.ToArray();

    }

    public Node FindNode(Vector3 pos)
    {
        for(int row = 0; row < m_nodeCount; ++row)
        {
            for(int col = 0; col < m_nodeCount; ++col)
            {
                if(m_nodeArr[row, col].Contains(pos))
                {
                    return m_nodeArr[row, col];
                }
            }
        }
        return null;
    }
    public void ResetNode()
    {
        for(int row = 0; row < m_nodeCount; ++row )
        {
            for(int col = 0; col < m_nodeCount; ++col)
            {
                m_nodeArr[row, col].Reset();
            }
        }
    }
}
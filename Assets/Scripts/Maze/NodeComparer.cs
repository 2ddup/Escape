using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeComparer : IComparer<Node>
{
    public int Compare(Node x, Node y)
    {
        if (x.FCost < y.FCost)
            return -1;

        else if (x.FCost > y.FCost)
            return 1;

        else if (x.FCost == y.FCost)
        {
            if (x.HCost < y.HCost)
                return -1;
                
            else if (x.HCost > y.HCost)
                return 1;
        }
        
        return 0;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class NewMapGenerator : MonoBehaviour
{
    public class Map
    {
        public enum Direction
        {
            NORTH,
            SOUTH,
            EAST,
            WEST
        }

        public class NodeEntrance
        {
            

            public int x;
            public int y;

            public Direction direction;

            public NodeEntrance(int x_, int y_, Direction d_)
            {
                x = x_;
                y = y_;
                direction = d_;
            }
        }

        public class Node
        {
            public int x;
            public int y;
            public int w;
            public int h;

            public int doorways;


            public List<Vector2Int> edges;

            public Node(int x_, int y_, int w_, int h_)
            {
                x = x_;
                y = y_;
                w = w_;
                h = h_;

                edges = new List<Vector2Int>();

                for (int i = 0; i < w; i++)
                {
                    if (i == 0 || i == w - 1)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            edges.Add(new Vector2Int(i, j));
                        }
                    }
                    else
                    {
                        edges.Add(new Vector2Int(i, 0));
                        edges.Add(new Vector2Int(i, h - 1));
                    }


                }

                return;
            }
        }

        public int w;
        public int h;
        public int[,] cells;
        List<Node> rooms;

        public Vector2Int start;
        public Vector2Int end;
        public int mincellw;
        public int mincellh;
        public int maxcellw;
        public int maxcellh;


        Vector2Int diffendstart;
        Node current;
        Vector2Int startposnextnode;
        int remhor;
        int remver;

        

        public Map(int w_, int h_, int mincellw_, int mincellh_, int maxcellw_, int maxcellh_)
        {
            w = w_;
            h = h_;
            cells = new int[w, h];
            rooms = new List<Node>();

            start = new Vector2Int(0, 0);
            end = new Vector2Int(w-2, h-2);
            mincellw = mincellw_;
            mincellh = mincellh_;
            maxcellw = maxcellw_;
            maxcellh = maxcellh_;

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    cells[i, j] = 0;
                }
            }
        }

        public bool CanPlaceGenuineRoom()
        {
            //Create the node based on minimum and maximum dimensions or the remaining size if about to reach the end cell
            remhor = (w - 1 - startposnextnode.x);
            remver = (h - 1 - startposnextnode.y);
            return ((remhor >= mincellw) || (remver >= mincellh));
        }

        public void CreateCurrentNode()
        {
            if (CanPlaceGenuineRoom())
            {
                //the minimum sized room is NOT too big for the grid - proceed normally 
                current = new Node(startposnextnode.x, startposnextnode.y, Random.Range(mincellw, Mathf.Min(maxcellw, remhor) + 1), Random.Range(mincellh, Mathf.Min(maxcellh, remver) + 1));
            }
            else
            {
                //the minimum sized room IS too big for the grid - construct a smaller room using the remaining cells
                current = new Node(startposnextnode.x, startposnextnode.y, remhor, remver);
            }

            rooms.Add(current);
        }

        public void UpdateCellMatrixForCurrent()
        {
            //mark all cells occupied by the current room node as 1
            for (int i = current.x; i < current.x + current.w; i++)
            {
                for (int j = current.y; j < current.y + current.h; j++)
                {
                    Debug.Log("Setting " + i + " " + j);
                    cells[i, j] = 1;
                }
            }
        }

        public void UpdateStartPosNextNode(int linearity_)
        {
            int chance = Random.Range(1, 101);
            

            //calculate vector subtraction from end to start to determine whether further vertically or horizontally from end room
            diffendstart = end - startposnextnode;

            //calculate the next node
            //determine if distance from end node is greater vertically or horizontally
            if (diffendstart.y > diffendstart.x)
            {
                //startposnextnode = new Vector2Int(Random.Range(current.x, current.x + current.w), current.y + current.h);

                if (chance <= linearity_) startposnextnode = new Vector2Int(Random.Range(current.x, current.x + current.w), current.y + current.h);
                else startposnextnode = new Vector2Int(current.x + current.w, Random.Range(current.y, current.y + current.h));
            }
            else
            {
                //startposnextnode = new Vector2Int(current.x + current.w, Random.Range(current.y, current.y + current.h));

                if (chance > linearity_) startposnextnode = new Vector2Int(Random.Range(current.x, current.x + current.w), current.y + current.h);
                else startposnextnode = new Vector2Int(current.x + current.w, Random.Range(current.y, current.y + current.h));
            }
        }

        public void GeneratePath(int linearity_)
        {
            
            current = null;
            startposnextnode = start;

            while (current == null || ((current.x + current.w) < end.x && (current.y + current.h) < end.y) )
            {
                CreateCurrentNode();
                UpdateCellMatrixForCurrent();
                UpdateStartPosNextNode(linearity_);
            }

            Debug.Log("Number of rooms = " + rooms.Count);
        }
    }


    public int mapwidth;
    public int mapheight;

    [SerializeField] Tilemap thetilemap;
    [SerializeField] TileBase tile1;
    [SerializeField] RuleTile ruletile1;

    public int linearity; //value between 1 and 100 effectively meaning the % chance to follow the most efficient path

    public void DrawTiles(Map map)
    {
        for (int i = 0; i < map.w; i++)
        {
            for (int j = 0; j < map.h; j++)
            {
                if (map.cells[i, j] == 1)
                {
                    thetilemap.SetTile(new Vector3Int(i, j, 0), ruletile1);
                }
            }
        }
    }

    public void Start()
    {
        Map map = new Map(32, 32, 3, 3, 5, 5);

        map.GeneratePath(linearity);

        //modify cells on tilemap with value 1
        DrawTiles(map);
        
    }
    
    
    
    
    

    
}

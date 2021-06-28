﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class NewMapGenerator : MonoBehaviour
{
    float timer;
    public int scaleupmult;
    enum MapGeneratorState
    {
        INIT,
        BUILDMAP,
        FINISH
    }

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

    public class Edge
    {
        public Vector2Int start;
        public Vector2Int end;
        public int thickness;

        public Edge(Vector2Int start_, Vector2Int end_, int thickness_)
        {
            start = start_;
            end = end_;
            thickness = thickness_;
        }
    }

    public class Node
    {
        public int x;
        public int y;
        public int w;
        public int h;

        public List<NodeEntrance> entrances;

        public List<Vector2Int> edges;

        public List<Node> connections;

        public Node(int x_, int y_, int w_, int h_)
        {
            x = x_;
            y = y_;
            w = w_;
            h = h_;

            entrances = new List<NodeEntrance>();

            edges = new List<Vector2Int>();

            connections = new List<Node>();


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


        }
    }

    public class Map
    {
        

        public int w;
        public int h;
        public int offsetx;
        public int offsety;
        public int[,] cells;
        List<Node> rooms;
        Dictionary<GameObject, Node> roomtonodemappings;
        public Node GetCorrespondingNode(GameObject g_)
        {
            Node n_;
            if (roomtonodemappings.TryGetValue(g_, out n_)) return n_;
            else
            {
                Debug.Log("Cannot retrieve node");
                return null;
            }
            //return roomtonodemappings[g_];
        }

        //PATHS
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


        //public Map(int w_, int h_)
        //{
        //    w = w_;
        //    h = h_;
        //    cells = new int[w, h];
        //    rooms = new List<Node>();

        //    start = new Vector2Int(0, 0);
        //    end = new Vector2Int(w - 2, h - 2);

        //    for (int i = 0; i < w; i++)
        //    {
        //        for (int j = 0; j < h; j++)
        //        {
        //            cells[i, j] = 0;
        //        }
        //    }
        //}


        //public Map(int w_, int h_, int mincellw_, int mincellh_, int maxcellw_, int maxcellh_)
        //{
        //    w = w_;
        //    h = h_;
        //    cells = new int[w, h];
        //    rooms = new List<Node>();

        //    start = new Vector2Int(0, 0);
        //    end = new Vector2Int(w-2, h-2);
        //    mincellw = mincellw_;
        //    mincellh = mincellh_;
        //    maxcellw = maxcellw_;
        //    maxcellh = maxcellh_;

        //    for (int i = 0; i < w; i++)
        //    {
        //        for (int j = 0; j < h; j++)
        //        {
        //            cells[i, j] = 0;
        //        }
        //    }
        //}

        public Map(int w_, int h_, int offsetx_, int offsety_, int mincellw_, int mincellh_, int maxcellw_, int maxcellh_)
        {
            Debug.Log("Creating map " + w_ + " x " + h_);
            w = w_;
            h = h_;
            offsetx = offsetx_;
            offsety = offsety_;
            cells = new int[w, h];
            rooms = new List<Node>();
            roomtonodemappings = new Dictionary<GameObject, Node>();

            mincellw = 2;
            mincellh = 2;
            maxcellw = 2;
            maxcellh = 2;

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

            ////mark all cells occupied by the current room node as 1
            //for (int i = current.x; i < current.x + current.w; i++)
            //{
            //    for (int j = current.y; j < current.y + current.h; j++)
            //    {
            //        //Debug.Log("Setting " + i + " " + j);
            //        cells[i, j] = 1;
            //    }
            //}


            /*Method 1 : Do walls and floors separately*/

            int padding = 12;
            //mark all cells occupied by the current room node as 1
            //for (int i = current.x + padding; i < current.x + current.w - padding; i++)
            //{
            //    for (int j = current.y + padding; j < current.y + current.h - padding; j++)
            //    {
            //        //Debug.Log("Setting " + i + " " + j);
            //        cells[i, j] = 1;
            //    }
            //}

            //Walls / borders
            int xxx = current.x + current.w;
            int yyy = current.y + current.h;
            int xx = current.x + padding;
            int xy = current.x + current.w - padding;
            int yx = current.y + current.h - padding;
            int yy = current.y + padding;
            //for (int i = current.x; i < current.x + current.w; i++)
            //{
            //    for (int j = current.y; j < current.y + current.h; j++)
            //    {
            //        //Debug.Log("Setting " + i + " " + j);
            //        if ( (i < xx || (i > xy && i < xxx))  && (j < yy || (j > yx && j < yyy)) ) cells[i, j] = 2;
            //    }
            //}

            /*Method 2 : Do walls and floors in the same loop*/
            for (int i = current.x; i < current.x + current.w; i++)
            {
                for (int j = 0; j < current.y + current.h; j++)
                {
                    if (i >= xx && i < xy && j > yy && j < yx)
                    {
                        //apply a floor tile
                        cells[i, j] = 1;
                    }
                    else if (i >= xx-2 && i < xy+2 && j > yy-2 && j < yx+2)
                    {
                        //apply a wall tile
                        cells[i, j] = 2;
                    }
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

                if (chance <= linearity_)
                {
                    startposnextnode = new Vector2Int(Random.Range(current.x, current.x + current.w), current.y + current.h);
                    //current.entrances.Add(new NodeEntrance(startposnextnode.x, startposnextnode.y - 1, Direction.NORTH));
                }
                else
                {
                    startposnextnode = new Vector2Int(current.x + current.w, Random.Range(current.y, current.y + current.h));
                }
            }
            else
            {
                //startposnextnode = new Vector2Int(current.x + current.w, Random.Range(current.y, current.y + current.h));

                if (chance > linearity_)
                {
                    startposnextnode = new Vector2Int(Random.Range(current.x, current.x + current.w), current.y + current.h);
                }
                else
                {
                    startposnextnode = new Vector2Int(current.x + current.w, Random.Range(current.y, current.y + current.h));
                }
            }
        }

        public void GeneratePath(Node room1, Node room2, int linearity_)
        {
            current = null;
            startposnextnode = new Vector2Int((room1.x + room1.x + room1.w)/2, (room1.y + room1.y + room1.h) / 2);
            end = new Vector2Int((room2.x + room2.x + room2.w) / 2, (room2.y + room2.y + room2.h) / 2);

            if (end.x >= start.x && end.y >= start.y)
            {
                while (current == null || ((current.x + current.w) < end.x && (current.y + current.h) < end.y))
                {
                    CreateCurrentNode();
                    UpdateCellMatrixForCurrent();
                    UpdateStartPosNextNode(linearity_);
                }
            }
        }

        public void UpdateCellMatrixWithEdge(Edge e)
        {
            //y = mx + b
            int m = 0;
            int b = 0;

            if ((e.end.x - e.start.x) == 0) return; //incase denominator = 0;
            else Debug.Log("Drawing edge " + e.start + " to " + e.end);
            m = (e.end.y - e.start.y) / (e.end.x - e.start.x);
            b = e.start.y - m * e.start.x;

            int x_, y_;

            for (int i = e.start.x; i <= e.end.x; i++ )
            {
                x_ = i;
                y_ = m * x_ + b;
                Debug.Log("Calc y_: " + m + "*" + x_ + "+" + b);
                Debug.Log(i + ": Line for " + e.start + " to " + e.end + " : " + x_ + ", " + y_);
                for (int t = x_; t < x_ + e.thickness; t++)
                {
                    for (int u = y_; u < y_ + e.thickness; u++)
                    {
                        cells[t, u] = 1;
                    }
                }
            }
        }

        public void GenerateCells(List<GameObject> rooms_)
        {
            current = null;
            int x_, y_, w_, h_;
            //foreach (GameObject g_ in rooms_)
            //{

            //    //confirm variables for current room node
            //    x_ = (int)(g_.transform.position.x) + offsetx + 1;
            //    y_ = (int)(g_.transform.position.y) + offsety + 1;
            //    w_ = (int)(g_.transform.localScale.x) * 8;
            //    h_ = (int)(g_.transform.localScale.y) * 8;

            //    x_ *= 12;
            //    y_ *= 12;



            //    //create the current room node object
            //    current = new Node(x_, y_, w_, h_);
            //    rooms.Add(current);

            //    roomtonodemappings[g_] = current;
            //    Debug.Log("Creating " + w_ + " " + h_ + " at " + x_ + " " + y_ + " for " + g_.GetComponent<Transform>().position + " mapping: " + roomtonodemappings[g_]);
            //    UpdateCellMatrixForCurrent();
            //}

            foreach (GameObject g_ in rooms_)
            {

                //confirm variables for current room node
                x_ = (int)(g_.transform.position.x) + offsetx /* + 1*/;
                y_ = (int)(g_.transform.position.y) + offsety /*+ 1*/;

                //Vector2 boxsize = g_.GetComponent<BoxCollider2D>().size;

                w_ = /*(int)(boxsize.x) * 10 ; */ (int)(g_.transform.localScale.x) * 20;
                h_ = /*(int)(boxsize.y) * 10;  */ (int)(g_.transform.localScale.y) * 20;

                x_ *= 20;
                y_ *= 20;



                //create the current room node object
                current = new Node(x_, y_, w_, h_);
                rooms.Add(current);

                roomtonodemappings[g_] = current;
                Debug.Log("Creating " + w_ + " " + h_ + " at " + x_ + " " + y_ + " for " + g_.GetComponent<Transform>().position + " mapping: " + roomtonodemappings[g_]);
                UpdateCellMatrixForCurrent();
            }


        }
    }

    MapGeneratorState m_State;
    Map m_Map;

    public int mapwidth;
    public int mapheight;

    [SerializeField] Tilemap thetilemap;
    [SerializeField] TileBase tile1;
    [SerializeField] RuleTile ruletile1;
    [SerializeField] RuleTile ruletile2;

    [SerializeField] public GameObject box;

    public int linearity; //value between 1 and 100 effectively meaning the % chance to follow the most efficient path

    public void DrawTiles(Map map)
    {
        for (int i = 0; i < map.w; i++)
        {
            for (int j = 0; j < map.h; j++)
            {
                if (map.cells[i, j] == 1)
                {
                    //Debug.Log("Setting " + i.ToString() + " " + j.ToString());
                    thetilemap.SetTile(new Vector3Int(i, j, 0), ruletile1);
                    
                }
                else if (map.cells[i, j] == 2)
                {
                    thetilemap.SetTile(new Vector3Int(i, j, 0), ruletile2);
                }
                else if (map.cells[i, j] == 0)
                {

                }
            }
        }
    }

    public void CreatePaths()
    {
        Debug.Log("Ok so number of boxes: " + boxes.Count);
        foreach (GameObject box in boxes)
        {
            RoomSimulator boxsim = box.GetComponent<RoomSimulator>();
            Debug.Log(boxsim);
            foreach (RoomSimulator r_ in boxsim.neighbours)
            {
                
                if (!boxsim.connections.Contains(r_) && !r_.connections.Contains(boxsim))
                {
                    Debug.Log("Neighbour " + r_);
                    ////m_Map.GeneratePath()
                    ////Vector2Int pathstart = new Vector2Int((room1.x + room1.x + room1.w) / 2, (room1.y + room1.y + room1.h) / 2);
                    ////Vector2Int pathend = new Vector2Int((room2.x + room2.x + room2.w) / 2, (room2.y + room2.y + room2.h) / 2);
                    ////Debug.Log("G.O. : " + r_.gameObject);
                    Node node1 = m_Map.GetCorrespondingNode(box);
                    Node node2 = m_Map.GetCorrespondingNode(r_.gameObject); //why does .GetComponent<GameObject>() not work???

                    Edge e_ = new Edge(new Vector2Int(node1.x+node1.w/2, node1.y+node1.h/2), new Vector2Int(node2.x+node2.w/2, node2.y+node2.h/2), 2);

                    Debug.Log("Created an edge from " + node1.x + " " + node1.y + " to " + node2.x + " " + node2.y);

                    m_Map.UpdateCellMatrixWithEdge(e_);

                    ////m_Map.GeneratePath(node1, node2, 50);

                    node1.connections.Add(node2);
                    node2.connections.Add(node1);
                }
            }

            //DrawTiles(m_Map);
        }
        

        //foreach (RoomSimulator room in )
        //modify cells on tilemap with value 1
        //DrawTiles(m_Map);
    }

    public int mincellw;
    public int mincellh;
    public int maxcellw;
    public int maxcellh;
    public int mincelldimsdiff;
    public int maxcelldimsdiff;
    public int numrooms;

    public List<GameObject> boxes;

    //private variables
    int miny, maxy, minx, maxx;

    public void CreateRoomSimulators()
    {
        boxes = new List<GameObject>();
        

        int x;
        int y;
        int w;
        int h;

        for (int i = 0; i < numrooms; i++)
        {
            //instantiate the box and update its properties appropriately
            GameObject g_ = Instantiate(box);
            boxes.Add(g_);
            x = Random.Range(0, mapwidth/4);
            y = Random.Range(0, mapheight/4);
            w = Random.Range(mincellw, maxcellw);
            h = Random.Range(Mathf.Max(mincellh, w + mincelldimsdiff), Mathf.Min(maxcellh, w + maxcelldimsdiff));

            g_.GetComponent<Transform>().SetPositionAndRotation(new Vector3(x, y, 0), new Quaternion());
            //box.GetComponent<BoxCollider2D>().size = new Vector2(box.GetComponent<Transform>().localScale.x + 0.3f, box.GetComponent<Transform>().localScale.y + 0.3f);

            g_.GetComponent<Transform>().localScale = new Vector3(w, h, 0);
            //g_.GetComponent<TextMesh>().text = i.ToString();
            //box.GetComponent<Rigidbody2D>().AddForce(new Vector2( Random.Range(0, 100) , Random.Range(0, 100) ) );

            
        }

        
    }

    public void UpdateMinMaxXY()
    {
        int x;
        int y;

        //collect min and max x and y values
        minx = int.MaxValue;
        miny = int.MaxValue;
        maxx = int.MinValue;
        maxy = int.MinValue;

        foreach (GameObject g_ in boxes)
        {
            x = (int)g_.transform.position.x ;
            y = (int)g_.transform.position.y ;


            if (x > maxx) maxx = x;
            else if (x < minx) minx = x;
            if (y > maxy) maxy = y;
            else if (y < miny) miny = y;
        }

        Debug.Log("Finished creating " + boxes.Count + " room simulators minmaxxy " + minx + " " + miny + " " + maxx + " " + maxy);


    }

    public bool AreAllBoxesReady()
    {
        if (timer > 3f) return true;
        else return false;

        /*
         * Should return true if boxes' velocity slows to a certain level
         * For some reason it's not correctly so I am hacking it with the timer variable
         */
        //if (boxes.Count == numrooms)
        //{
        //    int cnt = 0;
        //    Debug.Log("Enough rooms built");
        //    foreach (GameObject r_ in boxes)
        //    {
        //        if (r_.GetComponent<RoomSimulator>() == null) Debug.Log("No roomsim...");

        //        if (!r_.GetComponent<RoomSimulator>().triggering)
        //        {
        //            Debug.Log(r_ + r_.GetComponent<TextMesh>().text + "has " + r_.GetComponent<RoomSimulator>().triggering.ToString() + " Only " + cnt + " rooms triggering");
        //            return false;
        //        }
        //        else if (r_.GetComponent<RoomSimulator>().triggering) cnt++;
        //    }
        //}
        //else
        //{
        //    Debug.Log("Not all boxes built yet - only " + boxes.Count.ToString() +  " where should be " + numrooms.ToString());
        //    return false;
        //}

        //Debug.Log("Ready to fill cells");
        //return true;
    }

    public void CreateCellMatrix()
    {
        int h_ = maxy - miny;
        int w_ = maxx - minx;

        m_Map = new Map(w_ * scaleupmult, h_ * scaleupmult, 0-minx, 0-miny, mincellw, mincellh, maxcellw, maxcellh);

        Debug.Log("Map created: " + m_Map.w + " " +  m_Map.h);

        Debug.Log("Number of boxes  " + boxes.Count);

        m_Map.GenerateCells(boxes);
    }

    public void Start()
    {
        timer = 0.0f;
        scaleupmult = 40;
        m_State = MapGeneratorState.INIT;

        boxes = new List<GameObject>();
        

    }

    private void Update()
    {
        if (timer < 3f) timer += Time.deltaTime;

        switch (m_State)
        {
            case MapGeneratorState.INIT:
            {
                CreateRoomSimulators();
                m_State = MapGeneratorState.BUILDMAP;
                break;
            }
            case MapGeneratorState.BUILDMAP:
            {
                if (AreAllBoxesReady())
                {
                    //Create the rooms
                    Debug.Log("Proceeding");
                    UpdateMinMaxXY();
                    CreateCellMatrix();

                    Debug.Log("Finished creating the rooms mate");
                    //Create paths between rooms
                    CreatePaths();

                    //Actually draw the tiles
                    DrawTiles(m_Map);
                    

                    //Update MapGenerator State
                    m_State = MapGeneratorState.FINISH;
                }
                break;
            } 
            case MapGeneratorState.FINISH:
            {
                break;
            }
        }


        
    }







}

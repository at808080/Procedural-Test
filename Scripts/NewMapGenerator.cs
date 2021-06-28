using System.Collections;
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

        public List<Node> connections;

        public Node(int x_, int y_, int w_, int h_)
        {
            x = x_;
            y = y_;
            w = w_;
            h = h_;

            connections = new List<Node>();
        }
    }

    public class Map
    {
        int scalerooms;
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
        }

        Node current;

        public Map(int w_, int h_, int offsetx_, int offsety_, int scalerooms_)
        {
            //Debug.Log("Creating map " + w_ + " x " + h_);
            w = w_;
            h = h_;
            offsetx = offsetx_;
            offsety = offsety_;
            scalerooms = scalerooms_;
            cells = new int[w, h];
            rooms = new List<Node>();
            roomtonodemappings = new Dictionary<GameObject, Node>();

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    cells[i, j] = 0;
                }
            }
        }

        public void UpdateCellMatrixForCurrent()
        {
            int padding = scalerooms / 2;
            int wallthick = scalerooms / 3;

            //Walls / borders
            //int xxx = current.x + current.w;
            //int yyy = current.y + current.h;
            //int xx = current.x + padding;
            //int xy = current.x + current.w - padding;
            //int yx = current.y + current.h - padding;
            //int yy = current.y + padding;

            /* Do walls and floors in the same loop*/
            //for (int i = current.x; i < current.x + current.w; i++)
            //{
            //    for (int j = 0; j < current.y + current.h; j++)
            //    {
            //        if (i >= xx && i < xy && j > yy && j < yx)
            //        {
            //            if (i < 0 || i > this.w || j < 0 || j > this.h) Debug.Log("OUTSIDE ARRAY BOUNDS");
            //            //apply a floor tile
            //            else cells[i, j] = 1;
            //        }
            //        else if (i >= xx-wallthick && i < xy+ wallthick && j > yy- wallthick && j < yx+ wallthick)
            //        {
            //            if (i < 0 || i > this.w || j < 0 || j > this.h) Debug.Log("OUTSIDE ARRAY BOUNDS");
            //            //apply a wall tile
            //            else cells[i, j] = 2;
            //        }
            //    }
            //}

            int xx = current.x - current.w / 2;
            int xxx = current.x + current.w / 2;
            int xy = xx + padding;
            int xxy = xxx - padding;
            int yy = current.y - current.h / 2;
            int yyy = current.y + current.h / 2;
            int yx = yy + padding;
            int yyx = yyy - padding;

            for (int i = xx; i < xxx; i++)
            {
                for (int j = yy ; j < yyy; j++)
                {
                    if (i >= xy && i < xxy && j > yx && j < yyx)
                    {
                        if (i < 0 || i >= this.w || j < 0 || j >= this.h) Debug.Log("OUTSIDE ARRAY BOUNDS");
                        //apply a floor tile
                        else cells[i, j] = 1;
                    }
                    else if (i >= xx - wallthick && i < xxx + wallthick && j >= yy - wallthick && j < yyy + wallthick)
                    {
                        if (i < 0 || i >= this.w || j < 0 || j >= this.h) Debug.Log("OUTSIDE ARRAY BOUNDS");
                        //apply a wall tile
                        else cells[i, j] = 2;
                    }
                }
            }
        }

        public void UpdateCellMatrixWithEdge(Edge e)
        {
            void DrawPartOfPath(int x_, int y_)
            {
                for (int t = x_; t < x_ + e.thickness; t++)
                {
                    for (int u = y_; u < y_ + e.thickness; u++)
                    {

                        if (t < 0 || t >= this.w || u < 0 || u >= this.h) Debug.Log("OUTSIDE ARRAY BOUNDS");
                        //apply a wall tile
                        else cells[t, u] = 1;
                    }
                }
            }
            //y = mx + b
            int m = 0;
            int b = 0;

            if ((e.end.x - e.start.x) == 0) //return; //incase denominator = 0;
            {
                for (int j = e.start.y; j <= e.end.y; j++)
                {
                    DrawPartOfPath(e.start.x, j);
                }
            }
            else
            {
                //Debug.Log("Drawing edge " + e.start + " to " + e.end);
                m = (e.end.y - e.start.y) / (e.end.x - e.start.x);
                b = e.start.y - m * e.start.x;

                int x__, y__;

                if (m <= 1 && m >= -1)
                {
                    Debug.Log("Lateral path m = " + m + " from " + e.start + " to " + e.end);
                    for (int i = e.start.x; i <= e.end.x; i++)
                    {
                        x__ = i;
                        y__ = m * x__ + b;
                        //Debug.Log("Calc y_: " + m + "*" + x_ + "+" + b);
                        //Debug.Log(i + ": Line for " + e.start + " to " + e.end + " : " + x_ + ", " + y_);
                        DrawPartOfPath(x__, y__);
                    }
                }
                else
                {
                    Debug.Log("Tall path m = " + m + " from " + e.start + " to " + e.end);
                    for (int j = e.start.y; j <= e.end.y; j++)
                    {
                        
                        y__ = j;
                        x__ = (int)((y__ - b) / (float)m);
                        Debug.Log(x__ + " " + y__);
                        DrawPartOfPath(x__, y__);
                    }
                    Debug.Log("End tall path");
                }
                
            }
            
        }

        public void GenerateRooms(List<GameObject> rooms_)
        {
            current = null;
            int x_, y_, w_, h_;

            foreach (GameObject g_ in rooms_)
            {

                //confirm variables for current room node
                x_ = (int)((g_.transform.position.x + offsetx)*scalerooms) /* + 1*/;
                y_ = (int)((g_.transform.position.y + offsety)*scalerooms) /*+ 1*/;

                //Vector2 boxsize = g_.GetComponent<BoxCollider2D>().size;

                w_ = /*(int)(boxsize.x) * 10 ; */ (int)((g_.transform.localScale.x) * scalerooms);
                h_ = /*(int)(boxsize.y) * 10;  */ (int)((g_.transform.localScale.y) * scalerooms);

                //create the current room node object
                current = new Node(x_, y_, w_, h_);
                rooms.Add(current);

                roomtonodemappings[g_] = current;
                //Debug.Log("Creating " + w_ + " " + h_ + " at " + x_ + " " + y_ + " for " + g_.GetComponent<Transform>().position + " mapping: " + roomtonodemappings[g_]);
                UpdateCellMatrixForCurrent();
            }


        }
    }

    MapGeneratorState m_State;
    Map m_Map;

    //public int mapwidth;
    //public int mapheight;

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
        foreach (GameObject box in boxes)
        {
            RoomSimulator boxsim = box.GetComponent<RoomSimulator>();
            foreach (RoomSimulator r_ in boxsim.neighbours)
            {
                if (!boxsim.connections.Contains(r_) && !r_.connections.Contains(boxsim))
                {
                    Node node1 = m_Map.GetCorrespondingNode(box);
                    Node node2 = m_Map.GetCorrespondingNode(r_.gameObject); //why does .GetComponent<GameObject>() not work???

                    //Vector2Int pathstart = new Vector2Int(node1.x + node1.w/2, node1.y + node1.h/2);
                    //Vector2Int pathend = new Vector2Int(node2.x + node2.w/2, node2.y + node2.h/2);

                    Vector2Int pathstart = new Vector2Int(node1.x, node1.y);
                    Vector2Int pathend = new Vector2Int(node2.x, node2.y);

                    Edge e_ = new Edge(pathstart, pathend, 3);

                    Debug.Log("Created an edge from " + node1.x + " " + node1.y + " to " + node2.x + " " + node2.y);

                    m_Map.UpdateCellMatrixWithEdge(e_);

                    node1.connections.Add(node2);
                    node2.connections.Add(node1);
                }
            }
        }
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
            x =  Random.Range(0, maxcellw);
            y =  Random.Range(0, maxcellh);
            w = Random.Range(mincellw, maxcellw);
            h = Random.Range(Mathf.Min(mincellh, w + mincelldimsdiff), Mathf.Max(maxcellh, w + maxcelldimsdiff));

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
            if (x < minx) minx = x;
            if (y > maxy) maxy = y;
            if (y < miny) miny = y;
        }

        miny -= this.maxcellh;
        minx -= this.maxcellw;
        maxy += this.maxcellh;
        maxx += this.maxcellw;

    }

    public bool AreAllBoxesReady()
    {
        if (timer < 2f) timer += Time.deltaTime;
        if (timer > 2f) return true;
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
        int mapheight = 2 * scaleupmult * (maxy - miny + this.maxcellh);
        int mapwidth = 2 * scaleupmult * (maxx - minx + this.maxcellw);

        m_Map = new Map(mapwidth, mapheight, 0-minx, 0-miny, scaleupmult/2);

        Debug.Log("Map created: " + m_Map.w + " " +  m_Map.h);

        Debug.Log("Number of boxes  " + boxes.Count);

        m_Map.GenerateRooms(boxes);
    }

    public void Start()
    {
        timer = 0.0f;
        m_State = MapGeneratorState.INIT;

        boxes = new List<GameObject>();
        

    }

    private void Update()
    {
        

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

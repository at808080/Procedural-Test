using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class NewMapGenerator : MonoBehaviour
{
    float timer;
    enum MapGeneratorState
    {
        INIT,
        BUILDMAP,
        FINISH
    }

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

            public List<NodeEntrance> entrances;

            public List<Vector2Int> edges;

            public Node(int x_, int y_, int w_, int h_)
            {
                x = x_;
                y = y_;
                w = w_;
                h = h_;

                entrances = new List<NodeEntrance>();

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
        public int offsetx;
        public int offsety;
        public int[,] cells;
        List<Node> rooms;

        //public Vector2Int start;
        //public Vector2Int end;
        //public int mincellw;
        //public int mincellh;
        //public int maxcellw;
        //public int maxcellh;


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

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    cells[i, j] = 0;
                }
            }
        }

        //public bool CanPlaceGenuineRoom()
        //{
        //    //Create the node based on minimum and maximum dimensions or the remaining size if about to reach the end cell
        //    remhor = (w - 1 - startposnextnode.x);
        //    remver = (h - 1 - startposnextnode.y);
        //    return ((remhor >= mincellw) || (remver >= mincellh));
        //}

        //public void CreateCurrentNode()
        //{
        //    if (CanPlaceGenuineRoom())
        //    {
        //        //the minimum sized room is NOT too big for the grid - proceed normally 
        //        current = new Node(startposnextnode.x, startposnextnode.y, Random.Range(mincellw, Mathf.Min(maxcellw, remhor) + 1), Random.Range(mincellh, Mathf.Min(maxcellh, remver) + 1));
        //    }
        //    else
        //    {
        //        //the minimum sized room IS too big for the grid - construct a smaller room using the remaining cells
        //        current = new Node(startposnextnode.x, startposnextnode.y, remhor, remver);
        //    }

        //    rooms.Add(current);
        //}

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

        //public void UpdateStartPosNextNode(int linearity_)
        //{
        //    int chance = Random.Range(1, 101);
            

        //    //calculate vector subtraction from end to start to determine whether further vertically or horizontally from end room
        //    diffendstart = end - startposnextnode;

        //    //calculate the next node
        //    //determine if distance from end node is greater vertically or horizontally
        //    if (diffendstart.y > diffendstart.x)
        //    {
        //        //startposnextnode = new Vector2Int(Random.Range(current.x, current.x + current.w), current.y + current.h);

        //        if (chance <= linearity_)
        //        {
        //            startposnextnode = new Vector2Int(Random.Range(current.x, current.x + current.w), current.y + current.h);
        //            //current.entrances.Add(new NodeEntrance(startposnextnode.x, startposnextnode.y - 1, Direction.NORTH));
        //        }
        //        else
        //        {
        //            startposnextnode = new Vector2Int(current.x + current.w, Random.Range(current.y, current.y + current.h));
        //        }
        //    }
        //    else
        //    {
        //        //startposnextnode = new Vector2Int(current.x + current.w, Random.Range(current.y, current.y + current.h));

        //        if (chance > linearity_)
        //        {
        //            startposnextnode = new Vector2Int(Random.Range(current.x, current.x + current.w), current.y + current.h);
        //        }
        //        else
        //        {
        //            startposnextnode = new Vector2Int(current.x + current.w, Random.Range(current.y, current.y + current.h));
        //        }
        //    }
        //}

        //public void GeneratePath(int linearity_)
        //{
            
        //    current = null;
        //    startposnextnode = start;

        //    while (current == null || ((current.x + current.w) < end.x && (current.y + current.h) < end.y) )
        //    {
        //        CreateCurrentNode();
        //        UpdateCellMatrixForCurrent();
        //        UpdateStartPosNextNode(linearity_);
        //    }

        //    Debug.Log("Number of rooms = " + rooms.Count);
        //}

        public void GenerateCells(List<GameObject> rooms_)
        {
            
            current = null;
            int x_, y_, w_, h_;
            foreach (GameObject g_ in rooms_)
            {
                
                //confirm variables for current room node
                x_ = (int)(g_.transform.position.x) + offsetx + 1;
                y_ = (int)(g_.transform.position.y) + offsety + 1;
                w_ = (int)(g_.transform.localScale.x);
                h_ = (int)(g_.transform.localScale.y);

                Debug.Log("Creating " + w_ + " " + h_ + " at " + x_ + " " + y_ + " for " + g_.GetComponent<Transform>().position);

                //create the current room node object
                current = new Node(x_, y_, w_, h_);
                rooms.Add(current);
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
                    Debug.Log("Setting " + i.ToString() + " " + j.ToString());
                    thetilemap.SetTile(new Vector3Int(i, j, 0), ruletile1);
                }
            }
        }
    }

    //public void CreatePath()
    //{
    //    Map map = new Map(32, 32, 3, 3, 5, 5);
    //    map.GeneratePath(linearity);
    //    //modify cells on tilemap with value 1
    //    DrawTiles(map);
    //}

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
            x = Random.Range(0, mapwidth);
            y = Random.Range(0, mapheight);
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
            x = (int)g_.transform.position.x;
            y = (int)g_.transform.position.x;


            if (x > maxx) maxx = x;
            else if (x < minx) minx = x;
            if (y > maxy) maxy = y;
            else if (y < miny) miny = y;
        }

        Debug.Log("Finished creating " + boxes.Count + " room simulators minmaxxy " + minx + " " + miny + " " + maxx + " " + maxy);


    }

    public bool AreAllBoxesReady()
    {
        if (timer > 5f) return true;
        else return false;

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

        m_Map = new Map(w_ * 2, h_ * 2, 0-minx, 0-miny, mincellw, mincellh, maxcellw, maxcellh);

        Debug.Log("Map created: " + m_Map.w + " " +  m_Map.h);

        Debug.Log("Number of boxes  " + boxes.Count);

        m_Map.GenerateCells(boxes);
    }

    public void Start()
    {
        timer = 0.0f;
        m_State = MapGeneratorState.INIT;
        boxes = new List<GameObject>();
        
    }

    private void Update()
    {
        timer += Time.deltaTime;

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
                    Debug.Log("Proceeding");
                    UpdateMinMaxXY();
                    CreateCellMatrix();
                    DrawTiles(m_Map);
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

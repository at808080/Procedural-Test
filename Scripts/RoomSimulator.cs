using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSimulator : MonoBehaviour
{
    enum RoomSimulatorState
    {
        INIT,
        BUILDING,
        FINISH
    }
    public Vector2 v_;
    public Vector2 vel;
    public BoxCollider2D collider;
    public BoxCollider2D trigger;

    public List<RoomSimulator> neighbours;
    public List<RoomSimulator> connections;

    public bool triggering;
    RoomSimulatorState m_State;
    // Start is called before the first frame update
    void Awake()
    {
        m_State = RoomSimulatorState.INIT;
        v_ = GetComponent<Transform>().position;

        neighbours = new List<RoomSimulator>();
        connections = new List<RoomSimulator>();

        BoxCollider2D[] cols = gameObject.GetComponents<BoxCollider2D>();
        foreach (BoxCollider2D b_ in cols)
        {
            if (b_.isTrigger) trigger = b_;
            else collider = b_;
        }
        //trigger.GetComponent<GameObject>().SetActive(false);
        trigger.enabled = false;
        triggering = false;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("Coll");
        if (collision.tag == "RoomSimulator" && collision.GetComponent<RoomSimulator>().triggering && !neighbours.Contains(collision.GetComponent<RoomSimulator>()))
        {
            //Debug.Log("CollSim");
            neighbours.Add(collision.GetComponent<RoomSimulator>());
        }
    }

    // Update is called once per frame
    void Update()
    {

        switch (m_State)
        {
            case RoomSimulatorState.INIT:
                {
                    m_State = RoomSimulatorState.BUILDING;
                    break;
                }
            case RoomSimulatorState.BUILDING:
                {

                    Vector2 curpos = GetComponent<Transform>().position;
                    vel = curpos - v_;
                    v_ = curpos;
                    if (!triggering && vel.magnitude < 0.001)
                    {
                        //Debug.Log(GetComponent<TextMesh>().text + " enabled");
                        trigger.enabled = true;
                        triggering = true;
                        m_State = RoomSimulatorState.FINISH;
                    }
                    break;
                }
            case RoomSimulatorState.FINISH:
                {
                    break;
                }
        }

        //Debug.Log("My trigger is " + triggering);
        

    }
}

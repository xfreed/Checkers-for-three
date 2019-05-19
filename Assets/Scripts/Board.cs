using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    #region Global components

    //Audio
    //public new AudioSource audio;

    // Game Objects
    public GameObject BluePiecePrefab, RedPiecePrefab, GreenPiecePrefab;

    // Standart player names
    public static string[] PlayersNames = { "Blue", "Green", "Red" };

    // Text to see whos turn is it
    public Text Turn_Text, Turn_Text_second;

    // Turn number and count of pieces
    public sbyte Turn, Blueches, RedChes, GreenChes;

    // Last touched object
    private GameObject LstTouchedObject = null;


    //Bot mid
    private BotMind BM0;

    private BotMind BM1;

    private BotMind BM2;

    #endregion Global components

    // Use this for initialization
    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;  // Never sleep
        GenerateBoard();                                // Create board for game and place there checkers
        BM0 = new BotMind(PlayersNames[0]);
        BM1 = new BotMind(PlayersNames[1]);
        BM2 = new BotMind(PlayersNames[2]);
    }

    // Update is called once per frame
    private void Update()
    {
        try
        {
            if (BM0.BotName == PlayersNames[Turn - 1])
            {
                BM0.WhereWeGo();
                NextTurn();
            }
            else if (BM1.BotName == PlayersNames[Turn - 1])
            {
                BM1.WhereWeGo();
                NextTurn();
            }
            else
            {
                BM2.WhereWeGo();
                NextTurn();
            }
            if (Input.GetKey(KeyCode.Escape)) // If player touch escape on phone
            {
                // Create winodws with word " Are you sure want exit from game? "

                //UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == touchPhase) // If we touch something
            {
                touchPosWorld = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position); // Get position were we touch
                Vector2 touchPosWorld2D = new Vector2(touchPosWorld.x, touchPosWorld.y); // Set touched position to Vector2
                RaycastHit2D hitInformation = Physics2D.Raycast(touchPosWorld2D, Camera.main.transform.forward); // Get information what we touch
                if (hitInformation.collider != null)  // If object have colider (all visible must have)
                {
                    GameObject touchedObject = hitInformation.transform.gameObject;
                    if (EndGame()) // Someone win?
                    {
                    }
                    else
                    {
                        MovePiece(touchedObject);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("<color=red>Error: </color>" + e.Message);
        }
    }

    // Create board and place on it pieces
    public void GenerateBoard()
    {
        Blueches = RedChes = GreenChes = 8;
        // Random first Turn
        Turn = System.Convert.ToSByte(UnityEngine.Random.Range(1, 4));
        switch (Turn)
        {
            case 1:
                Turn_Text.text = PlayersNames[0] + " turn!"; break;
            case 2:
                Turn_Text.text = PlayersNames[1] + " turn!"; break;
            case 3:
                Turn_Text.text = PlayersNames[2] + " turn!"; break;
            default:
                Turn_Text.text = "Error!"; break;
        }
        Turn_Text_second.text = Turn_Text.text;
        // Set pieces position
        sbyte[,] Board = new sbyte[,]
        {
              {0,2,2,2,-1,-1,-1 },
              {1,0,2,2,2,-1,-1  },
              {1,1,0,0,2,2,-1   },
              {1,1,0,0,0,0,0    },
              {1,1,0,0,3,3,-1   },
              {1,0,3,3,3,-1,-1  },
              {0,3,3,3,-1,-1,-1 }
       };
        for (sbyte x = 0; x < 7; ++x)
        {
            for (sbyte y = 0; y < 7; ++y)
            {
                switch (Board[x, y])
                {
                    case 1: GeneratePiece(x, y, BluePiecePrefab, PlayersNames[0] + x.ToString() + y.ToString()); break;
                    case 2: GeneratePiece(x, y, GreenPiecePrefab, PlayersNames[1] + x.ToString() + y.ToString()); break;
                    case 3: GeneratePiece(x, y, RedPiecePrefab, PlayersNames[2] + x.ToString() + y.ToString()); break;
                }
            }
        }
    }

    // Create pieces
    private void GeneratePiece(sbyte x, sbyte y, GameObject PerfabPiece, string name)
    {
        GameObject coordianate = GameObject.Find(x.ToString() + y.ToString());
        GameObject go = Instantiate(PerfabPiece) as GameObject;
        go.transform.name = go.transform.name + name;
        go.tag = "piece";
        go.transform.SetParent(transform);
        go.transform.position = new Vector3(coordianate.transform.position.x, coordianate.transform.position.y, coordianate.transform.position.z - 0.5f);
    }

    // Convert String to Vector3
    public static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

    // Check if someone win
    private bool EndGame()
    {
        if (RedChes == 0 && Blueches == 0 && GreenChes != 0)
        { Turn_Text.text = PlayersNames[1] + " Win!"; Turn_Text_second.text = Turn_Text.text; return true; }
        else if (Blueches == 0 && GreenChes == 0 && RedChes != 0)
        { Turn_Text.text = PlayersNames[2] + " Win!"; Turn_Text_second.text = Turn_Text.text; return true; }
        else if (RedChes == 0 && GreenChes == 0 && Blueches != 0)
        { Turn_Text.text = PlayersNames[0] + " Win!"; Turn_Text_second.text = Turn_Text.text; return true; }
        return false;
    }

    // Some SHIT here
    private void MovePiece(GameObject touchedObject)
    {
        if (touchedObject.name.Contains(PlayersNames[Turn - 1]) || touchedObject.name.ToString().All(char.IsDigit))
        {
            if (LstTouchedObject != null && LstTouchedObject.name.Equals(touchedObject.name)) // see if last touched object is not touched object
            {
                LstTouchedObject.GetComponent<Renderer>().material.color = Color.white; // clear color of piece
                LstTouchedObject = null;
            }
            else if (!touchedObject.name.ToString().All(char.IsDigit)) // if we touched piece
            {
                LstTouchedObject = touchedObject;
                // Set color of selected piece
                LstTouchedObject.GetComponent<Renderer>().material.color = new Color32(255, 215, 0, 255);//new Color(1.0f, 0.92f, 0.016f, 1.0f);
            }
            // if we want to move our piece to other position
            else if (LstTouchedObject != null && !touchedObject.name.Equals(LstTouchedObject.name) && touchedObject.name.ToString().All(char.IsDigit))
            {
                if (CheckMove(touchedObject) == true)
                {
                    //GetComponent<AudioSource>().Play();
                    LstTouchedObject.transform.position = new Vector3(touchedObject.transform.position.x, touchedObject.transform.position.y, touchedObject.transform.position.z - 0.5f);
                    LstTouchedObject.GetComponent<Renderer>().material.color = Color.white;
                    LstTouchedObject = null;
                    NextTurn();
                    Turn_Text_second.text = Turn_Text.text;
                }
                else
                {
                    LstTouchedObject.GetComponent<Renderer>().material.color = Color.white;
                    LstTouchedObject = null;
                }
            }
        }
    }

    private bool CheckMove(GameObject touchedObject)
    {
        bool push = Attack(touchedObject);
        // Set radius ( just move or atack )
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(new Vector2(touchedObject.transform.position.x, touchedObject.transform.position.y), push ? 0.7f : 1.4f);
        foreach (Collider2D value in hitColliders)
        {
            if (value.gameObject.transform.position.Equals(touchedObject.transform.position) && push)
            {
                if (Vector2.Distance(new Vector2(LstTouchedObject.transform.position.x, LstTouchedObject.transform.position.y), new Vector2(touchedObject.transform.position.x, touchedObject.transform.position.y)) < 3.4)
                {
                    return true;
                }
                else
                    return false;
            }
            else if (value.gameObject.transform.position == LstTouchedObject.transform.position)
                return true;
        }
        return false;
    }

    private bool Attack(GameObject touchedObject)
    {
        if (Vector2.Distance(new Vector2(LstTouchedObject.transform.position.x, LstTouchedObject.transform.position.y),
                             new Vector2(touchedObject.transform.position.x, touchedObject.transform.position.y)) > 3.2)//3.2 work Good but 1 not work (right TOP)
            return false;
        RaycastHit2D[] hits = Physics2D.LinecastAll(new Vector2(LstTouchedObject.transform.position.x, LstTouchedObject.transform.position.y),
                                                    new Vector2(touchedObject.transform.position.x, touchedObject.transform.position.y));
        foreach (RaycastHit2D value in hits)
        {
            if (value.transform.name.Contains("(Clone)"))
            {
                switch (Turn)
                {
                    case 1:
                        if (value.transform.name.Contains(PlayersNames[1]))
                        {
                            --GreenChes;
                            Destroy(value.transform.gameObject);
                            return true;
                        }
                        else if (value.transform.name.Contains(PlayersNames[2]))
                        {
                            --RedChes;
                            Destroy(value.transform.gameObject);
                            return true;
                        }
                        break;

                    case 2:
                        if (value.transform.name.Contains(PlayersNames[0]))
                        {
                            --Blueches;
                            Destroy(value.transform.gameObject);
                            return true;
                        }
                        else if (value.transform.name.Contains(PlayersNames[2]))
                        {
                            --RedChes;
                            Destroy(value.transform.gameObject);
                            return true;
                        }
                        break;

                    case 3:
                        if (value.transform.name.Contains(PlayersNames[1]))
                        {
                            --GreenChes;
                            Destroy(value.transform.gameObject);
                            return true;
                        }
                        else if (value.transform.name.Contains(PlayersNames[0]))
                        {
                            --Blueches;
                            Destroy(value.transform.gameObject);
                            return true;
                        }
                        break;

                    default: break;
                }
            }
        }
        return false;
    }

    private void NextTurn()
    {
        if (EndGame() == true)
        {
        }
        else
        {
            switch (Turn)
            {
                case 1:
                    if (GreenChes == 0)
                    {
                        Turn_Text.text = PlayersNames[2] + " Turn!"; Turn += 2;
                    }
                    else
                    {
                        Turn_Text.text = PlayersNames[1] + " turn!"; ++Turn;
                    }
                    break;

                case 2:
                    if (RedChes == 0)
                    {
                        Turn_Text.text = PlayersNames[0] + " Turn!"; Turn += 2;
                    }
                    else
                    {
                        Turn_Text.text = PlayersNames[2] + " turn!"; ++Turn;
                    }
                    break;

                case 3:
                    if (Blueches == 0)
                    {
                        Turn_Text.text = PlayersNames[1] + " Turn!"; Turn = 1; // added + maybe here bug
                    }
                    else
                    {
                        Turn_Text.text = PlayersNames[0] + " turn!"; Turn = 1;
                    }
                    break;

                default:
                    Turn_Text.text = "Error!"; break;
            }
            Turn_Text_second.text = Turn_Text.text;
        }
    }
}
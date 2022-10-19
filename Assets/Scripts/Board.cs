using System;
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

    // Currect touched object
    private Vector3 touchPosWorld;

    // Phase of touching
    private readonly TouchPhase touchPhase = TouchPhase.Ended;

    //Bot mid
    private BotMind BM0;

    private BotMind BM1;

    private BotMind BM2;

    #endregion Global components

    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;  // Never sleep
        GenerateBoard();                                // Create board for game and place there checkers
        // Bot initialization
        BM0 = new BotMind(PlayersNames[0]);
        BM1 = new BotMind(PlayersNames[1]);
        BM2 = new BotMind(PlayersNames[2]);
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            LoadBalancer b1 = LoadBalancer.GetLoadBalancer();
            LoadBalancer b2 = LoadBalancer.GetLoadBalancer();
            LoadBalancer b3 = LoadBalancer.GetLoadBalancer();
            LoadBalancer b4 = LoadBalancer.GetLoadBalancer();
            // Same instance?
            if (b1 == b2 && b2 == b3 && b3 == b4)
            {
                GameObject item = Pieces[Random.Range(0, Pieces.Length)];
                // Find enemy pices near bot piece[i]
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(new Vector2(item.transform.position.x, item.transform.position.y), 1)
                .Where(val => val.transform.name.Contains("(Clone)") && !val.transform.name.Contains(BotName)).ToArray();
                Console.WriteLine("Same instance\n");
            }
            // Load balance 15 server requests
            LoadBalancer balancer = LoadBalancer.GetLoadBalancer();
            for (int i = 0; i < 15; i++)
            {
                string server = balancer.Server;
                Console.WriteLine("Dispatch Request to: " + server);
            }
            // Wait for user
            Console.ReadKey();
        }
    }
  
    public class LoadBalancer
    {
        static LoadBalancer instance;
        List<string> servers = new List<string>();
        Random random = new Random();
        // Lock synchronization object
        private static object locker = new object();
        // Constructor (protected)
        protected LoadBalancer()
        {
            // List of available servers
            servers.Add("ServerI");
            servers.Add("ServerII");
            servers.Add("ServerIII");
            servers.Add("ServerIV");
            servers.Add("ServerV");
        }
        public static LoadBalancer GetLoadBalancer()
        {
            // Support multithreaded applications through
            // 'Double checked locking' pattern which (once
            // the instance exists) avoids locking each
            // time the method is invoked
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new LoadBalancer();
                    }
                }
            }
            return instance;
        }
        // Simple, but effective random load balancer
        public string Server
        {
            get
            {
                using var reader = new ResourceReader(ResxFile);
                var resx = reader.Cast<DictionaryEntry>().ToList();
                var existingResource = resx.FirstOrDefault(r => r.Key.ToString() == key);
                {
                    var modifiedResx = new DictionaryEntry()
                        { Key = existingResource.Key, Value = value };
                    resx.Remove(existingResource);  // Remove resource
                    resx.Add(modifiedResx);  // and then add new one
                }
                int r = random.Next(servers.Count);
                return servers[r].ToString();
            }
        }
    }


    // Update is called once per frame
    private void Update()
    {
        try
        {
            #region IfBotPlay

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

            #endregion IfBotPlay

            //if (Input.GetKey(KeyCode.Escape)) // If player touch escape on phone
            //{
            // Create winodws with word " Are you sure want exit from game? "

            //UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            //}
            // If we touch something
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == touchPhase)
            {
                // Get position were we touch
                touchPosWorld = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                // Set touched position to Vector2
                Vector2 touchPosWorld2D = new Vector2(touchPosWorld.x, touchPosWorld.y);
                // Get information what we touch
                RaycastHit2D hitInformation = Physics2D.Raycast(touchPosWorld2D, Camera.main.transform.forward);
                // If object have colider (all visible must have)
                 GameObject Piece = Pieces[Random.Range(0, Pieces.Length)];
                // Get all hit's in radius near this piece
                Collider2D[] hits = Physics2D.OverlapCircleAll(new Vector2(Piece.transform.position.x, Piece.transform.position.y), 1)
                     .Where(vval => vval.transform.name.Contains("(Clone)") && !vval.transform.name.Contains(BotName)).ToArray();
                // And find only free places
                hits = Physics2D.OverlapCircleAll(new Vector2(Piece.transform.position.x, Piece.transform.position.y), 1)
                .Where(val => val.transform.name.All(char.IsDigit)).ToArray();
                if (hitInformation.collider != null)
                {
                    GameObject touchedObject = hitInformation.transform.gameObject;
                    // Someone win?
                    if (EndGame())
                    {
                    }
                    // Nope, play further
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
    
      private bool CheckBack(GameObject BotObj, Collider2D[] HitInRadius)
    {
        // Get line from Bot piece to enemy piece and check if behaind enemy is empty place if it is -> attack
        foreach (Collider2D val in HitInRadius)
        {
            //Get Place behind enemy and check if it empty if it empty, check if there some enemy too
            ////                                        FIX THIS PLACE(Maybe fixed) //Test Here
            Debug.Log(Physics2D.LinecastAll(new Vector3(val.transform.position.x, val.transform.position.y),
                new Vector2(val.transform.position.x - (BotObj.transform.position.x - val.transform.position.x),
                val.transform.position.y - (BotObj.transform.position.y - val.transform.position.y)
                )).Where(vall => vall.transform.name.Contains("(Clone)") && !vall.transform.name.Contains(BotName)).ToArray().Length);

            if (Physics2D.LinecastAll(new Vector3(val.transform.position.x, val.transform.position.y),
                new Vector2(val.transform.position.x - (BotObj.transform.position.x - val.transform.position.x),
                val.transform.position.y - (BotObj.transform.position.y - val.transform.position.y)
                )).Where(vall => vall.transform.name.Contains("(Clone)") && !vall.transform.name.Contains(BotName)).ToArray().Length == 1)
            {
                //if (Physics2D.OverlapCircleAll(new Vector2(val.transform.position.x, val.transform.position.y), 1)
                // .Where(vval => vval.transform.name.Contains("(Clone)") && !vval.transform.name.Contains(BotName)).ToArray().Length == 1)
                {
                    BotObj.transform.position = val.transform.position + (BotObj.transform.position - val.transform.position);  // Move piece to new place

                    if (val.transform.name.Contains(Board.PlayersNames[0]))
                        ch.Blueches--;
                    else if (val.transform.name.Contains(Board.PlayersNames[1]))
                        ch.GreenChes--;
                    else
                        ch.RedChes--;
                    UnityEngine.Object.Destroy(val.gameObject); // Destroy enemy checker
                    return true;
                }
            }
        }
        return false;
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
        // And generate piece on board
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

    // Some REALY SHIT here
    private void MovePiece(GameObject touchedObject)
    {
        // Сheck if we touch our piece or we touch free board place
        if (touchedObject.name.Contains(PlayersNames[Turn - 1]) || touchedObject.name.ToString().All(char.IsDigit))
        {
            // See if last touched object is not touched object
            if (LstTouchedObject != null && LstTouchedObject.name.Equals(touchedObject.name))
            {
                // clear color of piece
                LstTouchedObject.GetComponent<Renderer>().material.color = Color.white;
                LstTouchedObject = null;
            }
            // if we touched our piece to pickup
            else if (!touchedObject.name.ToString().All(char.IsDigit))
            {
                // Last touched object will be our toched piece
                LstTouchedObject = touchedObject;
                // Set color of selected piece to new one
                LstTouchedObject.GetComponent<Renderer>().material.color = new Color32(255, 215, 0, 255);//new Color(1.0f, 0.92f, 0.016f, 1.0f);
            }
            // if we want to move our piece to other position
            else if (LstTouchedObject != null && !touchedObject.name.Equals(LstTouchedObject.name) && touchedObject.name.ToString().All(char.IsDigit))
            {
                if (CheckMove(touchedObject) == true)
                {
                    // Play some shit
                    //GetComponent<AudioSource>().Play();
                    // Move to new position
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
        // will we attack?
        bool AttackMode = Attack(touchedObject);
        // Set radius ( just move or atack )
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(new Vector2(touchedObject.transform.position.x, touchedObject.transform.position.y), AttackMode ? 0.7f : 1.4f);
        foreach (Collider2D value in hitColliders)
        {
            // Can we Attack and move piece to new position ( finally test )
            if (value.gameObject.transform.position.Equals(touchedObject.transform.position) && AttackMode)
            {
                // check if distance is valid
                if (Vector2.Distance(new Vector2(LstTouchedObject.transform.position.x, LstTouchedObject.transform.position.y), new Vector2(touchedObject.transform.position.x, touchedObject.transform.position.y)) < 3.4)
                {
                    return true;
                }
                else
                    return false;
            }
            // If new position is last position what we toched, we canceled our selected piece and color will return to normal
            else if (value.gameObject.transform.position == LstTouchedObject.transform.position)
                return true;
        }
        return false;
    }

    // Сheck if we can attack and then attack...
    private bool Attack(GameObject touchedObject)
    {
        // Can we attack?
        if (Vector2.Distance(new Vector2(LstTouchedObject.transform.position.x, LstTouchedObject.transform.position.y),
                             new Vector2(touchedObject.transform.position.x, touchedObject.transform.position.y)) > 3.2)//3.2 work Good but 1 not work (right TOP)
            return false;
        // Check what piece was attackated
        RaycastHit2D[] hits = Physics2D.LinecastAll(new Vector2(LstTouchedObject.transform.position.x, LstTouchedObject.transform.position.y),
                                                    new Vector2(touchedObject.transform.position.x, touchedObject.transform.position.y));
        // And remove it
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
        //if we don't attack anything in end
        return false;
    }

    // Just next turn nothing interesting here
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

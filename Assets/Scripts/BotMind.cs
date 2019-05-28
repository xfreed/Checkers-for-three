using System.Linq;
using UnityEngine;

public class BotMind /*: MonoBehaviour*/
{
    private readonly Board ch = new Board();
    public string BotName;

    public BotMind(string botname)
    {
        BotName = botname;
    }

    public void WhereWeGo()
    {
        // We only go or attack
        bool IfWeGo = false;
        //Find all bot piece's
        GameObject[] Pieces = GameObject.FindGameObjectsWithTag("piece").Where(val => val.name.Contains(BotName)).ToArray();
        for (int i = 0; i < Pieces.Length; ++i)//foreach (GameObject item in GO)
        {
            //Get piece[i]
            GameObject item = Pieces[Random.Range(0, Pieces.Length)];
            // Find enemy pices near bot piece[i]
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(new Vector2(item.transform.position.x, item.transform.position.y), 1)
            .Where(val => val.transform.name.Contains("(Clone)") && !val.transform.name.Contains(BotName)).ToArray();

            //if there is some enemy piece check if we can attack them and check is it good place to move piece[i](check if enemy is nearby)
            //nearby hasn't enemy pieces
            if (hitColliders.Length == 0)
            {
                // If we move piece[i] will we be attacked?
                IfWeGo = CheckMove(item);
                if (IfWeGo)
                    break;
            }
            else
            {
                // If we attack enemy pice can he attack back by other his piece?
                IfWeGo = (CheckAttack(item, hitColliders));
                if (IfWeGo)
                    break;
            }
        }
        //If we already move bot piece ( attack or move )
        /* THIS PLACE RETURN ALWAYS TRUE */
        if (IfWeGo)
            return;
        // if we don't do anything
        for (int i = 0; i < Pieces.Length; ++i)//foreach (GameObject item in GO)
        {
            // Get random bot piece
            GameObject Piece = Pieces[Random.Range(0, Pieces.Length)];
            // Get all hit's in radius near this piece
            Collider2D[] hits = Physics2D.OverlapCircleAll(new Vector2(Piece.transform.position.x, Piece.transform.position.y), 1)
                 .Where(vval => vval.transform.name.Contains("(Clone)") && !vval.transform.name.Contains(BotName)).ToArray();
            // And find only free places
            hits = Physics2D.OverlapCircleAll(new Vector2(Piece.transform.position.x, Piece.transform.position.y), 1)
            .Where(val => val.transform.name.All(char.IsDigit)).ToArray();

            foreach (Collider2D val in hits)
            {
                // If placse is on valid distance
                if (Physics2D.OverlapCircleAll(new Vector2(val.transform.position.x, val.transform.position.y), 0.3f).Length == 1)
                {
                    // Move piece
                    Piece.transform.position = new Vector3(val.transform.position.x, val.transform.position.y, val.transform.position.z - 0.5f);
                    return;
                }
            }
        }
    }

    private bool CheckMove(GameObject BotObj)
    {
        // Scan by radius if enemy piece is nearby
        Collider2D[] hits = Physics2D.OverlapCircleAll(new Vector2(BotObj.transform.position.x, BotObj.transform.position.y), 1)
                 .Where(vval => vval.transform.name.Contains("(Clone)") && !vval.transform.name.Contains(BotName)).ToArray();
        // Hasn't? return false
        if (hits.Length != 0)
            return false;
        // Else we move bot piece to the new position
        hits = Physics2D.OverlapCircleAll(new Vector2(BotObj.transform.position.x, BotObj.transform.position.y), 1)
            .Where(val => val.transform.name.All(char.IsDigit)).ToArray();

        foreach (Collider2D val in hits)
        {
            if (Physics2D.OverlapCircleAll(new Vector2(val.transform.position.x, val.transform.position.y), 0.3f).Length == 1)
            {
                BotObj.transform.position = new Vector3(val.transform.position.x, val.transform.position.y, val.transform.position.z - 0.5f);
                break;
            }
        }
        // and return True
        return true;
    }

    private bool CheckAttack(GameObject BotObj, Collider2D[] HitInRadius)
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
}
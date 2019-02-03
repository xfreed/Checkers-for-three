using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotMind /*: MonoBehaviour*/
{
    private Board ch = new Board();
    public string BotName;

    public BotMind(string botname)
    {
        BotName = botname;
    }

    public void WhereWeGo()
    {
        bool IfWeGo = false;
        GameObject[] GO = GameObject.FindGameObjectsWithTag("piece").Where(val => val.name.Contains(BotName)).ToArray();
        for (int i = 0; i < GO.Length; ++i)//foreach (GameObject item in GO)
        {
            GameObject item = GO[Random.Range(0, GO.Length)];
            // Find enemy pices behind bot piece
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(new Vector2(item.transform.position.x, item.transform.position.y), 1)
            .Where(val => val.transform.name.Contains("(Clone)") && !val.transform.name.Contains(BotName)).ToArray();

            //if there is some enemy piece check if we can attack them || check is it good place to move piece(check if enemy is nearby)
            if (hitColliders.Length == 0)
            {
                IfWeGo = CheckMove(item);
                if (IfWeGo)
                    break;
            }
            else
            {
                IfWeGo = (CheckAttack(item, hitColliders));
                if (IfWeGo)
                    break;
            }
        }
        if (IfWeGo)
            return;
        for (int i = 0; i < GO.Length; ++i)//foreach (GameObject item in GO)
        {
            GameObject item = GO[Random.Range(0, GO.Length)];
            Collider2D[] hits = Physics2D.OverlapCircleAll(new Vector2(item.transform.position.x, item.transform.position.y), 1)
                 .Where(vval => vval.transform.name.Contains("(Clone)") && !vval.transform.name.Contains(BotName)).ToArray();
            hits = Physics2D.OverlapCircleAll(new Vector2(item.transform.position.x, item.transform.position.y), 1)
            .Where(val => val.transform.name.All(char.IsDigit)).ToArray();

            foreach (Collider2D val in hits)
            {
                if (Physics2D.OverlapCircleAll(new Vector2(val.transform.position.x, val.transform.position.y), 0.3f).Length == 1)
                {
                    item.transform.position = new Vector3(val.transform.position.x, val.transform.position.y, val.transform.position.z - 0.5f);
                    return;
                }
            }
        }
    }

    private bool CheckMove(GameObject BotObj)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(new Vector2(BotObj.transform.position.x, BotObj.transform.position.y), 1)
                 .Where(vval => vval.transform.name.Contains("(Clone)") && !vval.transform.name.Contains(BotName)).ToArray();
        if (hits.Length != 0)
            return false;

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

    private void LessChekcers()
    {
    }
}
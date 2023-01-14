using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UserInput : MonoBehaviour
{
    public GameObject slot1;
    private Solitaire solitaire;
    // Start is called before the first frame update
    void Start()
    {
        solitaire = FindObjectOfType<Solitaire>();
        slot1 = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        GetMouseClick();
    }

    void GetMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -10));
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit)
            {
                //what has been hit? Deck/Card/EmptySlot...
                if (hit.collider.CompareTag("Deck"))
                {
                    //clicked deck
                    Deck();
                }
                else if (hit.collider.CompareTag("Card"))
                {
                    //clicked card
                    Card(hit.collider.gameObject);
                }
                else if (hit.collider.CompareTag("Top"))
                {
                    //clicked top
                    Top(hit.collider.gameObject);
                }
                else if (hit.collider.CompareTag("Bottom"))
                {
                    //clicked bottom
                    Bottom(hit.collider.gameObject);
                }
            }
        }
    }

    void Deck()
    {
        Debug.Log("Clicked on deck");
        StartCoroutine(solitaire.DealFromDeck());
    }
    
    void Card(GameObject selected)
    {
        Debug.Log("Clicked on card");
        if (!selected.GetComponent<Selectable>().faceUp) //if card clicked is face down
        {
            if (!Blocked(selected))//if card clicked is not blocked
            {
                //flip over
                selected.GetComponent<Selectable>().faceUp = true;
                slot1 = this.gameObject;
            }
        }
        else if(selected.GetComponent<Selectable>().inDeckPile)
        {
            if (!Blocked(selected))
            {
                slot1 = selected;
            }
        }

        //if card clicked is on deck pile with trips
            //if it is not blocked
                //select it

        //if card face up
            //if there is no card currently selected
                //select the card
        if(slot1 == this.gameObject)
        {
            slot1 = selected;
        }

        //if there is a card selected (and is not the same card)
        else if(slot1 != selected)
        {
            //if the new card is eligible to stack on the old card
            if (Stackable(selected))
            {
                //stack it
                Stack(selected);
            }
            else
            {
                //select new card
                slot1 = selected;
            }
        }

        //else if there is a card selected and it is the same card
        //if the time is short enough, is double click
        //if the card is eligible to fly up to top, do it
    }

    void Top(GameObject selected)
    {
        Debug.Log("Clicked on top");
        if (slot1.CompareTag("Card"))
        {
            //if the card is an ace and the empty slot is top then stack
            if(slot1.GetComponent<Selectable>().value == 1)
            {
                Stack(selected);
            }
        }
    }

    void Bottom(GameObject selected)
    {
        Debug.Log("Clicked on bottom");
        //if the card is a king and the empty slot is bottom then stack
        if (slot1.CompareTag("Card"))
        {
            if(slot1.GetComponent<Selectable>().value == 13)
            {
                Stack(selected);
            }
        }
    }

    bool Stackable(GameObject selected)
    {
        Selectable s1 = slot1.GetComponent<Selectable>();
        Selectable s2 = selected.GetComponent<Selectable>();
        //compare them to see if they stack

        if (!s2.inDeckPile)
        {
            //if in top pile must stack suited Ace to King
            if (s2.top)
            {
                if (s1.suit == s2.suit || (s1.value == 1 && s2.suit == null))
                {
                    if (s1.value == s2.value + 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else //if in bottom pile must stack alternate colors King to Ace
            {
                if (s1.value == s2.value - 1)
                {
                    bool card1Red = true;
                    bool card2Red = true;

                    if (s1.suit == "C" || s1.suit == "S")
                    {
                        card1Red = false;
                    }

                    if (s2.suit == "C" || s2.suit == "S")
                    {
                        card2Red = false;
                    }

                    if (card1Red == card2Red)
                    {
                        Debug.Log("Not Stackable");
                        return false;
                    }
                    else
                    {
                        Debug.Log("Stackable");
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void Stack(GameObject selected)
    {
        //if on top of king or empty bottom stack the cards in place
        //else stack the cards with a negative y offset

        Selectable s1 = slot1.GetComponent<Selectable>();
        Selectable s2 = selected.GetComponent<Selectable>();
        float yOffset = 0.3f;

        if (s2.top || (!s2.top && s1.value == 13))
        {
            yOffset = 0;
        }

        slot1.transform.position = new Vector3(selected.transform.position.x, selected.transform.position.y - yOffset, selected.transform.position.z - 0.1f);
        slot1.transform.parent = selected.transform; //makes children move with the parents

        if (s1.inDeckPile) //removes the cards from the top pile to prevent duplicate cards
        {
            solitaire.tripsOnDisplay.Remove(slot1.name);
        }
        else if (s1.top && s2.top && s1.value == 1) //allows movement of cards between top spots
        {
            solitaire.topPos[s1.row].GetComponent<Selectable>().value = 0;
            solitaire.topPos[s1.row].GetComponent<Selectable>().suit = null;
        }
        else if (s1.top) //keeps track of current value of the top decks as a card has been removed
        {
            solitaire.topPos[s1.row].GetComponent<Selectable>().value = s1.value - 1;
        }
        else //removes the card string from the appropriate bottom list
        {
            solitaire.bottoms[s1.row].Remove(slot1.name);
        }

        s1.inDeckPile = false; //you cannot add cards to the trops pile so this is always fine
        s1.row = s2.row;

        if (s2.top)
        {
            solitaire.topPos[s1.row].GetComponent<Selectable>().value = s1.value;
            solitaire.topPos[s1.row].GetComponent<Selectable>().suit = s1.suit;
            s1.top = true;
        }
        else
        {
            s1.top = false;
        }

        //after completing move reset slot1 to be essentially null as being null will break the logic
        slot1 = this.gameObject;
    }

    bool Blocked(GameObject selected)
    {
        Selectable s2 = selected.GetComponent<Selectable>();
        if (s2.inDeckPile == true)
        {
            if(s2.name == solitaire.tripsOnDisplay.Last())
            {
                return false;
            }
            else
            {
                Debug.Log(s2.name + " is blocked by " + solitaire.tripsOnDisplay.Last());
                return true;
            }
        }
        else
        {
            if(s2.name == solitaire.bottoms[s2.row].Last()) //check if it is the bottom card
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}

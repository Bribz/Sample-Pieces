using System;

namespace CardDeck
{
    enum Suit
    {
        Spades,
        Hearts,
        Clubs,
        Diamonds 
    }
    
    enum Face
    {
        Ace = 1,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King
    }

    class Program
    {
        static void Main(string[] args)
        {
            Deck dTest = new Deck();
            dTest.Print();
            
            dTest.Shuffle();

            dTest.Print();            
        }
    }

    class Deck
    {
        private Card[] deck;
        private const int NUM_CARDS = 52; // This shouldn't change.
        
        public Deck() // Create standard 52 card deck
        {
            deck = new Card[NUM_CARDS];
            GenerateDeck();
        }

        public Deck(int _numCards) // Create more than 52 card deck
        {
            deck = new Card[_numCards];
            GenerateDeck();
        }


        public void Print()
        {
            foreach(Card c in deck)
            {
                c.Print();
            }
        }

        public void Shuffle() //Fisher-Yates Shuffle Algorithm
        {
            Card temp; 
            Random r = new Random(25565);
            int tmpIndex;
            for(int i = deck.Length-1; i > 0; i--)
            {
                temp = deck[0];
                tmpIndex = r.Next(deck.Length);
                deck[0] = deck[tmpIndex];
                deck[tmpIndex] = temp;
            }
        }

        private void GenerateDeck()
        {
            Suit currSuit = Suit.Spades;
            Face currFace = Face.Ace;
            for(int i = 0; i < deck.Length; i++)
            {
                deck[i].CARD_FACE = currFace;
                deck[i].CARD_SUIT = currSuit;

                if (currFace < Face.King)
                {
                    currFace++;
                }
                else
                {
                    currFace = Face.Ace;
                    if (currSuit < Suit.Diamonds)
                    {
                        currSuit++;
                    }
                    else
                    {
                        currSuit = Suit.Spades;
                    }
                }
            }
        }
    }

    struct Card
    {
        public Suit CARD_SUIT;
        public Face CARD_FACE;
        public static int CURR_NUM_CARDS = 0;

        public Card(Suit _suit, Face _face)
        {
            CARD_SUIT = _suit;
            CARD_FACE = _face;
            CURR_NUM_CARDS++;
        }

        public void Print()
        {
            System.Console.WriteLine("Card: "+CARD_FACE+" of "+CARD_SUIT);
        }
    }
}

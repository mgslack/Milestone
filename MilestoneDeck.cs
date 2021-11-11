using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

/*
 * Primary class defines the card deck for playing milestone (mille bornes).
 * Methods are called to shuffle, get next card and manage the milestone cards
 * within the deck.
 *
 * Author:  M. G. Slack
 * Written: 2014-02-11
 *
 * ----------------------------------------------------------------------------
 * 
 * Updated: yyyy-mm-dd -
 *
 */
namespace Milestone
{
    /* Milestone cards that can be gotten from the milestone deck. */
    public enum MilestoneCards { Empty_Card,
                                 Out_Of_Gas, Flat_Tire, Accident, Speed_Limit_50, Stop,
                                 Gas, Spare_Tire, Repairs, End_Speed_Limit, Roll,
                                 Extra_Tank, Permanent_Tire, Driving_Ace, Right_Of_Way,
                                 M200, M100, M75, M50, M25 };

    /*
     * Class used to create 'pretty' name strings from each of the milestone cards
     * defined in the enum
     */
    public static class MilestoneCardsTextExtender
    {
        public static string AsText(this MilestoneCards card)
        {
            switch(card) {
                case MilestoneCards.Empty_Card: return "Empty Card";
                case MilestoneCards.Out_Of_Gas: return "Out of Gas";
                case MilestoneCards.Flat_Tire: return "Flat Tire";
                case MilestoneCards.Accident: return "Accident";
                case MilestoneCards.Speed_Limit_50: return "Speed Limit 50";
                case MilestoneCards.Stop: return "Stop";
                case MilestoneCards.Gas: return "Gas";
                case MilestoneCards.Spare_Tire: return "Spare Tire";
                case MilestoneCards.Repairs: return "Repairs";
                case MilestoneCards.End_Speed_Limit: return "End Speed Limit";
                case MilestoneCards.Roll: return "Roll";
                case MilestoneCards.Extra_Tank: return "Extra Tank";
                case MilestoneCards.Permanent_Tire: return "Permanent Tire";
                case MilestoneCards.Driving_Ace: return "Driving Ace";
                case MilestoneCards.Right_Of_Way: return "Right-of-Way";
                case MilestoneCards.M200: return "200 Mile";
                case MilestoneCards.M100: return "100 Mile";
                case MilestoneCards.M75: return "75 Mile";
                case MilestoneCards.M50: return "50 Mile";
                case MilestoneCards.M25: return "25 Mile";
                default: return "Unknown MilestoneCard - " + Convert.ToString((int) card);
            }
        }
    }

    public class MilestoneDeck
    {
        /* Number of cards in the milestone deck. */
        public const int MAX_CARDS_IN_DECK = 106;

        private MilestoneCards[] _deck = new MilestoneCards[MAX_CARDS_IN_DECK];
        private int nextCard = 0;
        private bool _shuffled = false;

        /* Deck shuffled or not. */
        public bool Shuffled { get { return _shuffled; } }
        /* Number of cards left in the deck. */
        public int CardsLeft { get { return MAX_CARDS_IN_DECK - nextCard; } }

        // --------------------------------------------------------------------

        /*
         * Constructor that will create a milestone deck of milestone cards
         * with 106 cards in it.  The deck is created unsorted.
         */
        public MilestoneDeck()
        {
            for (int i = MAX_CARDS_IN_DECK - 1; i >= 0; i--) {
                if (i >= 96)
                    _deck[i] = MilestoneCards.M25;   // 10
                else if (i >= 86)
                    _deck[i] = MilestoneCards.M50;   // 10
                else if (i >= 76)
                    _deck[i] = MilestoneCards.M75;   // 10
                else if (i >= 64)
                    _deck[i] = MilestoneCards.M100;  // 12
                else if (i >= 60)
                    _deck[i] = MilestoneCards.M200;  // 4
                else if (i == 59)
                    _deck[i] = MilestoneCards.Right_Of_Way;   // 1
                else if (i == 58)
                    _deck[i] = MilestoneCards.Driving_Ace;    // 1
                else if (i == 57)
                    _deck[i] = MilestoneCards.Permanent_Tire; // 1
                else if (i == 56)
                    _deck[i] = MilestoneCards.Extra_Tank;     // 1
                else if (i >= 42)
                    _deck[i] = MilestoneCards.Roll;            // 14
                else if (i >= 36)
                    _deck[i] = MilestoneCards.End_Speed_Limit; // 6
                else if (i >= 30)
                    _deck[i] = MilestoneCards.Repairs;         // 6
                else if (i >= 24)
                    _deck[i] = MilestoneCards.Spare_Tire;      // 6
                else if (i >= 18)
                    _deck[i] = MilestoneCards.Gas;             // 6
                else if (i >= 13)
                    _deck[i] = MilestoneCards.Stop;           // 5
                else if (i >= 9)
                    _deck[i] = MilestoneCards.Speed_Limit_50; // 4
                else if (i >= 6)
                    _deck[i] = MilestoneCards.Accident;       // 3
                else if (i >= 3)
                    _deck[i] = MilestoneCards.Flat_Tire;      // 3
                else
                    _deck[i] = MilestoneCards.Out_Of_Gas;     // 3
            }
        }

        // --------------------------------------------------------------------

        /*
         * Method used to shuffle the deck and ready it for use. Calling
         * shuffle resets the deck.
         */
        public void Shuffle()
        {
            SingleRandom rnd = SingleRandom.Instance;

            nextCard = 0;
            _shuffled = true;

            // pseudo shuffle
            for (int i = 0; i < 10; i++) {
                for (int j = 0; j < MAX_CARDS_IN_DECK; j++) {
                    int p = (int) Math.Floor(rnd.NextDouble() * MAX_CARDS_IN_DECK);
                    // swap card at j with card at p
                    MilestoneCards c = _deck[j];
                    _deck[j] = _deck[p];
                    _deck[p] = c;
                }
            }
        }

        /*
         * Method to determine if the deck has more cards to deal out.
         * Returns true if more cards can be dealt, false if not.
         */
        public bool HasMoreCards()
        {
            return nextCard < MAX_CARDS_IN_DECK;
        }

        /*
         * Method used to get the next card in the deck and return it
         * to the caller. Card is considered 'played' and removed
         * from the deck if LeaveInDeck is false. Card is left in
         * the deck (not played) if LeaveInDeck is true. May return
         * an 'Empty_Card' if no more cards are available to play in
         * the deck.
         */
        public MilestoneCards GetNextCard(bool LeaveInDeck)
        {
            MilestoneCards ret = MilestoneCards.Empty_Card;

            if (HasMoreCards()) {
                ret = _deck[nextCard];
                if (!LeaveInDeck) nextCard++;
            }

            return ret;
        }

        /*
         * Method used to get the next card in the deck and return it
         * to the caller. Card is removed from the deck. May return
         * an 'Empty_Card' if no more cards are available to play in
         * the deck.
         */
        public MilestoneCards GetNextCard()
        {
            return GetNextCard(false);
        }

        /*
         * Method returns a read-only collection of the milestone card
         * deck. Returns all of the cards in the order they would be
         * returned with GetNextCard, even if they've already been
         * played. The current card that would be returned will have
         * to be determined with other techniques. This method is
         * mainly for testing and debugging.
         */
        public ReadOnlyCollection<MilestoneCards> GetDeckCards()
        {
            return Array.AsReadOnly(_deck);
        }

        /*
         * Method returns the milestone deck in a representative string form.
         * Mainly used for testing and debugging purposes.
         */
        public override string ToString()
        {
            return "MilestoneDeck: (Number of Cards - 106, CurrentCard - " + nextCard +
                   ", shuffled - " + _shuffled + ")";
        }
    }
}

using System;

/*
 * Primary class defines a card hand for playing milestone (mille bornes).
 * Having a defined hand class makes it easier to work with the cards in play
 * rather than managing a set of arrays.
 *
 * Author:  M. G. Slack
 * Written: 2014-02-13
 *
 * ----------------------------------------------------------------------------
 * 
 * Updated: 2022-01-21 - Removed unnecessary usings and added regions to code.
 *
 */
namespace Milestone
{
    public class MilestoneHand
    {
        #region Constants
        /* Number of cards in each milestone hand created (max cards). */
        public const int CARDS_IN_MILESTONE_HAND = 7;
        #endregion

        #region Private vars / public properties
        private MilestoneCards[] cards = new MilestoneCards[CARDS_IN_MILESTONE_HAND];
        private int numCards = 0;

        /* Current number of valid cards in the hand (0 - CARDS_IN_MILESTONE_HAND). */
        public int NumCardsInHand { get { return numCards; } }
        #endregion

        // --------------------------------------------------------------------

        /*
         * Constructor creates a milestone card hand and initializes each slot
         * in the hand to be an 'Empty_Card'.
         */
        public MilestoneHand()
        {
            RemoveAll();
        }

        // --------------------------------------------------------------------

        #region Public methods
        /*
         * Method used to add a milestone card to the hand. Needs an empty
         * slot to add the card to. If at max cards, will return a 'false',
         * otherwise, adds the card to the first empty slot and returns a
         * 'true'.
         */
        public bool Add(MilestoneCards card)
        {
            bool added = false;

            if (numCards != CARDS_IN_MILESTONE_HAND) {
                for (int i = 0; i < CARDS_IN_MILESTONE_HAND; i++) {
                    if (cards[i] == MilestoneCards.Empty_Card) {
                        numCards++;
                        cards[i] = card;
                        added = true;
                        break;
                    }
                }
            }

            return added;
        }

        /*
         * Method used to remove a milestone card from the hand at the
         * given slot and return the card to the caller. Will return a
         * 'Empty_Card' if the slot is out of range. Replaces the card
         * with an 'Empty_Card' at the slot removed from.
         */
        public MilestoneCards Remove(int slot)
        {
            MilestoneCards ret = MilestoneCards.Empty_Card;

            if ((slot >= 0) && (slot < CARDS_IN_MILESTONE_HAND)) {
                ret = cards[slot];
                cards[slot] = MilestoneCards.Empty_Card;
                numCards--;
            }

            return ret;
        }

        /*
         * Method used to remove all cards from the hand and reset back
         * to all empty_cards.
         */
        public void RemoveAll()
        {
            for (int i = 0; i < CARDS_IN_MILESTONE_HAND; i++)
                cards[i] = MilestoneCards.Empty_Card;
            numCards = 0;
        }

        /*
         * Method moves all of the 'Empty_Card' values in the milestone
         * card hand to the end of the hand (all valid cards are in the
         * beginning slots).
         */
        public void CompressHand()
        {
            MilestoneCards[] temp = new MilestoneCards[CARDS_IN_MILESTONE_HAND];
            int j = 0;

            // initialize temporary hand
            for (int i = 0; i < CARDS_IN_MILESTONE_HAND; i++) {
                temp[i] = MilestoneCards.Empty_Card;
            }
            // move good cards over to temp
            for (int i = 0; i < CARDS_IN_MILESTONE_HAND; i++) {
                if (cards[i] != MilestoneCards.Empty_Card) {
                    temp[j++] = cards[i];
                }
            }
            // replace hand with compressed hand
            for (int i = 0; i < CARDS_IN_MILESTONE_HAND; i++) {
                cards[i] = temp[i];
            }
        }

        /*
         * Method returns the card at a given slot from the milestone card
         * hand. If the slot is out of range, an 'Empty_Card' is returned.
         */
        public MilestoneCards CardAt(int slot)
        {
            MilestoneCards ret = MilestoneCards.Empty_Card;

            if ((slot >= 0) && (slot < CARDS_IN_MILESTONE_HAND)) ret = cards[slot];

            return ret;
        }

        /*
         * Method returns the milestone cards in the card hand in a string of
         * values. Mainly used for debugging and testing.
         */
        public override string ToString()
        {
            String ret = "MilestoneHand: [";

            for (int i = 0; i < CARDS_IN_MILESTONE_HAND; i++) {
                ret += "(" + i + ") " + cards[i].AsText();
                if (i < CARDS_IN_MILESTONE_HAND - 1) ret += ", ";
            }
            ret += "]";

            return ret;
        }
        #endregion
    }
}

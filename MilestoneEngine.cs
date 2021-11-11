using System;

/*
 * Primary class defines the card selection routines and play routines used by
 * the computer opponent for the game of Milestone (mille bornes).  Depends on
 * the MilestoneDeck class for the card definitions.
 *
 * Author:  M. G. Slack
 * Written: 2014-02-11
 *
 * ----------------------------------------------------------------------------
 * 
 * Updated: 2014-06-01 - Fixed issue in 'PlaySafety' method where Coup Fourres
 *                       were not being counted for computer when a 'stop' was
 *                       in the battle pile and a right-of-way was played.
 *          2021-11-05 - Removed unnessary 'using' per VS hint.             
 *
 */
namespace Milestone
{
    public enum Players { Computer, Human };

    public class MilestoneEngine
    {
        #region String contants
        private const string MSG_ALREADY_SL = "Already have a speed limit.";
        private const string MSG_ALREADY_HAZ = "Already have a hazard played.";
        private const string MSG_NO_ROLL = "Cannot play hazard, no roll card.";
        private const string MSG_HAVE_SAFETY = "Have the safety, can't play that card.";
        private const string MSG_NO_SL = "Don't have a speed limit to end.";
        private const string MSG_HAVE_HAZ = "Cannot play roll, have a hazard.";
        private const string MSG_HAVE_ROLL = "Already have a roll played.";
        private const string MSG_NOT_THAT_HAZ = "Don't have that hazard, cannot play remedy.";
        private const string MSG_HAVE_SL = "Have a speed limit, cannot play that distance card.";
        private const string MSG_EXCEED_MILEAGE = "That will exceed the round mileage of {0}.";
        private const string MSG_NEED_ROLL = "Need to play roll card before playing mileage cards.";
        #endregion

        #region Public constants
        public const int MAX_PLAYERS = 2;
        public const int DEF_GAME_WIN_POINTS = 5000;
        public const int ROUND_MILEAGE = 1000;
        public const int PLAYED_SAFETY_SCORE = 100;
        public const int COUP_FOURRE_SCORE = 300;
        public const int BONUS_COMPLETE_TRIP_SCORE = 400;
        public const int BONUS_SHUTOUT_SCORE = 500;
        public const int MAX_SAFETIES = 5; // last two always set together (right-of-way)

        /*
         * Hazard/Remedy indicators. Hazards are notated in the battle value of the
         * scratch pad as negative remedies. If remedy played, the battle value is
         * the same as the constant (positive). Battle value must == 5 in order
         * to play mileage cards, speed limits are notated with the bool value.
         * Safeties are index by using the haz_rem_xxx value - 1. If played the
         * right-of-way, both of the last two safeties are marked as true
         * (protected from speed limits and stop cards).
         */
        public const int HAZ_REM_GAS = 1;
        public const int HAZ_REM_TIRE = 2;
        public const int HAZ_REM_REPAIR = 3;
        public const int HAZ_REM_SPEED_LIMIT = 4;
        public const int HAZ_REM_ROLL = 5;
        #endregion

        #region Private constants
        private const int HAZ_START = 1;
        private const int REM_START = 6;
        private const int SAFE_START = 11;
        private const int MILE_START = 15;
        private const int HOLD_MILEAGE = 100;   // hold on, if can, mileage cards >= this
        private const int MAX_TRIES = 20;       // number of tries to pick a reasonable random discard
        private const int NO_SPD_MILEAGE = 949; // speed limit 50 does no good
        private const int END_MILEAGE = 790;    // mileage to start worrying about dumping safety cards (self)
        private const int OPPO_END_MILEAGE = END_MILEAGE - 50; // " " (opponent)
        private const int NO_END_SPD_MILEAGE = 900; // end speed limit is no good (discard)
        #endregion

        #region Engine structs
        public struct Scores {
            public int GrandTotal;
            public int CoupFourres;
            public int Safeties;
            public int RoundTotal;
        }

        public struct ScratchPad {
            public int Mileage;
            public bool SpeedLimit;
            public int BattleValue;
            public bool[] Safeties;
        }

        public struct CompPlay {
            public bool PlayCard;
            public int CardInHand;
            public bool CoupFourre;
            public bool Safety;
            public bool OnOpponent;
        }

        private Scores[] scores = new Scores[MAX_PLAYERS];
        private ScratchPad[] scratchPad = new ScratchPad[MAX_PLAYERS];
        #endregion

        // --------------------------------------------------------------------

        /*
         * Constructor to create the Milestone rule and play engine. Creates
         * all of the internal structures needed to process the game as play
         * continues along with the scoring.
         */
        public MilestoneEngine()
        {
            for (int i = 0; i < MAX_PLAYERS; i++) {
                scores[i] = new Scores();
                ResetScore(i, true);
                scratchPad[i] = new ScratchPad();
                scratchPad[i].Safeties = new bool[MAX_SAFETIES];
                ResetScratchPad(i);
            }
        }

        // --------------------------------------------------------------------

        #region Private Methods
        private void ResetScore(int player, bool resetGrandTotal)
        {
            scores[player].CoupFourres = 0;
            scores[player].Safeties = 0;
            scores[player].RoundTotal = 0;
            if (resetGrandTotal) scores[player].GrandTotal = 0;
        }

        private void ResetScratchPad(int player)
        {
            scratchPad[player].Mileage = 0;
            scratchPad[player].SpeedLimit = false;
            scratchPad[player].BattleValue = 0;
            for (int i = 0; i < MAX_SAFETIES; i++) scratchPad[player].Safeties[i] = false;
        }

        private int GetMileageValue(MilestoneCards card)
        {
            int[] miles = { 200, 100, 75, 50, 25 };
            int ret = 0;

            if ((int) card >= MILE_START) ret = miles[(int) card - MILE_START];

            return ret;
        }

        private int PlaySafety(Players player, MilestoneHand hand, ref bool sf, ref bool cf)
        {
            int card = 0, sfcard = -1, sfp = -1;

            sf = false; cf = false;
            while (card < MilestoneHand.CARDS_IN_MILESTONE_HAND) {
                MilestoneCards _card = hand.CardAt(card);
                int cval = (int) _card;
                if ((_card != MilestoneCards.Empty_Card) && (cval >= SAFE_START) && (cval < MILE_START)) {
                    if ((scratchPad[(int) player].SpeedLimit) && (_card == MilestoneCards.Right_Of_Way))
                        cf = true;
                    sf = true; sfp = cval - SAFE_START + 1; sfcard = card;
                    if (sfp == 4) sfp++; // right-of-way, compare to stop for cf return...
                    if (sfp == -scratchPad[(int) player].BattleValue) cf = true;
                    if (cf) return card;
                }
                card++;
            }

            return sfcard;
        }

        private bool PlayEndSpeedLimit(Players player, MilestoneCards _card)
        {
            if ((_card == MilestoneCards.End_Speed_Limit) &&
                (scratchPad[(int) player].Mileage < NO_END_SPD_MILEAGE))
                return true;
            else
                return false;
        }

        private int PlayRemedy(Players player, MilestoneHand hand)
        {
            int card = 0;

            while (card < MilestoneHand.CARDS_IN_MILESTONE_HAND) {
                MilestoneCards _card = hand.CardAt(card);
                int cval = (int) _card, hz = 0;
                if ((_card != MilestoneCards.Empty_Card) && (cval >= REM_START) && (cval < SAFE_START)) {
                    hz = cval - REM_START + 1;
                    if ((scratchPad[(int) player].BattleValue == -hz) ||
                        ((scratchPad[(int) player].SpeedLimit) && (PlayEndSpeedLimit(player, _card))))
                        return card;
                }
                card++;
            }

            return -1;
        }

        private int PlayRoll(MilestoneHand hand)
        {
            int card = 0;

            while (card < MilestoneHand.CARDS_IN_MILESTONE_HAND) {
                if (hand.CardAt(card) == MilestoneCards.Roll) return card;
                card++;
            }

            return -1;
        }

        private int PlayMileage(Players player, MilestoneHand hand, int limit)
        {
            int card = 0, ret = -1, svMiles = 0;

            while (card < MilestoneHand.CARDS_IN_MILESTONE_HAND) {
                MilestoneCards _card = hand.CardAt(card);
                int cval = (int) _card, miles = GetMileageValue(_card);
                if ((_card != MilestoneCards.Empty_Card) && (cval >= MILE_START)) {
                    if ((miles < limit) &&
                        ((miles + scratchPad[(int) player].Mileage) <= ROUND_MILEAGE) &&
                        (miles > svMiles)) {
                        svMiles = miles;
                        ret = card;
                    }
                }
                card++;
            }

            return ret;
        }

        private int PlayHazard(Players opponent, MilestoneHand hand)
        {
            int card = 0;

            while (card < MilestoneHand.CARDS_IN_MILESTONE_HAND) {
                MilestoneCards _card = hand.CardAt(card);
                int cval = (int) _card, hz = 0;
                if ((_card != MilestoneCards.Empty_Card) && (cval >= HAZ_START) && (cval < REM_START)) {
                    if (((scratchPad[(int) opponent].BattleValue == HAZ_REM_ROLL) &&
                         (_card != MilestoneCards.Speed_Limit_50)) ||
                        ((!scratchPad[(int) opponent].SpeedLimit) && (_card == MilestoneCards.Speed_Limit_50) &&
                         (scratchPad[(int) opponent].Mileage < NO_SPD_MILEAGE))) {
                        hz = cval - HAZ_START;
                        if (!scratchPad[(int) opponent].Safeties[hz]) return card;
                    }
                }
                card++;
            }

            return -1;
        }

        private int PickRandomDiscard(MilestoneHand hand)
        {
            int card = -1, tries = 0, miles = 0;
            SingleRandom rnd = SingleRandom.Instance;

            do {
                if (hand.NumCardsInHand > 1)
                    card = rnd.Next(hand.NumCardsInHand);
                else
                    card = 0;
                if (hand.NumCardsInHand < MilestoneHand.CARDS_IN_MILESTONE_HAND) break;
                miles = GetMileageValue(hand.CardAt(card)); tries++;
            } while ((miles >= HOLD_MILEAGE) && (tries < MAX_TRIES));

            return card;
        }

        private int PickDiscard(Players player, Players opponent, MilestoneHand hand)
        {
            int card = 0;

            // pass 1, pick a card that we definitely can't use
            while (card < MilestoneHand.CARDS_IN_MILESTONE_HAND) {
                MilestoneCards _card = hand.CardAt(card);
                int cval = (int) _card, hz = -1, rem = -1, miles = GetMileageValue(_card);
                if (_card != MilestoneCards.Empty_Card) {
                    if ((cval >= HAZ_START) && (cval < REM_START)) hz = cval - HAZ_START;
                    if ((cval >= REM_START) && (cval < SAFE_START)) rem = cval - REM_START;
                    if (((hz != -1) && (scratchPad[(int) opponent].Safeties[hz])) ||
                        ((_card == MilestoneCards.Speed_Limit_50) &&
                         (scratchPad[(int) opponent].Mileage > NO_SPD_MILEAGE)) ||
                        ((rem != -1) && (scratchPad[(int) player].Safeties[rem])) ||
                        ((cval >= MILE_START) &&
                         (miles > (ROUND_MILEAGE - scratchPad[(int) player].Mileage))))
                        return card;
                }
                card++;
            }
            // pass 2, pick a low mileage card
            card = 0;
            while (card < MilestoneHand.CARDS_IN_MILESTONE_HAND) {
                MilestoneCards _card = hand.CardAt(card);
                if ((_card != MilestoneCards.Empty_Card) && (_card >= MilestoneCards.M75))
                    return card;
                card++;
            }
            // pass 3, pick hazard or remedy card
            card = 0;
            while (card < MilestoneHand.CARDS_IN_MILESTONE_HAND)
            {
                MilestoneCards _card = hand.CardAt(card);
                int cval = (int) _card, hz = -1, rem = -1;
                if ((cval >= HAZ_START) && (cval < REM_START)) hz = cval - HAZ_START;
                if ((cval >= REM_START) && (cval < SAFE_START)) rem = cval - REM_START;
                if (_card != MilestoneCards.Empty_Card) {
                    if (((hz != -1) && (!scratchPad[(int) player].Safeties[hz])) ||
                        ((rem != -1) && (!scratchPad[(int) opponent].Safeties[rem])))
                        return card;
                }
                card++;
            }

            return -1;
        }

        private void ProcessMilesAndScore(CompPlay play, Players player, MilestoneHand hand)
        {
            if (play.PlayCard) {
                Players opponent = (player == Players.Computer) ? Players.Human : Players.Computer;
                int hz = -1, rm = -1, sf = -1, miles = 0;

                GetCardType(hand.CardAt(play.CardInHand), ref hz, ref rm, ref sf, ref miles);
                if (play.OnOpponent) {
                    // has to be a hazard
                    if (hz == HAZ_REM_SPEED_LIMIT)
                        scratchPad[(int) opponent].SpeedLimit = true;
                    else
                        scratchPad[(int) opponent].BattleValue = -hz;
                }
                else {
                    if (rm != -1) {
                        if (rm == HAZ_REM_SPEED_LIMIT)
                            scratchPad[(int) player].SpeedLimit = false;
                        else
                            scratchPad[(int) player].BattleValue = rm;
                    }
                    else {
                        if (sf != -1) {
                            scores[(int) player].Safeties += PLAYED_SAFETY_SCORE;
                            if (play.CoupFourre) scores[(int) player].CoupFourres += COUP_FOURRE_SCORE;
                            scratchPad[(int) player].Safeties[sf] = true;
                            if (sf == 3) { // right-of-way
                                sf++;
                                scratchPad[(int) player].Safeties[sf] = true;
                            }
                            sf++;
                            if (scratchPad[(int) player].BattleValue == -sf)
                                scratchPad[(int) player].BattleValue = sf;
                            if (sf == HAZ_REM_ROLL) // right-of-way
                                scratchPad[(int) player].SpeedLimit = false;
                        }
                        else {
                            scratchPad[(int) player].Mileage += miles;
                        }
                    }
                    // auto roll if played right-of-way safety and new safety or remedy
                    //  has cleared hazard
                    if ((scratchPad[(int) player].Safeties[MAX_SAFETIES - 1]) &&
                        (scratchPad[(int) player].BattleValue >= 0))
                        scratchPad[(int) player].BattleValue = HAZ_REM_ROLL;
                }
            }
        }
        #endregion

        // --------------------------------------------------------------------

        #region Public Methods
        /*
         * Method returns the specific card type of the milestone card. Values
         * will be -1 except for the card type. If hazard, hazard is set to 1 -
         * 5; if remedy, will be 1 - 5; if safety, will be 0 - 3; if mileage,
         * will return the miles on the card.
         */
        public void GetCardType(MilestoneCards card,
                                ref int hazard, ref int remedy, ref int safety, ref int mileage)
        {
            int cval = (int) card;

            hazard = -1; remedy = -1; safety = -1; mileage = -1;

            if ((cval >= HAZ_START) && (cval < REM_START)) hazard = cval - HAZ_START + 1;
            else if ((cval >= REM_START) && (cval < SAFE_START)) remedy = cval - REM_START + 1;
            else if ((cval >= SAFE_START) && (cval < MILE_START)) safety = cval - SAFE_START;
            else if (cval >= MILE_START) mileage = GetMileageValue(card);
        }

        /*
         * Method used to determine the card to play or discard from a given
         * players hand. Mainly used to determine the computer players move, but
         * is setup to also work as a hint routine if need be.
         * If determining computers hand, the processMilesAndScore flag should be
         * set to true, otherwise set it to false to not process the CompPlay
         * structure.
         */
        public CompPlay DeterminePlay(Players player, MilestoneHand hand, bool processMilesAndScore)
        {
            CompPlay play = new CompPlay();
            Players opponent = (player == Players.Computer) ? Players.Human : Players.Computer;
            bool coupFourre = false, safetyCard = false;
            int hz = scratchPad[(int) player].BattleValue, card = -1, safeCard = -1;

            play.PlayCard = true;
            play.OnOpponent = false;

            // check/play safety card
            safeCard = PlaySafety(player, hand, ref safetyCard, ref coupFourre);
            if (coupFourre) card = safeCard;
            if ((card == -1) && (safeCard != -1) &&
                ((hand.NumCardsInHand < MilestoneHand.CARDS_IN_MILESTONE_HAND) ||
                 (scratchPad[(int) player].Mileage > END_MILEAGE) ||
                 (scratchPad[(int) opponent].Mileage > OPPO_END_MILEAGE)))
                card = safeCard;
            // clear hazard, if have one
            if ((card == -1) && ((hz < 0) || (scratchPad[(int) player].SpeedLimit))) {
                card = PlayRemedy(player, hand);
            }
            // play roll or mileage
            if ((card == -1) && (hz >= 0)) {
                if (scratchPad[(int) player].BattleValue < HAZ_REM_ROLL) {
                    card = PlayRoll(hand);
                }
                else {
                    int lim = 201;
                    if (scratchPad[(int) player].SpeedLimit) lim = 51;
                    card = PlayMileage(player, hand, lim);
                }
            }
            // play hazard on player
            if (card == -1) {
                card = PlayHazard(opponent, hand);
                if (card != -1) play.OnOpponent = true;
            }
            // discard (no play?)
            if (card == -1) {
                play.PlayCard = false;
                card = PickDiscard(player, opponent, hand);
                if (card == -1) {
                    if (safetyCard) {
                        // play safety anyway (if have and not playing above)
                        play.PlayCard = true;
                        card = safeCard;
                    }
                    else
                        card = PickRandomDiscard(hand);
                }
            }

            play.CardInHand = card;
            play.CoupFourre = coupFourre;
            play.Safety = safetyCard;

            if (processMilesAndScore) ProcessMilesAndScore(play, player, hand);

            return play;
        }

        /*
         * Method used to pass back if the player's chosen card can be played
         * or not. Will pass back false if the card can't be played along with
         * the reason why not in 'msg'. If the card is valid and can be played,
         * returns true and sets the appropriate bool to true based on the card
         * played (onOppo if playing a hazard, safe if playing a safety, coup
         * if safety lead to a coup fourre).
         */
        public bool PlayerCanPlayCard(MilestoneCards card, ref bool onOppo, ref bool safe, ref bool coup, ref string msg)
        {
            int hz = -1, rm = -1, sf = -1, miles = -1;
            bool canPlay = true;
            ScratchPad cScratch = GetScratchPad(Players.Computer);
            ScratchPad pScratch = GetScratchPad(Players.Human);

            GetCardType(card, ref hz, ref rm, ref sf, ref miles);

            if (hz != -1) {
                if (hz == HAZ_REM_SPEED_LIMIT) {
                    if (cScratch.SpeedLimit) msg = MSG_ALREADY_SL;
                }
                else {
                    if (cScratch.BattleValue < 0)
                        msg = MSG_ALREADY_HAZ;
                    else if (cScratch.BattleValue != HAZ_REM_ROLL)
                        msg = MSG_NO_ROLL;
                }
                if (cScratch.Safeties[hz - 1]) msg = MSG_HAVE_SAFETY;
            }
            else if (rm != -1) {
                if (rm == HAZ_REM_SPEED_LIMIT) {
                    if (!pScratch.SpeedLimit) msg = MSG_NO_SL;
                }
                else  {
                    if (rm == HAZ_REM_ROLL) {
                        if ((pScratch.BattleValue < 0) && (pScratch.BattleValue != -rm))
                            msg = MSG_HAVE_HAZ;
                        else if (pScratch.BattleValue > HAZ_REM_SPEED_LIMIT)
                            msg = MSG_HAVE_ROLL;
                    }
                    else if (pScratch.BattleValue != -rm)
                        msg = MSG_NOT_THAT_HAZ;
                }
            }
            else if (sf != -1) {
                safe = true;
                if (sf == 3) sf = 4; // right-of-way card
                if ((pScratch.BattleValue == -(sf + 1)) || ((pScratch.SpeedLimit) && (sf == 4)))
                    coup = true;
            }
            else {
                if ((pScratch.SpeedLimit) && (miles > 50))
                    msg = MSG_HAVE_SL;
                else if ((miles + pScratch.Mileage) > ROUND_MILEAGE)
                    msg = String.Format(MSG_EXCEED_MILEAGE, Convert.ToString(ROUND_MILEAGE));
                else if (pScratch.BattleValue != HAZ_REM_ROLL)
                    msg = MSG_NEED_ROLL;
            }

            if (!"".Equals(msg)) canPlay = false;
            onOppo = ((canPlay) && (hz != -1));

            return canPlay;
        }

        /*
         * Method (in engine) to process the play of a player.  Needs the players hand,
         * which player, the card playing, if playing hazard on opponent, if it's a
         * safety card and if a Coup Fourre was scored.
         * This is typically used to process into the engine the human players move.
         * It should not be called if the player is discarding (will fail).
         */
        public void ProcessScoreAndMiles(MilestoneHand hand, Players player, int cardInHand,
            bool onOpponent, bool safety, bool coupFourre)
        {
            CompPlay play = new CompPlay();

            play.CardInHand = cardInHand;
            play.PlayCard = true;
            play.OnOpponent = onOpponent;
            play.Safety = safety;
            play.CoupFourre = coupFourre;

            ProcessMilesAndScore(play, player, hand);
        }

        /*
         * Method used to do the final score tally at rounds end. Score is kept within
         * the engine so need this to be done in the engine to finalize the scores and
         * keep them.
         */
        public void ScoreFinalTally()
        {
            int cCTBonus = 0, pCTBonus = 0, cSOBonus = 0, pSOBonus = 0;
            int plyr = (int) Players.Human, comp = (int) Players.Computer;
            int cMiles = scratchPad[comp].Mileage;
            int pMiles = scratchPad[plyr].Mileage;

            if (cMiles == ROUND_MILEAGE) cCTBonus = BONUS_COMPLETE_TRIP_SCORE;
            if (pMiles == ROUND_MILEAGE) pCTBonus = BONUS_COMPLETE_TRIP_SCORE;
            if ((pMiles == 0) && (cMiles > 0)) cSOBonus = BONUS_SHUTOUT_SCORE;
            if ((cMiles == 0) && (pMiles > 0)) pSOBonus = BONUS_SHUTOUT_SCORE;

            scores[comp].RoundTotal += scores[comp].CoupFourres + scores[comp].Safeties + cCTBonus + cSOBonus + cMiles;
            scores[plyr].RoundTotal += scores[plyr].CoupFourres + scores[plyr].Safeties + pCTBonus + pSOBonus + pMiles;

            scores[comp].GrandTotal += scores[comp].RoundTotal;
            scores[plyr].GrandTotal += scores[plyr].RoundTotal;
        }

        /*
         * Method returns the Scores structure for a given player.
         */
        public Scores GetScore(Players player)
        {
            return scores[(int) player];
        }

        /*
         * Method returns the engines scratch pad structure for a
         * given player.
         */
        public ScratchPad GetScratchPad(Players player)
        {
            return scratchPad[(int) player];
        }

        /*
         * Convenience method to return the mileage gain for a
         * given player in the current round.  Could be gotten
         * from the engine scratch pad for the given player
         * also.
         */
        public int GetMileage(Players player)
        {
            return scratchPad[(int) player].Mileage;
        }

        /*
         * Method used to reset the round scores within the Scores
         * structures.  Used at the start of each round to reset
         * the game.  If starting new game, the reset grand total
         * flag needs to be set to true.
         */
        public void ResetScores(bool resetGrandTotal)
        {
            for (int i = 0; i < MAX_PLAYERS; i++) ResetScore(i, resetGrandTotal);
        }

        /*
         * Method used to reset the scratchpad structures used by the
         * engine internally.  Used at the start of each round to reset
         * the game.
         */
        public void ResetScratchPads()
        {
            for (int i = 0; i < MAX_PLAYERS; i++) ResetScratchPad(i);
        }
        #endregion
    }
}

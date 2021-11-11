using System;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Media;
using GameStatistics;

/*
 * Primary class defines the partial class of the main window for the
 * game of Milestone (mille bornes).
 * This game is loosely based on a game written in basic by David Addison in
 * 1986. It was converted to Turbo Pascal, then to Speed Pascal (OS/2),
 * finally to C#. The engine was significantly rewritten with the C# port
 * to finally remove all of the GOTOs from the pick card to play routine.
 *
 * Author:  M. G. Slack
 * Written: 2014-02-11
 * Version: 1.0.6.0
 *
 * ----------------------------------------------------------------------------
 * 
 * Updated: 2014-03-04 - Updated several of the milestone card images. Cleaned
 *                       up some of the code/comments. Added a 'tada' sound
 *                       when playing a coup fourre.
 *          2014-03-06 - Minor tweaks to game flow.
 *          2014-03-20 - Added on GameStatistics library (assembly) to track
 *                       statistics such as games won/lost, highest winning
 *                       score (comp/player) along with highest score in a
 *                       round (comp/player).
 *          2014-06-01 - Fixed a bug in the MilestoneEngine.
 *          2021-03-08 - Removed unnecessary using's.
 *          2021-11-05 - Changed the couple of custom SoundPlayer's to use
 *                       .PlaySync() instead of .Play().
 */
namespace Milestone
{
    public partial class MainWin : Form
    {
        #region String constants
        private const string HTML_HELP_FILE = "Milestone_help.html";
        private const string SOUND_PATH = "Milestone.sounds.";
        // above consts should not be translated
        // custom statistic names
        private const string CSTAT_COMP_HIGH = "Computer High Score";
        private const string CSTAT_PLAYER_ROUND_HIGH = "Player Highest Round Total";
        private const string CSTAT_COMP_ROUND_HIGH = "Computer Highest Round Total";
        private const string CSTAT_MOST_ROUNDS = "Most Rounds Played in Game";
        private const string CSTAT_LEAST_ROUNDS = "Least Rounds Played in Game";
        // button/menu labels
        private const string NEW_GAME = "&New Game";
        private const string NEW_ROUND = "&New Round";
        private const string GAME_STATS = "Game Statistics";
        private const string ABOUT = "About";
        // various text strings
        private const string LS_ERR_TITLE = "Load Sound Error";
        private const string START_GAME = "Start new game when ready.";
        private const string START_ROUND = "Start new round when ready.";
        private const string HUM_PLAY = "Your move, play or discard.";
        private const string COMP_PLAY = "Computers move, please wait...";
        private const string COUP_FOURRE = "** Coup Fourre! **";
        private const string GAME_NOT_OVER = "Game is not over yet, quit anyway?";
        private const string ROUND_NOT_OVER = "Current round not completed, start new?";
        private const string PLAYER_STARTS = "Player will start.";
        private const string COMP_STARTS = "Computer will start.";
        private const string COMP_PLAY_MSG = "Computer will {0} a {1} card.";
        private const string COMP_P_MOVE = "play";
        private const string COMP_D_MOVE = "discard";
        #endregion

        #region Registry constants
        // registry name/keys
        private const string REG_NAME = @"HKEY_CURRENT_USER\Software\Slack and Associates\Games\Milestone";
        private const string REG_KEY1 = "PosX";
        private const string REG_KEY2 = "PosY";
        private const string REG_KEY3 = "GameWinPoints";
        private const string REG_KEY4 = "IncidentalSoundsOn";
        private const string REG_KEY5 = "PlayerAlwaysStarts";
        #endregion

        #region Milestone Window Class fields
        private MilestoneDeck deck = new MilestoneDeck();
        private MilestoneEngine engine = new MilestoneEngine();
        private MilestoneCardImage images = new MilestoneCardImage();
        private MilestoneHand[] hands = new MilestoneHand[MilestoneEngine.MAX_PLAYERS];

        private int RoundCounter = 0, GameWinPoints = MilestoneEngine.DEF_GAME_WIN_POINTS;
        private bool gameStarted = false, roundOver = true, soundsOn = true, playerAlwaysStarts = false;
        private PictureBox[] playerCards = new PictureBox[MilestoneHand.CARDS_IN_MILESTONE_HAND];
        private Button[] playBtns = new Button[MilestoneHand.CARDS_IN_MILESTONE_HAND];
        private Button[] discardBtns = new Button[MilestoneHand.CARDS_IN_MILESTONE_HAND];
        private PictureBox[] cSafeCards = new PictureBox[MilestoneEngine.MAX_SAFETIES - 1];
        private PictureBox[] pSafeCards = new PictureBox[MilestoneEngine.MAX_SAFETIES - 1];
        private bool _mileageSoundLoaded = false;
        private SoundPlayer mileageSound = null;
        private bool _tadaSoundLoaded = false;
        private SoundPlayer tadaSound = null;
        private Statistics stats = new Statistics(REG_NAME);

        // custom events
        private event EventHandler CompsPlay;
        private event EventHandler HumsPlay;
        #endregion

        // --------------------------------------------------------------------

        #region Private Methods
        private void DoEvent(EventHandler handler)
        {
            if (handler != null) { handler(this, EventArgs.Empty); }
        }

        private void LoadRegistryValues()
        {
            int winX = -1, winY = -1;
            string tempBool = "";

            try {
                winX = (int) Registry.GetValue(REG_NAME, REG_KEY1, winX);
                winY = (int) Registry.GetValue(REG_NAME, REG_KEY2, winY);
                GameWinPoints = (int) Registry.GetValue(REG_NAME, REG_KEY3, GameWinPoints);
                tempBool = (string) Registry.GetValue(REG_NAME, REG_KEY4, "True");
                if (tempBool != null) soundsOn = Convert.ToBoolean(tempBool);
                tempBool = (string) Registry.GetValue(REG_NAME, REG_KEY5, "False");
                if (tempBool != null) playerAlwaysStarts = Convert.ToBoolean(tempBool);
            }
            catch (Exception ex) { /* ignore, go with defaults */ }

            if ((winX != -1) && (winY != -1)) this.SetDesktopLocation(winX, winY);
        }

        private void WriteRegistryValues()
        {
            Registry.SetValue(REG_NAME, REG_KEY3, GameWinPoints);
            Registry.SetValue(REG_NAME, REG_KEY4, soundsOn);
            Registry.SetValue(REG_NAME, REG_KEY5, playerAlwaysStarts);
        }

        private void SetupContextMenu()
        {
            ContextMenu mnu = new ContextMenu();
            MenuItem mnuStats = new MenuItem(GAME_STATS);
            MenuItem sep = new MenuItem("-");
            MenuItem mnuAbout = new MenuItem(ABOUT);

            mnuStats.Click += new EventHandler(mnuStats_Click);
            mnuAbout.Click += new EventHandler(mnuAbout_Click);
            mnu.MenuItems.AddRange(new MenuItem[] { mnuStats, sep, mnuAbout });
            this.ContextMenu = mnu;
        }

        private void InitAndSetupComponents()
        {
            playerCards[0] = pbCard0; playerCards[1] = pbCard1; playerCards[2] = pbCard2;
            playerCards[3] = pbCard3; playerCards[4] = pbCard4; playerCards[5] = pbCard5;
            playerCards[6] = pbCard6;
            playBtns[0] = PlayBtn0; playBtns[1] = PlayBtn1; playBtns[2] = PlayBtn2;
            playBtns[3] = PlayBtn3; playBtns[4] = PlayBtn4; playBtns[5] = PlayBtn5;
            playBtns[6] = PlayBtn6;
            discardBtns[0] = DiscardBtn0; discardBtns[1] = DiscardBtn1; discardBtns[2] = DiscardBtn2;
            discardBtns[3] = DiscardBtn3; discardBtns[4] = DiscardBtn4; discardBtns[5] = DiscardBtn5;
            discardBtns[6] = DiscardBtn6;
            cSafeCards[0] = cExTank; cSafeCards[1] = cPermTire; cSafeCards[2] = cDriveAce; cSafeCards[3] = cRightWay;
            pSafeCards[0] = pExTank; pSafeCards[1] = pPermTire; pSafeCards[2] = pDriveAce; pSafeCards[3] = pRightWay;
        }

        private void InitAndSetupEvents()
        {
            CompsPlay += customCompsPlay;
            HumsPlay += customHumsPlay;
        }

        private void LoadMileageSound()
        {
            string path = SOUND_PATH + "mileage.wav";

            try {
                mileageSound = new SoundPlayer(MilestoneCardImage.GetResourceStream(path));
                _mileageSoundLoaded = true;
            }
            catch (Exception e) {
                MessageBox.Show("Wav (" + path + "): " + e.Message, LS_ERR_TITLE);
                mileageSound = null;
                _mileageSoundLoaded = false;
            }
        }

        private void LoadTadaSound()
        {
            string path = SOUND_PATH + "tada.wav";

            try {
                tadaSound = new SoundPlayer(MilestoneCardImage.GetResourceStream(path));
                _tadaSoundLoaded = true;
            }
            catch (Exception e) {
                MessageBox.Show("Wav (" + path + "): " + e.Message, LS_ERR_TITLE);
                tadaSound = null;
                _tadaSoundLoaded = false;
            }
        }

        private void PlayMileageSound()
        {
            if (_mileageSoundLoaded) mileageSound.PlaySync();
        }

        private void PlayTadaSound()
        {
            if (_tadaSoundLoaded) tadaSound.PlaySync();
        }

        private void EnDisButtons(bool enable)
        {
            int maxCards = (enable) ? hands[(int) Players.Human].NumCardsInHand : MilestoneHand.CARDS_IN_MILESTONE_HAND;
            for (int i = 0; i < maxCards; i++) {
                playBtns[i].Enabled = enable;
                discardBtns[i].Enabled = enable;
            }
        }

        private void ResetGame(bool gameOver)
        {
            for (int i = 0; i < MilestoneEngine.MAX_PLAYERS; i++)
                hands[i].RemoveAll();
            EnDisButtons(false);
            if (gameOver) {
                RoundCounter = 0;
                NewBtn.Text = NEW_GAME;
                MsgLbl.Text = START_GAME;
            }
            else {
                NewBtn.Text = NEW_ROUND;
                MsgLbl.Text = START_ROUND;
            }
        }

        private void ResetPBsToCardBacks()
        {
            for (int i = 0; i < MilestoneHand.CARDS_IN_MILESTONE_HAND; i++)
                playerCards[i].Image = images.GetCardBackImage(MilestoneCardBacks.Players);

            pbDiscard.Image = images.GetCardBackImage(MilestoneCardBacks.Normal);
            pSpeed.Image = images.GetCardBackImage(MilestoneCardBacks.Normal);
            pBattle.Image = images.GetCardBackImage(MilestoneCardBacks.Normal);
            pExTank.Image = images.GetCardBackImage(MilestoneCardBacks.Normal);
            pPermTire.Image = images.GetCardBackImage(MilestoneCardBacks.Normal);
            pDriveAce.Image = images.GetCardBackImage(MilestoneCardBacks.Normal);
            pRightWay.Image = images.GetCardBackImage(MilestoneCardBacks.Normal);

            cSpeed.Image = images.GetCardBackImage(MilestoneCardBacks.Normal);
            cBattle.Image = images.GetCardBackImage(MilestoneCardBacks.Normal);
            cExTank.Image = images.GetCardBackImage(MilestoneCardBacks.Normal);
            cPermTire.Image = images.GetCardBackImage(MilestoneCardBacks.Normal);
            cDriveAce.Image = images.GetCardBackImage(MilestoneCardBacks.Normal);
            cRightWay.Image = images.GetCardBackImage(MilestoneCardBacks.Normal);
        }

        private void ShowDistance()
        {
            pDistance.Text = Convert.ToString(engine.GetScratchPad(Players.Human).Mileage);
            cDistance.Text = Convert.ToString(engine.GetScratchPad(Players.Computer).Mileage);
        }

        private void ShowCardsLeft()
        {
            lblCardsLeft.Text = Convert.ToString(deck.CardsLeft);
        }

        private void ShowPlayersCards()
        {
            for (int i = 0; i < MilestoneHand.CARDS_IN_MILESTONE_HAND; i++) {
                MilestoneCards card = hands[(int) Players.Human].CardAt(i);
                if (card == MilestoneCards.Empty_Card) {
                    playerCards[i].Image = images.GetCardBackImage(MilestoneCardBacks.Players);
                    playBtns[i].Enabled = false;
                    discardBtns[i].Enabled = false;
                }
                else
                    playerCards[i].Image = images.GetCardImage(card);
            }
        }

        private void StartRound()
        {
            ResetPBsToCardBacks();
            engine.ResetScores(!gameStarted);
            engine.ResetScratchPads();
            ShowDistance();
            RoundCounter++; gameStarted = true; roundOver = false;
            deck.Shuffle();
            // deal cards
            for (int i = 0; i < MilestoneHand.CARDS_IN_MILESTONE_HAND; i++)
                for (int j = 0; j < MilestoneEngine.MAX_PLAYERS; j++)
                    hands[j].Add(deck.GetNextCard());
            ShowCardsLeft();
            ShowPlayersCards();
        }

        private void ShowScore()
        {
            ScoreDlg dlg = new ScoreDlg();

            dlg.Engine = engine;
            dlg.Round = RoundCounter;
            dlg.GameWinPoints = GameWinPoints;
            if (soundsOn) SystemSounds.Asterisk.Play();
            dlg.ShowDialog(this);
            dlg.Dispose();
        }

        private void GetNextCard(MilestoneHand hand)
        {
            if (deck.HasMoreCards()) {
                hand.Add(deck.GetNextCard());
                ShowCardsLeft();
            }
            else
                hand.CompressHand();
        }

        private void ProcessEndStats()
        {
            MilestoneEngine.Scores cScore = engine.GetScore(Players.Computer);
            MilestoneEngine.Scores pScore = engine.GetScore(Players.Human);

            if ((cScore.GrandTotal >= GameWinPoints) || (pScore.GrandTotal >= GameWinPoints))
                gameStarted = false;

            if (stats.CustomStatistic(CSTAT_PLAYER_ROUND_HIGH) < pScore.RoundTotal)
                stats.SetCustomStatistic(CSTAT_PLAYER_ROUND_HIGH, pScore.RoundTotal);
            if (stats.CustomStatistic(CSTAT_COMP_ROUND_HIGH) < cScore.RoundTotal)
                stats.SetCustomStatistic(CSTAT_COMP_ROUND_HIGH, cScore.RoundTotal);

            if (!gameStarted) {
                if (stats.CustomStatistic(CSTAT_MOST_ROUNDS) < RoundCounter)
                    stats.SetCustomStatistic(CSTAT_MOST_ROUNDS, RoundCounter);
                if ((stats.CustomStatistic(CSTAT_LEAST_ROUNDS) == 0) ||
                    (stats.CustomStatistic(CSTAT_LEAST_ROUNDS) > RoundCounter))
                    stats.SetCustomStatistic(CSTAT_LEAST_ROUNDS, RoundCounter);
                if (cScore.GrandTotal > pScore.GrandTotal) { // computer won
                    if (stats.CustomStatistic(CSTAT_COMP_HIGH) < cScore.GrandTotal)
                        stats.SetCustomStatistic(CSTAT_COMP_HIGH, cScore.GrandTotal);
                    stats.GameLost(0);
                }
                else // player won
                    stats.GameWon(pScore.GrandTotal);
            }
        }

        private bool RoundOrGameOver()
        {
            MilestoneEngine.ScratchPad cScratch = engine.GetScratchPad(Players.Computer);
            MilestoneEngine.ScratchPad pScratch = engine.GetScratchPad(Players.Human);

            roundOver = (((!deck.HasMoreCards()) &&
                          (hands[(int) Players.Computer].NumCardsInHand == 0) &&
                          (hands[(int) Players.Human].NumCardsInHand == 0)) ||
                         (cScratch.Mileage == MilestoneEngine.ROUND_MILEAGE) ||
                         (pScratch.Mileage == MilestoneEngine.ROUND_MILEAGE));

            if (roundOver) {
                ShowPlayersCards();
                engine.ScoreFinalTally();
                ShowScore();
                ProcessEndStats();
                ResetGame(!gameStarted);
            }

            return roundOver;
        }

        private void DisplayRightOfWayRoll(Players player, PictureBox spd, PictureBox btl)
        {
            MilestoneEngine.ScratchPad pad = engine.GetScratchPad(player);

            if (pad.Safeties[MilestoneEngine.HAZ_REM_ROLL - 1]) {
                // have played right-of-way card
                spd.Image = images.GetCardImage(MilestoneCards.End_Speed_Limit);
                if ((pad.BattleValue >= 0) || (pad.BattleValue == -MilestoneEngine.HAZ_REM_ROLL))
                    btl.Image = images.GetCardImage(MilestoneCards.Roll);
            }
        }

        private void DisplayCardPlayed(Players player, MilestoneCards card, MilestoneEngine.ScratchPad pad,
            PictureBox speed, PictureBox battle, PictureBox oSpeed, PictureBox oBattle, PictureBox[] safeties)
        {
            int hz = -1, rm = -1, sf = -1, miles = -1;

            engine.GetCardType(card, ref hz, ref rm, ref sf, ref miles);

            if (hz != -1) {
                if (hz == MilestoneEngine.HAZ_REM_SPEED_LIMIT)
                    oSpeed.Image = images.GetCardImage(card);
                else
                    oBattle.Image = images.GetCardImage(card);
            }
            else if (rm != -1) {
                if (rm == MilestoneEngine.HAZ_REM_SPEED_LIMIT)
                    speed.Image = images.GetCardImage(card);
                else
                    battle.Image = images.GetCardImage(card);
            }
            else if (sf != -1) {
                safeties[sf].Image = images.GetCardImage(card);
                switch (sf) {
                    case 0: if (pad.BattleValue == MilestoneEngine.HAZ_REM_GAS)
                                battle.Image = images.GetCardImage(MilestoneCards.Gas);
                            break;
                    case 1: if (pad.BattleValue == MilestoneEngine.HAZ_REM_TIRE)
                                battle.Image = images.GetCardImage(MilestoneCards.Spare_Tire);
                            break;
                    case 2: if (pad.BattleValue == MilestoneEngine.HAZ_REM_REPAIR)
                                battle.Image = images.GetCardImage(MilestoneCards.Repairs);
                            break;
                    default: break;
                }
            }
            else {
                if (soundsOn) PlayMileageSound();
                ShowDistance();
            }

            DisplayRightOfWayRoll(player, speed, battle);
        }

        private bool PlayerCanPlayCard(MilestoneCards card, ref bool onOppo, ref bool safe, ref bool coup)
        {
            string msg = "";
            bool canPlay = engine.PlayerCanPlayCard(card, ref onOppo, ref safe, ref coup, ref msg);

            if (!canPlay) {
                if (soundsOn) SystemSounds.Asterisk.Play();
                MessageBox.Show(msg, this.Text, MessageBoxButtons.OK);
            }

            return canPlay;
        }

        private void FinishPlayerTurn()
        {
            if (!RoundOrGameOver()) {
                GetNextCard(hands[(int) Players.Human]);
                ShowPlayersCards();
                DoEvent(CompsPlay);
            }
        }
        #endregion

        // --------------------------------------------------------------------

        public MainWin()
        {
            InitializeComponent();
        }

        #region Event Handlers
        private void MainWin_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < MilestoneEngine.MAX_PLAYERS; i++)
                hands[i] = new MilestoneHand();
            LoadRegistryValues();
            SetupContextMenu();
            InitAndSetupComponents();
            InitAndSetupEvents();
            ResetPBsToCardBacks();
            LoadMileageSound();
            LoadTadaSound();
            ResetGame(true);
            stats.GameName = this.Text;
        }

        private void MainWin_FormClosing(object sender, FormClosingEventArgs e)
        {
            MilestoneEngine.Scores compScore, humScore;
            DialogResult ret = DialogResult.Yes;

            if (gameStarted) {
                compScore = engine.GetScore(Players.Computer);
                humScore = engine.GetScore(Players.Human);
                if ((compScore.GrandTotal < GameWinPoints) && (humScore.GrandTotal < GameWinPoints)) {
                    MessageBoxIcon icon = (soundsOn) ? MessageBoxIcon.Question : MessageBoxIcon.None;
                    ret = MessageBox.Show(GAME_NOT_OVER, this.Text, MessageBoxButtons.YesNo, icon);
                }
            }
            e.Cancel = (ret != DialogResult.Yes);
        }

        private void MainWin_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (mileageSound != null) mileageSound.Dispose();
            if (tadaSound != null) tadaSound.Dispose();
            if (this.WindowState == FormWindowState.Normal) {
                Registry.SetValue(REG_NAME, REG_KEY1, this.Location.X);
                Registry.SetValue(REG_NAME, REG_KEY2, this.Location.Y);
            }
        }

        private void NewBtn_Click(object sender, EventArgs e)
        {
            DialogResult ret = DialogResult.Yes;

            if ((gameStarted) && (!roundOver)) {
                MessageBoxIcon icon = (soundsOn) ? MessageBoxIcon.Question : MessageBoxIcon.None;
                ret = MessageBox.Show(ROUND_NOT_OVER, this.Text, MessageBoxButtons.YesNo, icon);
                if (ret == DialogResult.Yes) {
                    RoundCounter--;
                    for (int i = 0; i < MilestoneEngine.MAX_PLAYERS; i++)
                        hands[i].RemoveAll();
                }
            }
            if (ret == DialogResult.Yes) {
                if (!gameStarted) stats.StartGame(true);
                StartRound();
                if ((playerAlwaysStarts) || (SingleRandom.Instance.Next(100) >= 50)) {
                    MessageBox.Show(PLAYER_STARTS, this.Text, MessageBoxButtons.OK);
                    DoEvent(HumsPlay);
                }
                else {
                    MessageBox.Show(COMP_STARTS, this.Text, MessageBoxButtons.OK);
                    DoEvent(CompsPlay);
                }
            }
        }

        private void ExitBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OptionsBtn_Click(object sender, EventArgs e)
        {
            OptionsDlg opts = new OptionsDlg();

            opts.GameWinPoints = GameWinPoints;
            opts.SoundsOn = soundsOn;
            opts.PlayerAlwaysStarts = playerAlwaysStarts;

            if (opts.ShowDialog(this) == DialogResult.OK) {
                GameWinPoints = opts.GameWinPoints;
                soundsOn = opts.SoundsOn;
                playerAlwaysStarts = opts.PlayerAlwaysStarts;
                WriteRegistryValues();
            }

            opts.Dispose();
        }

        private void HelpBtn_Click(object sender, EventArgs e)
        {
            var asm = Assembly.GetEntryAssembly();
            var asmLocation = Path.GetDirectoryName(asm.Location);
            var htmlPath = Path.Combine(asmLocation, HTML_HELP_FILE);

            try {
                Process.Start(htmlPath);
            }
            catch (Exception ex) {
                MessageBox.Show("Cannot load help: " + ex.Message, "Help Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PlayBtnX_Click(object sender, EventArgs e)
        {
            int slot = Convert.ToInt32((sender as Button).Tag);
            MilestoneCards card = hands[(int) Players.Human].CardAt(slot); // don't remove yet!
            bool onOpponent = false, safety = false, coupFourre = false;

            if (PlayerCanPlayCard(card, ref onOpponent, ref safety, ref coupFourre)) {
                engine.ProcessScoreAndMiles(hands[(int) Players.Human], Players.Human, slot,
                                            onOpponent, safety, coupFourre);
                DisplayCardPlayed(Players.Human, hands[(int) Players.Human].Remove(slot),
                    engine.GetScratchPad(Players.Human), pSpeed, pBattle, cSpeed, cBattle, pSafeCards);
                if (coupFourre) {
                    ShowPlayersCards(); // undisplay safety played from players hand before dialog shown
                    if (soundsOn) PlayTadaSound();
                    MessageBox.Show(COUP_FOURRE, this.Text, MessageBoxButtons.OK);
                }
                FinishPlayerTurn();
            }
        }

        private void DiscardBtnX_Click(object sender, EventArgs e)
        {
            int slot = Convert.ToInt32((sender as Button).Tag);
            MilestoneCards card = hands[(int) Players.Human].Remove(slot);

            pbDiscard.Image = images.GetCardImage(card);
            FinishPlayerTurn();
        }

        private void mnuStats_Click(object sender, EventArgs e)
        {
            stats.ShowStatistics(this);
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();

            about.ShowDialog(this);
            about.Dispose();
        }

        private void customCompsPlay(object sender, EventArgs e)
        {
            EnDisButtons(false);
            MsgLbl.Text = COMP_PLAY;

            MilestoneEngine.CompPlay play = engine.DeterminePlay(Players.Computer, hands[(int) Players.Computer], true);
            int cardSlot = play.CardInHand;
            MilestoneCards card = hands[(int) Players.Computer].Remove(cardSlot);
            string msgFmt = COMP_PLAY_MSG, move = COMP_P_MOVE, msg = "";

            if (play.PlayCard) {
                DisplayCardPlayed(Players.Computer, card, engine.GetScratchPad(Players.Computer), cSpeed, cBattle,
                    pSpeed, pBattle, cSafeCards);
            }
            else {
                move = COMP_D_MOVE;
                pbDiscard.Image = images.GetCardImage(card);
            }

            msg = String.Format(msgFmt, move, card.AsText());
            if (play.CoupFourre) {
                msg += Environment.NewLine + COUP_FOURRE;
                if (soundsOn) PlayTadaSound();
            }
            MessageBox.Show(msg, this.Text, MessageBoxButtons.OK);

            if (!RoundOrGameOver()) {
                GetNextCard(hands[(int) Players.Computer]);
                DoEvent(HumsPlay);
            }
        }

        private void customHumsPlay(object sender, EventArgs e)
        {
            MsgLbl.Text = HUM_PLAY;
            EnDisButtons(true);
        }
        #endregion
    }
}

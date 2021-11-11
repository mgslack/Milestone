using System;
using System.Drawing;
using System.Windows.Forms;

/*
 * Primary class defines the score dialog display used for the game of Milestone
 * (mille bornes). Will use the score and scratch pad structures defined in the
 * milestone engine. Note, for tie scores over the set game win points, the
 * player wins.
 *
 * Author:  M. G. Slack
 * Written: 2014-02-14
 *
 * ----------------------------------------------------------------------------
 * 
 * Updated: 2014-03-05 - Added a line label to separate round totals.
 *          2014-03-06 - Fixed shut out score determination when both players
 *                       were shut out.
 *
 */
namespace Milestone
{
    public partial class ScoreDlg : Form
    {
        private const string GO_TITLE = "Game Over";

        #region Properties
        private MilestoneEngine _engine = null;
        public MilestoneEngine Engine { set { _engine = value; } }

        private int _round = 0;
        public int Round { set { _round = value; } }

        private int _gameWinPoints = MilestoneEngine.DEF_GAME_WIN_POINTS;
        public int GameWinPoints { set { _gameWinPoints = value; } }
        #endregion

        // --------------------------------------------------------------------

        public ScoreDlg()
        {
            InitializeComponent();
        }

        // --------------------------------------------------------------------

        #region Private Methods
        private void LoadInCompScores(MilestoneEngine.ScratchPad cPad, MilestoneEngine.ScratchPad pPad,
                                      MilestoneEngine.Scores cScore)
        {
            cDistance.Text = Convert.ToString(cPad.Mileage);
            if (cPad.Mileage == MilestoneEngine.ROUND_MILEAGE)
                cTrip.Text = Convert.ToString(MilestoneEngine.BONUS_COMPLETE_TRIP_SCORE);
            cCoup.Text = Convert.ToString(cScore.CoupFourres);
            cSafe.Text = Convert.ToString(cScore.Safeties);
            if ((pPad.Mileage == 0) && (cPad.Mileage > 0))
                cShutOut.Text = Convert.ToString(MilestoneEngine.BONUS_SHUTOUT_SCORE);
            cRoundTot.Text = Convert.ToString(cScore.RoundTotal);
            cGrandTot.Text = Convert.ToString(cScore.GrandTotal);
        }

        private void LoadInPlyrScores(MilestoneEngine.ScratchPad pPad, MilestoneEngine.ScratchPad cPad,
                                      MilestoneEngine.Scores pScore)
        {
            pDistance.Text = Convert.ToString(pPad.Mileage);
            if (pPad.Mileage == MilestoneEngine.ROUND_MILEAGE)
                pTrip.Text = Convert.ToString(MilestoneEngine.BONUS_COMPLETE_TRIP_SCORE);
            pCoup.Text = Convert.ToString(pScore.CoupFourres);
            pSafe.Text = Convert.ToString(pScore.Safeties);
            if ((cPad.Mileage == 0) && (pPad.Mileage > 0))
                pShutOut.Text = Convert.ToString(MilestoneEngine.BONUS_SHUTOUT_SCORE);
            pRoundTot.Text = Convert.ToString(pScore.RoundTotal);
            pGrandTot.Text = Convert.ToString(pScore.GrandTotal);
        }

        private void ChangeLabel(Label winningGrandTotal)
        {
            winningGrandTotal.BackColor = Color.Green;
            winningGrandTotal.ForeColor = Color.White;
        }
        #endregion

        // --------------------------------------------------------------------

        #region Event Handlers
        private void ScoreDlg_Load(object sender, EventArgs e)
        {
            if (_engine != null) {
                MilestoneEngine.ScratchPad compScratch = _engine.GetScratchPad(Players.Computer);
                MilestoneEngine.ScratchPad plyrScratch = _engine.GetScratchPad(Players.Human);
                MilestoneEngine.Scores compScore = _engine.GetScore(Players.Computer);
                MilestoneEngine.Scores plyrScore = _engine.GetScore(Players.Human);
                LoadInCompScores(compScratch, plyrScratch, compScore);
                LoadInPlyrScores(plyrScratch, compScratch, plyrScore);
                if ((compScore.GrandTotal >= _gameWinPoints) ||
                    (plyrScore.GrandTotal >= _gameWinPoints)) {
                    this.Text = GO_TITLE;
                    if (compScore.GrandTotal > plyrScore.GrandTotal)
                        ChangeLabel(cGrandTot);
                    else
                        ChangeLabel(pGrandTot);
                }
            }
            RoundLbl.Text = String.Format(RoundLbl.Text, _round);
        }
        #endregion
    }
}

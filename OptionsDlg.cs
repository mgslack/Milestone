using System;
using System.Windows.Forms;

/*
 * Primary class defines the options dialog used for the game of Milestone
 * (mille bornes). Allows several game settings to be changed by the
 * player.
 *
 * Author:  M. G. Slack
 * Written: 2014-02-18
 *
 * ----------------------------------------------------------------------------
 * 
 * Updated: 2021-03-08 - Removed unnecessary using's, added regions.
 *
 */
namespace Milestone
{
    public partial class OptionsDlg : Form
    {
        public const int MAX_POINTS = 50000;

        #region Properties
        private int _gameWinPoints = MilestoneEngine.DEF_GAME_WIN_POINTS;
        public int GameWinPoints { get { return _gameWinPoints; }
                                   set { if (value <= MAX_POINTS) _gameWinPoints = value; } }

        private bool _soundsOn = true;
        public bool SoundsOn { get { return _soundsOn; } set { _soundsOn = value; } }

        private bool _alwaysStarts = false;
        public bool PlayerAlwaysStarts { get { return _alwaysStarts; } set { _alwaysStarts = value; } }
        #endregion

        // --------------------------------------------------------------------

        public OptionsDlg()
        {
            InitializeComponent();
        }

        // --------------------------------------------------------------------

        #region Event Handlers
        private void OptionsDlg_Load(object sender, EventArgs e)
        {
            udPoints.Value = _gameWinPoints;
            cbSounds.Checked = _soundsOn;
            cbAlwaysStart.Checked = _alwaysStarts;
        }

        private void udPoints_ValueChanged(object sender, EventArgs e)
        {
            _gameWinPoints = (int) udPoints.Value;
        }

        private void cbSounds_CheckedChanged(object sender, EventArgs e)
        {
            _soundsOn = cbSounds.Checked;
        }

        private void cbAlwaysStart_CheckedChanged(object sender, EventArgs e)
        {
            _alwaysStarts = cbAlwaysStart.Checked;
        }
        #endregion
    }
}

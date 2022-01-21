using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

/*
 * Primary class loads and caches the milestone card images embedded within the
 * assembly.  An enum is available to load up the two card back images available.
 *
 * Author:  M. G. Slack
 * Written: 2014-02-12
 *
 * ----------------------------------------------------------------------------
 * 
 * Updated: 2022-01-21 - Removed unnecessary usings and added regions to code.                     
 *
 */
namespace Milestone
{
    public enum MilestoneCardBacks { Players = 20, Normal };

    public class MilestoneCardImage
    {
        #region Constants
        /* Width of card images (in pixels). */
        public const int IMAGE_WIDTH = 60;
        /* Height of card images (in pixels). */
        public const int IMAGE_HEIGHT = 90;

        private const string IMAGE_NAMESPACE = "Milestone.images.";
        private const string IMAGE_EXT = ".bmp";
        #endregion

        #region Private vars
        private Dictionary<string, Bitmap> imageCache = new Dictionary<string, Bitmap>();
        #endregion

        // --------------------------------------------------------------------

        /*
         * Default constructor.
         */
        public MilestoneCardImage() { }

        // --------------------------------------------------------------------

        #region Private methods
        private Bitmap LoadImage(int imageNum)
        {
            string imageName = Convert.ToString(imageNum) + IMAGE_EXT;
            string path = IMAGE_NAMESPACE + imageName;
            Bitmap bitmap = null;

            if (imageCache.ContainsKey(path)) { return imageCache[path]; }

            try {
                bitmap = new Bitmap(GetResourceStream(path));
                if (bitmap != null) { imageCache.Add(path, bitmap); }
            }
            catch (Exception e) {
                MessageBox.Show("Image (" + imageName + "): " + e.Message, "LoadImage Error");
            }

            return bitmap;
        }
        #endregion

        // --------------------------------------------------------------------

        #region Public methods
        /*
         * Method used to get a manifest resource stream of the selected
         * resource represented by the resource path.
         */
        public static Stream GetResourceStream(string path)
        {
            Assembly asm = Assembly.GetExecutingAssembly();

            return asm.GetManifestResourceStream(path);
        }

        // --------------------------------------------------------------------

        /*
         * Method returns a bitmap image of the Milestone card passed in.  Will
         * return null if the card is not a valid Milestone card.
         */
        public Bitmap GetCardImage(MilestoneCards card)
        {
            return LoadImage((int) card);
        }

        /*
         * Method returns one of the available card back images.  May
         * return null if image can't be loaded or is invalid.
         */
        public Bitmap GetCardBackImage(MilestoneCardBacks back)
        {
            return LoadImage((int) back);
        }
        #endregion
    }
}

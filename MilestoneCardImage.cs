using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
 * Updated: yyyy-mm-dd - xxxx.                     
 *
 */
namespace Milestone
{
    public enum MilestoneCardBacks { Players = 20, Normal };

    public class MilestoneCardImage
    {
        /* Width of card images (in pixels). */
        public const int IMAGE_WIDTH = 60;
        /* Height of card images (in pixels). */
        public const int IMAGE_HEIGHT = 90;

        private const string IMAGE_NAMESPACE = "Milestone.images.";
        private const string IMAGE_EXT = ".bmp";

        private Dictionary<string, Bitmap> imageCache = new Dictionary<string, Bitmap>();

        // --------------------------------------------------------------------

        /*
         * Default constructor.
         */
        public MilestoneCardImage() { }

        // --------------------------------------------------------------------

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

        // --------------------------------------------------------------------

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
    }
}

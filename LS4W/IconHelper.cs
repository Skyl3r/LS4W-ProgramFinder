using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LS4W
{
    public static class IconHelper
    {
        public static Icon getLargestInList(List<Icon> icons)
        {
            if (icons.Count == 0)
                throw new Exception("Empty array");

            Icon icon = icons[0];
            foreach(Icon i in icons)
            {
                if (i.Width > icon.Width)
                    icon = i;
            }

            return icon;
        }
    }
}

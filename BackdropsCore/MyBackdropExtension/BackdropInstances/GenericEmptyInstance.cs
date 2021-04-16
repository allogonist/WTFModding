using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaywardExtensions;
using Microsoft.Xna.Framework;

namespace BackdropExtension
{
    public class GenericEmptyInstance : BackdropInstance
    {
        private Color colorKey;

        public GenericEmptyInstance(Color k)
        {
            colorKey = k;
        }

        public Color getKey()
        {
            return colorKey;
        }

        public void update(GameTime time)
        {
        }
    }
}

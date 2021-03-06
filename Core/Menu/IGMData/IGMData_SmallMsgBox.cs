﻿using System;
using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public class IGMData_SmallMsgBox : IGMData, I_Data<FF8String>
    {
        private Box_Options _options;
        #region Fields

        private Rectangle _bounding;

        #endregion Fields

        #region Constructors

        public IGMData_SmallMsgBox(FF8String data, int x, int y, Icons.ID? title = null, Box_Options options = Box_Options.Default, Rectangle? bounding = null) : base(container: new IGMDataItem_Box(data, new Rectangle(x, y, 0, 0), title, options: Box_Options.Center | Box_Options.Middle))
        {
            this._options = options;
            // probably won't work as static size is set in update. And this may run before it's set.
            _bounding = bounding ?? new Rectangle(Point.Zero, Menu.StaticSize.ToPoint());
            Reposition();
        }

        private void Reposition()
        {
            if (CONTAINER.GetType() == typeof(IGMDataItem_Box))
            {
                ((IGMDataItem_Box)CONTAINER).Draw(true);
                System.Tuple<Rectangle, Point, Rectangle> dims = ((IGMDataItem_Box)CONTAINER).Dims;
                if ((_options & Box_Options.Center) != 0)
                {
                    CONTAINER.X = _bounding.Width / 2 - dims.Item1.Width / 2;
                }
                if ((_options & Box_Options.Middle) != 0)
                {
                    CONTAINER.Y = _bounding.Height / 2 - dims.Item1.Height / 2;
                }
            }
        }

        public FF8String Data
        {
            get => ((IGMDataItem_Box)CONTAINER).Data; set
            {
                ((IGMDataItem_Box)CONTAINER).Data = value;
                Reposition();
            }
        }

        #endregion Constructors
    }
}
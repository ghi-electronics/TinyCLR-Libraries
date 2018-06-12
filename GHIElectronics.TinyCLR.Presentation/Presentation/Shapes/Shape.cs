////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Shapes
{
    public abstract class Shape : UIElement
    {
        public Media.Brush Fill
        {
            get
            {
                if (_fill == null)
                {
                    _fill = new SolidColorBrush(Color.Black);
                    _fill.Opacity = Bitmap.OpacityTransparent;
                }

                return _fill;
            }

            set
            {
                _fill = value;
                Invalidate();
            }
        }

        public Media.Pen Stroke
        {
            get
            {
                if (_stroke == null)
                {
                    _stroke = new Media.Pen(Color.White, 0);
                }

                return _stroke;
            }

            set
            {
                _stroke = value;
                Invalidate();
            }
        }

        private Media.Brush _fill;
        private Media.Pen _stroke;
    }
}



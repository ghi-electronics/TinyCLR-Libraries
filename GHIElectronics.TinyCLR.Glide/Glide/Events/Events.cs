////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Glide
{
    /// <summary>
    /// Tap event handler.
    /// </summary>
    /// <param name="sender">Object associated with the event.</param>
    public delegate void OnTap(object sender);

    /// <summary>
    /// Tap option event handler.
    /// </summary>
    /// <param name="sender">Object associated with the event.</param>
    /// <param name="args">Tap option event arguments.</param>
    public delegate void OnTapOption(object sender, TapOptionEventArgs args);

    /// <summary>
    /// Tap cell event handler.
    /// </summary>
    /// <param name="sender">Object associated with this event.</param>
    /// <param name="args">Tap cell event arguments.</param>
    public delegate void OnTapCell(object sender, TapCellEventArgs args);

    /// <summary>
    /// Tap key event handler.
    /// </summary>
    /// <param name="sender">Object associated with the event.</param>
    /// <param name="args">Tap key event arguments.</param>
    public delegate void OnTapKey(object sender, TapKeyEventArgs args);

    /// <summary>
    /// Value changed event handler.
    /// </summary>
    /// <param name="sender">Object associated with the event.</param>
    public delegate void OnValueChanged(object sender);

    /// <summary>
    /// Close event handler.
    /// </summary>
    /// <param name="sender">Object associated with the event.</param>
    public delegate void OnClose(object sender);

    /// <summary>
    /// Press event handler.
    /// </summary>
    /// <param name="sender">Object associated with the event.</param>
    public delegate void OnPress(object sender);

    /// <summary>
    /// Release event handler.
    /// </summary>
    /// <param name="sender">Object associated with the event.</param>
    public delegate void OnRelease(object sender);

    /// <summary>
    /// Rendered event handler.
    /// </summary>
    /// <param name="sender">Object associated with the event.</param>
    public delegate void OnRendered(object sender);

    /// <summary>
    /// Gesture event handler.
    /// </summary>
    /// <param name="sender">Object associated with the event.</param>
    /// <param name="args">Touch gesture event arguments.</param>
    //public delegate void OnGesture(object sender, TouchGestureEventArgs args);

    /// <summary>
    /// Tap option event arguments.
    /// </summary>
    public class TapOptionEventArgs
    {
        /// <summary>
        /// Index
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// Label
        /// </summary>
        public readonly string Label;

        /// <summary>
        /// Value
        /// </summary>
        public readonly object Value;

        /// <summary>
        /// Creates a new TapOptionEventArgs.
        /// </summary>
        /// <param name="index">Option index.</param>
        /// <param name="label">Option label.</param>
        /// <param name="value">Option value.</param>
        public TapOptionEventArgs(int index, string label, object value)
        {
            Index = index;
            Label = label;
            Value = value;
        }
    }

    /// <summary>
    /// Tap cell event arguments.
    /// </summary>
    public class TapCellEventArgs
    {
        /// <summary>
        /// Column index.
        /// </summary>
        public readonly int ColumnIndex;

        /// <summary>
        /// Row index.
        /// </summary>
        public readonly int RowIndex;

        /// <summary>
        /// Creates a new TapCellEventArgs.
        /// </summary>
        /// <param name="columnIndex">X coordinate</param>
        /// <param name="rowIndex">Y coordinate</param>
        public TapCellEventArgs(int columnIndex, int rowIndex)
        {
            ColumnIndex = columnIndex;
            RowIndex = rowIndex;
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns>Tap cell event properties.</returns>
        public override string ToString()
        {
            return "{ ColumnIndex: " + ColumnIndex + ", RowIndex: " + RowIndex + " }";
        }
    }

    /// <summary>
    /// Tap key event arguments.
    /// </summary>
    public class TapKeyEventArgs
    {
        /// <summary>
        /// Value
        /// </summary>
        public readonly string Value;

        /// <summary>
        /// Index
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// Creates a new KeyUpEventArgs.
        /// </summary>
        /// <param name="value">Character the key represents.</param>
        /// <param name="index">Key index within current view.</param>
        public TapKeyEventArgs(string value, int index)
        {
            Value = value;
            Index = index;
        }
    }
}

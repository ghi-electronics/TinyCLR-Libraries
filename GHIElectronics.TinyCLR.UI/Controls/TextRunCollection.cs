////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>An ordered collection of <see cref="TextRun"/> items belonging to a <see cref="TextFlow"/>.</summary>
    public class TextRunCollection : ICollection {
        private TextFlow _textFlow;
        private ArrayList _textRuns;

        internal TextRunCollection(TextFlow textFlow) {
            this._textFlow = textFlow;
            this._textRuns = new ArrayList();
        }

        /// <summary>The number of runs in the collection.</summary>
        public int Count => this._textRuns.Count;

        /// <summary>Creates a run from the given text, font, and color and adds it, returning its index.</summary>
        public int Add(string text, System.Drawing.Font font, Color foreColor) => Add(new TextRun(text, font, foreColor));

        /// <summary>Adds an existing run to the collection and returns its index.</summary>
        public int Add(TextRun textRun) {
            if (textRun == null) {
                throw new ArgumentNullException("textRun");
            }

            var result = this._textRuns.Add(textRun);
            this._textFlow.InvalidateMeasure();
            return result;
        }

        /// <summary>Removes all runs from the collection.</summary>
        public void Clear() {
            this._textRuns.Clear();
            this._textFlow.InvalidateMeasure();
        }

        /// <summary>Returns whether the given run is in the collection.</summary>
        public bool Contains(TextRun run) => this._textRuns.Contains(run);

        /// <summary>Returns the index of the given run, or -1 if it is not present.</summary>
        public int IndexOf(TextRun run) => this._textRuns.IndexOf(run);

        /// <summary>Inserts a run at the given index.</summary>
        public void Insert(int index, TextRun run) {
            this._textRuns.Insert(index, run);
            this._textFlow.InvalidateMeasure();
        }

        /// <summary>Removes the given run from the collection.</summary>
        public void Remove(TextRun run) {
            this._textRuns.Remove(run);
            this._textFlow.InvalidateMeasure();
        }

        /// <summary>Removes the run at the given index.</summary>
        public void RemoveAt(int index) {
            if (index < 0 || index >= this._textRuns.Count) {
                throw new ArgumentOutOfRangeException("index");
            }

            this._textRuns.RemoveAt(index);

            this._textFlow.InvalidateMeasure();
        }

        /// <summary>Gets or sets the run at the given index.</summary>
        public TextRun this[int index] {
            get => (TextRun)this._textRuns[index];

            set {
                this._textRuns[index] = value;
                this._textFlow.InvalidateMeasure();
            }
        }

        #region ICollection Members

        /// <summary>Always false; access to the collection is not synchronized.</summary>
        public bool IsSynchronized => false;

        /// <summary>Copies the runs to the given array starting at the specified index.</summary>
        public void CopyTo(Array array, int index) => this._textRuns.CopyTo(array, index);

        /// <summary>Always null; the collection does not expose a synchronization root.</summary>
        public object SyncRoot => null;

        #endregion

        #region IEnumerable Members

        /// <summary>Returns an enumerator over the runs in the collection.</summary>
        public IEnumerator GetEnumerator() => this._textRuns.GetEnumerator();

        #endregion
    }
}



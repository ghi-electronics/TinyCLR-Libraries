////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class TextRunCollection : ICollection {
        private TextFlow _textFlow;
        private ArrayList _textRuns;

        internal TextRunCollection(TextFlow textFlow) {
            this._textFlow = textFlow;
            this._textRuns = new ArrayList();
        }

        public int Count => this._textRuns.Count;

        public int Add(string text, System.Drawing.Font font, Color foreColor) => Add(new TextRun(text, font, foreColor));

        public int Add(TextRun textRun) {
            if (textRun == null) {
                throw new ArgumentNullException("textRun");
            }

            var result = this._textRuns.Add(textRun);
            this._textFlow.InvalidateMeasure();
            return result;
        }

        public void Clear() {
            this._textRuns.Clear();
            this._textFlow.InvalidateMeasure();
        }

        public bool Contains(TextRun run) => this._textRuns.Contains(run);

        public int IndexOf(TextRun run) => this._textRuns.IndexOf(run);

        public void Insert(int index, TextRun run) {
            this._textRuns.Insert(index, run);
            this._textFlow.InvalidateMeasure();
        }

        public void Remove(TextRun run) {
            this._textRuns.Remove(run);
            this._textFlow.InvalidateMeasure();
        }

        public void RemoveAt(int index) {
            if (index < 0 || index >= this._textRuns.Count) {
                throw new ArgumentOutOfRangeException("index");
            }

            this._textRuns.RemoveAt(index);

            this._textFlow.InvalidateMeasure();
        }

        public TextRun this[int index] {
            get => (TextRun)this._textRuns[index];

            set {
                this._textRuns[index] = value;
                this._textFlow.InvalidateMeasure();
            }
        }

        #region ICollection Members

        public bool IsSynchronized => false;

        public void CopyTo(Array array, int index) => this._textRuns.CopyTo(array, index);

        public object SyncRoot => null;

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator() => this._textRuns.GetEnumerator();

        #endregion
    }
}



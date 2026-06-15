
using System.Drawing;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>A list box item that highlights its text and background when selected.</summary>
    public class ListBoxItemHighlightable : ListBoxItem {
        private readonly Text content;

        private Media.Color backgroundSelectedColor;
        private Media.Color foreColorSelectedColor;
        private Media.Color foreColorUnselectColor;

        /// <summary>Creates a highlightable item with the given text and selected/unselected colors.</summary>
        public ListBoxItemHighlightable(string content, Font font, int margin, Media.Color backgroundSelectedColor, Media.Color foreColorSelectedColor, Media.Color foreColorUnselectColor) : base() {
            this.content = new Text(font, content);
            this.content.SetMargin(margin);
            this.Child = this.content;

            this.backgroundSelectedColor = backgroundSelectedColor;
            this.foreColorSelectedColor = foreColorSelectedColor;
            this.foreColorUnselectColor = foreColorUnselectColor;

            this.Background = null;
            this.content.ForeColor = this.foreColorUnselectColor;
        }

        /// <summary>Updates the background and text color when the selected state changes.</summary>
        protected internal override void OnIsSelectedChanged(bool isSelected) {
            if (isSelected) {                
                this.Background = new Media.SolidColorBrush(this.backgroundSelectedColor);
                this.content.ForeColor = this.foreColorSelectedColor;
            }
            else {
                this.Background = null;
                this.content.ForeColor = this.foreColorUnselectColor;
            }
        }

    }
}

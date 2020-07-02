using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.UI;

namespace GHIElectronics.TinyCLR.Glide.Controls {
    public class ElementContainer {
        public UIElement Element { get; set; }
        public string ElementName { get; set; }

        public ElementContainer(string name,UIElement element) {
            this.ElementName = name;
            this.Element = element;
        }
    }
}

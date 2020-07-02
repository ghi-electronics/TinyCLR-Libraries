using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Glide.Controls;

namespace GHIElectronics.TinyCLR.UI.Controls  {
    public class GlideWindow : Window {
        private Hashtable uiControls;

        public GlideWindow() {
            this.uiControls = new  Hashtable();
        }

        public void AddControls(string Name,UIElement element) {
            uiControls.Add(Name, element);
        }

        public void ClearControls() {
            uiControls.Clear();
        }

        public void RemoveControls(string Name) {
            if(uiControls.Contains(Name))
                uiControls.Remove(Name);

        }

        public UIElement GetControl(string Name) {
            if (uiControls.Contains(Name))
                return uiControls[Name] as UIElement;
            else
                return null;
        }

        public ElementContainer[] GetAllControls() {
            var arr = new ElementContainer[uiControls.Count];
            var counter = 0;
            foreach(var item in uiControls.Keys) {
                arr[counter++] = new ElementContainer(item.ToString(), (UIElement)uiControls[item]);
            }
            return arr;
        }
    }
}

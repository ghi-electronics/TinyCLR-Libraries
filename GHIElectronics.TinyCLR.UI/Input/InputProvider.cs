using System;
using GHIElectronics.TinyCLR.UI.Threading;

namespace GHIElectronics.TinyCLR.UI.Input {
    public sealed class InputProvider {
        private readonly InputProviderSite buttonSite;
        private readonly Application application;

        public InputProvider(Application a) {
            this.buttonSite = InputManager.CurrentInputManager.RegisterInputProvider(this);
            this.application = a;
        }

        public void RaiseButton(HardwareButton button, bool state, DateTime time) {
            var report = new RawButtonInputReport(null, time, button, state ? RawButtonActions.ButtonUp : RawButtonActions.ButtonDown);
            var dev = InputManager.CurrentInputManager.ButtonDevice;

            // Avoid stacking BeginInvokes (e.g. DispatcherTimer + GPIO already marshals to the UI thread) — long queues make Stop Debugging sluggish.
            if (this.application.Dispatcher.CheckAccess()) {
                this.buttonSite.ReportInput(dev, report);
            } else {
                this.application.Dispatcher.BeginInvoke(
                    new DispatcherOperationCallback(ReportButtonInput),
                    new InputReportArgs(dev, report));
            }
        }

        private object ReportButtonInput(object o) {
            var a = (InputReportArgs)o;
            this.buttonSite.ReportInput(a.Device, a.Report);
            return null;
        }

        public void RaiseTouch(int x, int y, TouchMessages which, DateTime time) => Application.Current.OnEvent(new TouchEvent() { Time = time, EventMessage = (byte)which, Touches = new[] { new TouchInput() { X = x, Y = y } } });

        /// <summary>
        /// Moves focus between tab stops (map hardware keys or UART keys to this for PC-style navigation).
        /// </summary>
        public void RaiseFocusNavigation(bool forward) =>
            this.application.Dispatcher.BeginInvoke(_ => FocusNavigator.TryMoveFocus(forward), null);
    }
}

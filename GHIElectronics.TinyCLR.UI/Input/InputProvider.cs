using System;

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
            var args = new InputReportArgs(InputManager.CurrentInputManager.ButtonDevice, report);

            this.application.Dispatcher.BeginInvoke(_ => this.buttonSite.ReportInput(args.Device, args.Report), null);
        }

        public void RaiseTouch(int x, int y, TouchMessages which, DateTime time) => Application.Current.OnEvent(new TouchEvent() { Time = time, EventMessage = (byte)which, Touches = new[] { new TouchInput() { X = x, Y = y } } });
    }
}

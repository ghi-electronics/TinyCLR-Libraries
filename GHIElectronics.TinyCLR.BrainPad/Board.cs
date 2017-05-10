using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.ComponentModel;

namespace GHIElectronics.TinyCLR.BrainPad {
    /// <summary>
    /// The BrainPad class used with GHI Electronics's BrainPad.
    /// </summary>
    public class Board {
        /// <summary>
        /// Writes a message to the output window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void WriteToComputer(string message) => System.Diagnostics.Debug.WriteLine(message);

        /// <summary>
        /// Writes a message to the output window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void WriteToComputer(int message) => WriteToComputer(message.ToString("N0"));

        /// <summary>
        /// Writes a message to the output window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void WriteToComputer(double message) => WriteToComputer(message.ToString("N4"));

        /// <summary>
        /// Provices access to the accelerometer on the BrainPad.
        /// </summary>
        public Accelerometer Accelerometer { get; }
        /// <summary>
        /// Provices access to the buttons on the BrainPad.
        /// </summary>
        public Buttons Buttons { get; }
        /// <summary>
        /// Provides access to the buzzer on the BrainPad.
        /// </summary>
        public Buzzer Buzzer { get; }
        /// <summary>
        /// Controls the display on the BrainPad.
        /// </summary>
        public Display Display { get; }
        /// <summary>
        /// Provides access to the light bulb on the BrainPad.
        /// </summary>
        public LightBulb LightBulb { get; }
        /// <summary>
        /// Provides access to the light sensor on the BrainPad.
        /// </summary>
        public LightSensor LightSensor { get; }
        /// <summary>
        /// Provides access to the servo motor on the BrainPad.
        /// </summary>
        public ServoMotors ServoMotors { get; }
        /// <summary>
        /// Provides access to the temperature sensor on the BrainPad.
        /// </summary>
        public TemperatureSensor TemperatureSensor { get; }
        /// <summary>
        /// Tells the BrainPad to wait.
        /// </summary>
        public Wait Wait { get; }

        public Board() {
            this.Accelerometer = new Accelerometer();
            this.Buttons = new Buttons();
            this.Buzzer = new Buzzer();
            this.Display = new Display();
            this.LightBulb = new LightBulb();
            this.LightSensor = new LightSensor();
            this.ServoMotors = new ServoMotors();
            this.TemperatureSensor = new TemperatureSensor();
            this.Wait = new Wait();
        }

        private static bool typeSet;
        private static BoardType boardType;

        internal static BoardType BoardType {
            get {
                if (!Board.typeSet) {
                    using (var detectPin = GpioController.GetDefault().OpenPin(G30.GpioPin.PC1)) {
                        detectPin.SetDriveMode(GpioPinDriveMode.InputPullDown);

                        Board.boardType = detectPin.Read() == GpioPinValue.High ? BoardType.BP1 : BoardType.Original;
                    }

                    Board.typeSet = true;
                }

                return Board.boardType;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType() => base.GetType();
    }

    internal enum BoardType {
        Original,
        BP1
    }
}

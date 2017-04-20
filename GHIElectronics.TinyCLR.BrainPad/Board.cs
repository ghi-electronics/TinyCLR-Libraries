using GHIElectronics.TinyCLR.BrainPad.Internal;

namespace GHIElectronics.TinyCLR.BrainPad {
    /// <summary>
    /// The BrainPad class used with GHI Electronics's BrainPad.
    /// </summary>
    public class Board {
        /// <summary>
        /// Writes a message to the output window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void WriteOnComputer(string message) => System.Diagnostics.Debug.WriteLine(message);

        /// <summary>
        /// Writes a message to the output window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void WriteOnComputer(int message) => WriteOnComputer(message.ToString("N0"));

        /// <summary>
        /// Writes a message to the output window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void WriteOnComputer(double message) => WriteOnComputer(message.ToString("N4"));

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
    }
}

using GHIElectronics.TinyCLR.BrainPad.Internal;

namespace GHIElectronics.TinyCLR.BrainPad {
    /// <summary>
    /// The BrainPad class used with GHI Electronics's BrainPad.
    /// </summary>
    public class Board {
        /// <summary>
        /// A constant value that is always true for endless looping.
        /// </summary>
        public bool Looping = true;

        /// <summary>
        /// Writes a message to the output window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void WriteDebugMessage(string message) => System.Diagnostics.Debug.WriteLine(message);

        /// <summary>
        /// Writes a message to the output window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void WriteDebugMessage(int message) => WriteDebugMessage(message.ToString());

        /// <summary>
        /// Writes a message to the output window.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void WriteDebugMessage(double message) => WriteDebugMessage(message.ToString());

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
        /// Controls the display on the BrainPad.
        /// </summary>
        public ImageBuffer ImageBuffer { get; }
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
        public ServoMotor ServoMotor { get; }
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
            this.ImageBuffer = this.Display.imageBuffer;
            this.LightBulb = new LightBulb();
            this.LightSensor = new LightSensor();
            this.ServoMotor = new ServoMotor();
            this.TemperatureSensor = new TemperatureSensor();
            this.Wait = new Wait();
        }
    }
}

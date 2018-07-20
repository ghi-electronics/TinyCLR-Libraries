using System;
using System.ComponentModel;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Pins;

namespace GHIElectronics.TinyCLR.BrainPad {
    public class ServoMotors {
        public class Servo {
            private PwmChannel servo;
            private bool invertServo;
            private double minPulseLength;
            private double maxPulseLength;
            private PwmController controller;
            private ServoType type;

            internal Servo(int servo) {
                switch (Board.BoardType) {
                    case BoardType.BP2:
                        this.controller = PwmController.FromName(FEZCLR.PwmPin.Controller2.Id);

                        break;

                    case BoardType.Original:
                        this.controller = PwmController.FromName(servo == 0 ? G30.PwmPin.Controller1.Id : G30.PwmPin.Controller2.Id);

                        break;

                    default: throw new InvalidOperationException();
                }

                this.invertServo = false;

                this.ConfigurePulseParameters(1.0, 2.0);
                this.ConfigureAsPositional(false);

                switch (Board.BoardType) {
                    case BoardType.BP2:
                        this.servo = this.controller.OpenChannel(servo == 0 ? FEZCLR.PwmPin.Controller2.PA3 : FEZCLR.PwmPin.Controller2.PA0);

                        break;

                    case BoardType.Original:
                        this.servo = this.controller.OpenChannel(servo == 0 ? G30.PwmPin.Controller1.PA8 : G30.PwmPin.Controller2.PA0);

                        break;

                    default: throw new InvalidOperationException();
                }

                this.EnsureFrequency();
            }

            private enum ServoType {
                Positional,
                Continuous
            }

            public void ConfigureAsPositional(bool inverted) {
                this.type = ServoType.Positional;
                this.invertServo = inverted;
            }

            public void ConfigureAsContinuous(bool inverted) {
                this.type = ServoType.Continuous;
                this.invertServo = inverted;
            }

            public void ConfigurePulseParameters(double minimumPulseWidth, double maximumPulseWidth) {
                if (minimumPulseWidth > 1.5 || minimumPulseWidth < 0.1) throw new ArgumentOutOfRangeException("Must be between 0.1 and 1.5 ms");
                if (maximumPulseWidth > 3 || maximumPulseWidth < 1.6) throw new ArgumentOutOfRangeException("Must be between 1.6 and 3 ms");

                this.minPulseLength = minimumPulseWidth;
                this.maxPulseLength = maximumPulseWidth;
            }

            public void Set(double value) {
                if (this.type == ServoType.Positional)
                    this.FixedSetPosition(value);
                else
                    this.ContiniousSetSpeed(value);
            }

            private void FixedSetPosition(double position) {
                if (position < 0 || position > 180) throw new ArgumentOutOfRangeException("degrees", "degrees must be between 0 and 180.");

                this.EnsureFrequency();// in case we used the other stuff. remove when we fix PWM controllers

                if (this.invertServo == true)
                    position = 180 - position;

                // Typically, with 50 hz, 0 degree is 0.05 and 180 degrees is 0.10
                //double duty = ((position / 180.0) * (0.10 - 0.05)) + 0.05;
                var duty = ((position / 180.0) * (this.maxPulseLength / 20 - this.minPulseLength / 20)) + this.minPulseLength / 20;


                this.servo.SetActiveDutyCyclePercentage(duty);
                this.servo.Start();
            }

            private void ContiniousSetSpeed(double speed) {
                if (speed < -100 || speed > 100) throw new ArgumentOutOfRangeException("speed", "degrees must be between -100 and 100.");

                speed += 100;
                var d = speed / 200.0 * 180;
                FixedSetPosition(d);
            }

            private void EnsureFrequency() => this.controller.SetDesiredFrequency(1 / 0.020);


            /// <summary>
            /// Stops the servo motor.
            /// </summary>
            public void Stop() => this.servo.Stop();

            [EditorBrowsable(EditorBrowsableState.Never)]
            public override bool Equals(object obj) => base.Equals(obj);
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override int GetHashCode() => base.GetHashCode();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override string ToString() => base.ToString();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public new Type GetType() => base.GetType();
        }

        public Servo ServoOne { get; }
        public Servo ServoTwo { get; }

        public ServoMotors() {
            this.ServoOne = new Servo(0);
            this.ServoTwo = new Servo(1);
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
}

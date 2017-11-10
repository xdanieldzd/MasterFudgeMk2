using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using SharpDX.DirectInput;

using MasterFudgeMk2.Common.Enumerations;
using MasterFudgeMk2.Common.EventArguments;

namespace MasterFudgeMk2.InputBackends
{
    [Description("DirectInput (SharpDX)")]
    public class DirectInputBackend : IInputBackend
    {
        DirectInput directInput;

        Keyboard keyboard;
        Joystick joystick;

        public DirectInputBackend()
        {
            directInput = new DirectInput();

            keyboard = new Keyboard(directInput);
            if (keyboard != null)
            {
                keyboard.Properties.BufferSize = 128;
                keyboard.Acquire();
            }

            var joystickDevices = directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
            if (joystickDevices.Count > 0)
            {
                var joystickGuid = joystickDevices.FirstOrDefault().InstanceGuid;
                joystick = new Joystick(directInput, joystickGuid);
                if (joystick != null)
                {
                    joystick.Properties.BufferSize = 128;
                    joystick.Properties.DeadZone = 2000;
                    joystick.Acquire();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (joystick != null)
                {
                    joystick.Unacquire();
                    joystick.Dispose();
                }

                if (keyboard != null)
                {
                    keyboard.Unacquire();
                    keyboard.Dispose();
                }

                if (directInput != null)
                {
                    directInput.Dispose();
                }
            }
        }

        public IEnumerable<Enum> GetInputValues()
        {
            // TODO: ?????

            List<Enum> values = new List<Enum>();
            values.AddRange(Enum.GetValues(typeof(JoystickOffset)).Cast<Enum>());
            values.AddRange(Enum.GetValues(typeof(Key)).Cast<Enum>());
            return values;
        }

        public string GetInputDescription(Enum input, int value)
        {
            string description = string.Empty;

            try
            {
                DeviceObjectInstance objectInstance;

                if (input.GetType() == typeof(Key))
                    objectInstance = keyboard?.GetObjectInfoByName(Enum.GetName(typeof(Key), input));
                else if (input.GetType() == typeof(JoystickOffset))
                    objectInstance = joystick?.GetObjectInfoByName(Enum.GetName(typeof(JoystickOffset), input));
                else
                    throw new Exception("Trying to get description from unhandled enum");

                if (objectInstance != null)
                    description = objectInstance.Name;
            }
            catch (SharpDX.SharpDXException e)
            {
                // 0x80070002 (ERROR_FILE_NOT_FOUND) means we're trying to get infos for an object the input device doesn't support, otherwise rethrow the exception
                if (e.HResult != -2147024894)
                    throw e;
            }

            return description;
        }

        public void OnPollInput(object sender, PollInputEventArgs e)
        {
            var inputs = new List<Enum>();

            if (keyboard != null)
            {
                keyboard.Poll();
                var keyboardUpdates = keyboard.GetBufferedData();
                inputs.AddRange(keyboardUpdates.Where(x => x.IsPressed).Select(x => (Enum)x.Key));
            }

            if (joystick != null)
            {
                joystick.Poll();
                var joystickUpdates = joystick.GetBufferedData();
                inputs.AddRange(TranslateJoystickUpdates(joystickUpdates).Cast<Enum>());
            }

            e.Pressed = inputs;
        }

        // TODO: only does "pressed once" doesn't do held buttons/stick/etc

        private ControllerState[] TranslateJoystickUpdates(JoystickUpdate[] joystickUpdates)
        {
            List<ControllerState> translatedStates = new List<ControllerState>();

            foreach (JoystickUpdate update in joystickUpdates)
            {
                switch (update.Offset)
                {
                    case JoystickOffset.X: translatedStates.Add(TranslateIsAxisNegative(update) ? ControllerState.PrimaryXNegative : ControllerState.PrimaryXPositive); break;
                    case JoystickOffset.Y: translatedStates.Add(TranslateIsAxisNegative(update) ? ControllerState.PrimaryYNegative : ControllerState.PrimaryYPositive); break;
                    case JoystickOffset.Z: translatedStates.Add(TranslateIsAxisNegative(update) ? ControllerState.PrimaryZNegative : ControllerState.PrimaryZPositive); break;
                    case JoystickOffset.RotationX: translatedStates.Add(TranslateIsAxisNegative(update) ? ControllerState.SecondaryXNegative : ControllerState.SecondaryXPositive); break;
                    case JoystickOffset.RotationY: translatedStates.Add(TranslateIsAxisNegative(update) ? ControllerState.SecondaryYNegative : ControllerState.SecondaryYPositive); break;
                    case JoystickOffset.RotationZ: translatedStates.Add(TranslateIsAxisNegative(update) ? ControllerState.SecondaryZNegative : ControllerState.SecondaryZPositive); break;

                    case JoystickOffset.PointOfViewControllers0:
                        if (update.Value != -1)
                        {
                            if (TranslateIsPoVUp(update)) translatedStates.Add(ControllerState.PointOfViewUp);
                            if (TranslateIsPoVRight(update)) translatedStates.Add(ControllerState.PointOfViewRight);
                            if (TranslateIsPoVDown(update)) translatedStates.Add(ControllerState.PointOfViewDown);
                            if (TranslateIsPoVLeft(update)) translatedStates.Add(ControllerState.PointOfViewLeft);
                        }
                        break;

                    case JoystickOffset.Buttons0: translatedStates.Add(ControllerState.Button0); break;
                    case JoystickOffset.Buttons1: translatedStates.Add(ControllerState.Button1); break;
                    case JoystickOffset.Buttons2: translatedStates.Add(ControllerState.Button2); break;
                    case JoystickOffset.Buttons3: translatedStates.Add(ControllerState.Button3); break;
                    case JoystickOffset.Buttons4: translatedStates.Add(ControllerState.Button4); break;
                    case JoystickOffset.Buttons5: translatedStates.Add(ControllerState.Button5); break;
                    case JoystickOffset.Buttons6: translatedStates.Add(ControllerState.Button6); break;
                    case JoystickOffset.Buttons7: translatedStates.Add(ControllerState.Button7); break;
                    case JoystickOffset.Buttons8: translatedStates.Add(ControllerState.Button8); break;
                    case JoystickOffset.Buttons9: translatedStates.Add(ControllerState.Button9); break;

                    default: throw new Exception(string.Format("Unhandled joystick offset {0} for translation", update.Offset));
                }
                System.Diagnostics.Debug.Print(translatedStates.LastOrDefault().ToString());
            }

            return translatedStates.ToArray();
        }

        private bool TranslateIsAxisNegative(JoystickUpdate update)
        {
            return ((update.Value - short.MaxValue) < 0);
        }

        private int NormalizePoV(JoystickUpdate update)
        {
            return (update.Value / 4500);
        }

        private bool TranslateIsPoVUp(JoystickUpdate update)
        {
            return (NormalizePoV(update) >= 0 && NormalizePoV(update) < 2);
        }

        private bool TranslateIsPoVRight(JoystickUpdate update)
        {
            return (NormalizePoV(update) >= 2 && NormalizePoV(update) < 4);
        }

        private bool TranslateIsPoVDown(JoystickUpdate update)
        {
            return (NormalizePoV(update) >= 4 && NormalizePoV(update) < 6);
        }

        private bool TranslateIsPoVLeft(JoystickUpdate update)
        {
            return (NormalizePoV(update) >= 6 && NormalizePoV(update) < 8);
        }
    }
}

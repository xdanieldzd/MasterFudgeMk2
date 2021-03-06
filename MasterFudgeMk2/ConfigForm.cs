﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

using MasterFudgeMk2.Common.XInput;

using MasterFudgeMk2.Machines;

namespace MasterFudgeMk2
{
    public partial class ConfigForm : Form, IMessageFilter
    {
        public MachineConfiguration Configuration { get; private set; }

        Dictionary<Enum, Enum> keyConfiguration;

        Keys keysPressed, modifiersPressed;
        Keys lastKeyDown;
        Controller controller;

        Timer settingWaitTimer, inputPollTimer;
        int settingWaitCounter;
        bool waitingForInput { get { return (settingWaitCounter > 0); } }

        public ConfigForm(IMachineManager machineManager)
        {
            InitializeComponent();
            Application.AddMessageFilter(this);

            Text = string.Format("Configuration ({0})", machineManager.FriendlyName);

            Configuration = machineManager.Configuration;

            /* Main config stuff */
            List<PropertyInfo> mainProps = Configuration.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(x => x.PropertyType != typeof(Enum) && x.CanWrite).ToList();
            mainProps.RemoveAll(x => x.GetCustomAttributes<ReadOnlyAttribute>().FirstOrDefault().IsReadOnly);
            if (mainProps.Count == 0) tcConfig.TabPages.Remove(tpMainConfig);

            tlpMainConfig.RowCount = (mainProps.Count + 1);
            for (int i = 0; i < mainProps.Count; i++)
            {
                PropertyInfo prop = mainProps[i];

                object[] descAttribs = prop.GetCustomAttributes(typeof(DescriptionAttribute), false);
                string settingDescription = (descAttribs != null && descAttribs.Length > 0 ? (descAttribs[0] as DescriptionAttribute).Description : prop.Name);

                object[] readOnlyAttribs = prop.GetCustomAttributes(typeof(ReadOnlyAttribute), false);
                bool isReadOnly = (readOnlyAttribs != null && readOnlyAttribs.Length > 0 ? (readOnlyAttribs[0] as ReadOnlyAttribute).IsReadOnly : false);
                if (isReadOnly) continue;

                if (prop.PropertyType == typeof(bool))
                {
                    CheckBox checkBox = new CheckBox() { Text = settingDescription, Dock = DockStyle.Fill, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(6, 3, 3, 3) };
                    checkBox.DataBindings.Add("Checked", Configuration, prop.Name, false, DataSourceUpdateMode.OnPropertyChanged);
                    tlpMainConfig.Controls.Add(checkBox, 0, i);
                    tlpMainConfig.SetColumnSpan(checkBox, 3);
                }
                else
                {
                    tlpMainConfig.Controls.Add(new Label() { Text = settingDescription, Dock = DockStyle.Fill, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft }, 0, i);

                    if (prop.PropertyType == typeof(string))
                    {
                        TextBox textBox = new TextBox() { Dock = DockStyle.Fill, TextAlign = HorizontalAlignment.Left };
                        textBox.DataBindings.Add("Text", Configuration, prop.Name, false, DataSourceUpdateMode.OnPropertyChanged);
                        tlpMainConfig.Controls.Add(textBox, 1, i);
                        if (prop.Name.EndsWith("Path"))
                        {
                            textBox.ReadOnly = true;
                            Button browseButton = new Button() { Text = "...", ClientSize = new Size(25, textBox.Height), Tag = textBox };
                            tlpMainConfig.Controls.Add(browseButton, 2, i);
                            Button clearButton = new Button() { Text = "Clear", ClientSize = new Size(40, textBox.Height), Tag = textBox };
                            tlpMainConfig.Controls.Add(clearButton, 3, i);
                            tlpMainConfig.SetColumnSpan(textBox, 1);

                            browseButton.Click += (s, e) =>
                            {
                                using (OpenFileDialog ofd = new OpenFileDialog() { Filter = machineManager.FileFilter })
                                {
                                    if (ofd.ShowDialog() == DialogResult.OK)
                                        ((s as Button).Tag as TextBox).Text = ofd.FileName;
                                }
                            };

                            clearButton.Click += (s, e) =>
                            {
                                ((s as Button).Tag as TextBox).Text = string.Empty;
                            };
                        }
                        else
                            tlpMainConfig.SetColumnSpan(textBox, 3);
                    }
                    else if (prop.PropertyType.BaseType == typeof(Enum))
                    {
                        ComboBox comboBox = new ComboBox() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
                        comboBox.DataSource = Enum.GetValues(prop.PropertyType)
                            .Cast<Enum>()
                            .Select(value => new
                            {
                                (Attribute.GetCustomAttribute(value.GetType().GetField(value.ToString()), typeof(DescriptionAttribute)) as DescriptionAttribute).Description,
                                value
                            })
                            .OrderBy(item => item.value)
                            .ToList();
                        comboBox.DisplayMember = "Description";
                        comboBox.ValueMember = "value";

                        comboBox.DataBindings.Add("SelectedValue", Configuration, prop.Name, false, DataSourceUpdateMode.OnPropertyChanged);
                        tlpMainConfig.Controls.Add(comboBox, 1, i);
                        tlpMainConfig.SetColumnSpan(comboBox, 3);
                    }
                }
            }

            /* Key config stuff */
            keyConfiguration = new Dictionary<Enum, Enum>();
            foreach (PropertyInfo prop in Configuration.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(x => x.PropertyType == typeof(Enum)))
            {
                var key = Type.GetType(machineManager.GetType().Namespace + ".MachineInputs").GetField(prop.Name, BindingFlags.Public | BindingFlags.Static).GetValue(null);
                var value = prop.GetValue(Configuration);
                keyConfiguration.Add((Enum)key, (Enum)value);
            }

            keysPressed = modifiersPressed = Keys.None;
            lastKeyDown = Keys.None;
            controller = ControllerManager.GetController(0);

            settingWaitTimer = new Timer();
            settingWaitTimer.Tick += SettingWaitTimer_Tick;

            inputPollTimer = new Timer();
            inputPollTimer.Interval = 10;
            inputPollTimer.Tick += (s, e) =>
            {
                Timer timer = (s as Timer);
                timer.Tag = null;

                controller.Update();

                if ((keysPressed & Keys.KeyCode) != Keys.None && !keysPressed.HasFlag(Keys.Escape)) timer.Tag = keysPressed;
                if (controller.Buttons != Buttons.None) timer.Tag = controller.Buttons;

                if (timer.Tag == null && controller.LeftThumbstick?.X < -XInputGamepad.LeftThumbDeadzone) timer.Tag = Buttons.DPadLeft;
                if (timer.Tag == null && controller.LeftThumbstick?.Y < -XInputGamepad.LeftThumbDeadzone) timer.Tag = Buttons.DPadDown;
                if (timer.Tag == null && controller.LeftThumbstick?.X > XInputGamepad.LeftThumbDeadzone) timer.Tag = Buttons.DPadRight;
                if (timer.Tag == null && controller.LeftThumbstick?.Y > XInputGamepad.LeftThumbDeadzone) timer.Tag = Buttons.DPadUp;

                if (timer.Tag != null || keysPressed.HasFlag(Keys.Escape))
                {
                    StopTimer(timer);
                    settingWaitTimer.Stop();

                    settingWaitCounter = 0;
                    keysPressed = modifiersPressed = Keys.None;
                    lastKeyDown = Keys.None;
                }
            };

            tlpInputConfig.SuspendLayout();
            tlpInputConfig.RowCount = (keyConfiguration.Count + 1);
            for (int i = 0; i < keyConfiguration.Count; i++)
            {
                KeyValuePair<Enum, Enum> mapping = keyConfiguration.ElementAt(i);

                var descriptionAttribs = mapping.Key.GetType().GetField(mapping.Key.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
                string keyDescription = ((descriptionAttribs != null && descriptionAttribs.Length > 0) ? (descriptionAttribs[0] as DescriptionAttribute).Description : mapping.Key.ToString());

                tlpInputConfig.Controls.Add(new Label() { Text = keyDescription, Dock = DockStyle.Fill, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft }, 0, i);

                Button keyChangeButton = new Button() { Dock = DockStyle.Fill, FlatStyle = FlatStyle.Popup, Tag = mapping };
                keyChangeButton.Click += (s, e) =>
                {
                    tcConfig.Enabled = tlpInputConfig.Enabled = false;

                    settingWaitCounter = 6;
                    settingWaitTimer.Interval = 1000;
                    settingWaitTimer.Tag = s;
                    SettingWaitTimer_Tick(settingWaitTimer, EventArgs.Empty);
                    settingWaitTimer.Start();

                    inputPollTimer.Start();
                };
                SetButtonLabel(keyChangeButton, mapping.Value);
                tlpInputConfig.Controls.Add(keyChangeButton, 1, i);

                Button keyClearButton = new Button() { Text = "Clear", Dock = DockStyle.Fill, FlatStyle = FlatStyle.Popup, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Tag = keyChangeButton };
                keyClearButton.Click += (s, e) =>
                {
                    Button keyChgBtn = ((s as Button).Tag as Button);
                    KeyValuePair<Enum, Enum> map = (KeyValuePair<Enum, Enum>)(keyChgBtn.Tag);
                    keyConfiguration.Remove(map.Key);
                    keyConfiguration.Add(map.Key, null);
                    SetButtonLabel(keyChgBtn, null);
                };
                tlpInputConfig.Controls.Add(keyClearButton, 2, i);
            }

            tlpInputConfig.ResumeLayout();
        }

        public bool PreFilterMessage(ref Message m)
        {
            const int WM_KEYUP = 0x101;
            const int WM_SYSKEYUP = 0x0105;

            if ((m.Msg == WM_KEYUP) || (m.Msg == WM_SYSKEYUP))
            {
                if (keysPressed == Keys.None && ((lastKeyDown & Keys.KeyCode) == Keys.ControlKey || (lastKeyDown & Keys.KeyCode) == Keys.ShiftKey || (lastKeyDown & Keys.KeyCode) == Keys.Menu))
                {
                    keysPressed |= (lastKeyDown & Keys.KeyCode);
                }
                else
                {
                    keysPressed &= ~(lastKeyDown & Keys.KeyCode);
                    modifiersPressed &= ~(lastKeyDown & Keys.Modifiers);
                }
            }

            return false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x100;
            const int WM_SYSKEYDOWN = 0x104;

            if (waitingForInput)
            {
                lastKeyDown = keyData;

                Keys keyCode = (keyData & Keys.KeyCode);
                Keys modifier = (keyData & Keys.Modifiers);

                if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
                {
                    if ((keyCode != Keys.ControlKey) && (keyCode != Keys.ShiftKey) && (keyCode != Keys.Menu))
                    {
                        keysPressed |= (keyCode | modifiersPressed);
                        return true;
                    }
                    else
                    {
                        modifiersPressed |= modifier;
                        return false;
                    }
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right) e.IsInputKey = true;
            base.OnPreviewKeyDown(e);
        }

        private void SettingWaitTimer_Tick(object sender, EventArgs e)
        {
            Timer timer = (sender as Timer);

            (timer.Tag as Button).Text = string.Format("Waiting for input ({0})...", (settingWaitCounter - 1));

            settingWaitCounter--;
            if (settingWaitCounter == 0)
            {
                StopTimer(timer);
                inputPollTimer.Stop();
            }
        }

        private void StopTimer(Timer timer)
        {
            tcConfig.Enabled = tlpInputConfig.Enabled = true;
            timer.Stop();
            CheckCommitInputSetting();
        }

        private void SetButtonLabel(Button button, Enum value)
        {
            if (value != null)
            {
                string sourceDescription, inputDescription;

                var sourceDescriptionAttrib = value.GetType().GetCustomAttributes(typeof(DescriptionAttribute), false);
                sourceDescription = (sourceDescriptionAttrib.FirstOrDefault() != null ? (sourceDescriptionAttrib.FirstOrDefault() as DescriptionAttribute).Description : value.GetType().Name);

                var inputDescriptionField = value.GetType().GetField(value.ToString());
                var inputDescriptionAttrib = inputDescriptionField?.GetCustomAttributes(typeof(DescriptionAttribute), false);
                inputDescription = ((inputDescriptionAttrib != null && inputDescriptionAttrib.FirstOrDefault() != null) ? (inputDescriptionAttrib.FirstOrDefault() as DescriptionAttribute).Description : value.ToString());

                if (inputDescription.Contains(","))
                    button.Text = string.Format("({0}) [{1}]", sourceDescription, inputDescription.Replace(", ", " | "));
                else
                    button.Text = string.Format("({0}) {1}", sourceDescription, inputDescription);
            }
            else
                button.Text = "---";
        }

        private void CheckCommitInputSetting()
        {
            Button button = (settingWaitTimer.Tag as Button);
            KeyValuePair<Enum, Enum> mapping = (KeyValuePair<Enum, Enum>)button.Tag;

            if (inputPollTimer.Tag != null)
            {
                Enum value = (inputPollTimer.Tag as Enum);
                keyConfiguration.Remove(mapping.Key);
                keyConfiguration.Add(mapping.Key, value);
                SetButtonLabel(button, value);
            }
            else
                SetButtonLabel(button, mapping.Value);
        }

        private void ConfigForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (waitingForInput)
            {
                e.Cancel = true;
                return;
            }

            Application.RemoveMessageFilter(this);

            if (DialogResult == DialogResult.OK)
            {
                foreach (KeyValuePair<Enum, Enum> mapping in keyConfiguration)
                {
                    PropertyInfo prop = Configuration.GetType().GetProperty(mapping.Key.ToString());
                    prop?.SetValue(Configuration, Convert.ChangeType(mapping.Value, typeof(Enum)));
                }
            }
        }
    }
}

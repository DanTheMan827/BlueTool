using com.DanTheMan827.BlueTool.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace com.DanTheMan827.BlueTool
{
    public partial class MainForm : Form
    {
        private Control ParentControl;
        private BluetoothControl control = new BluetoothControl();
        private readonly IReadOnlyCollection<BluetoothControl.BluetoothDeviceData.DataType> interestedDataTypes = new BluetoothControl.BluetoothDeviceData.DataType[] {
            BluetoothControl.BluetoothDeviceData.DataType.Name,
            BluetoothControl.BluetoothDeviceData.DataType.Address,
            BluetoothControl.BluetoothDeviceData.DataType.Paired,
            BluetoothControl.BluetoothDeviceData.DataType.Pairable
        };

        public struct MenuItemTag
        {
            public BluetoothControl.BluetoothDeviceData Device;
            public BluetoothControl Control;
            public EventHandler Handler;
        }

        public struct Address
        {
            public string Hostname;
            public int? Port;
        }

        public MainForm()
        {
            InitializeComponent();
            textBoxAddress.Text = Settings.Default.Address;
            ParentControl = this;
            control.OnData += Control_OnData;
            labelInfo.Text = $"BlueTool v{Program.AppDisplayVersion}";
            listView1.Items.Clear();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MainForm_Resize(sender, e);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            columnHeader1.Width = listView1.Width - 35;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            control?.StopListener();
            Application.Exit();
        }

        private Address? ParseAddress(string address)
        {
            var regex = new Regex(@"^([a-z0-9\.]+)(?:\:(\d+))?$", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var match = regex.Match(address.Trim());
            if (match.Success)
            {
                var addressStruct = new Address()
                {
                    Hostname = match.Groups[1].Value
                };

                if (match.Groups[2].Value.Length > 0)
                {
                    addressStruct.Port = int.Parse(match.Groups[2].Value);
                }

                return addressStruct;
            }

            return null;
        }

        private void buttonConnection_Click(object sender, EventArgs e)
        {
            if ((bool)(buttonConnection.Tag ?? false) == false)
            {
                var address = ParseAddress(textBoxAddress.Text);
                
                if (!control.Connected)
                {
                    
                    control.StartListener(address?.Hostname, address?.Port ?? 787, false);
                }
                control.ProbeDevices();
                control.SetScan(true);
                this.ParentControl?.Invoke(new Action(PopulateMenu));
                buttonConnection.Tag = true;
                buttonConnection.Text = "Disconnect";
                textBoxAddress.Enabled = false;
            }
            else
            {
                control?.SetScan(false);
                control?.StopListener();
                listView1.Items.Clear();
                listView1.Groups.Clear();

                buttonConnection.Tag = false;
                buttonConnection.Text = "Connect";
                textBoxAddress.Enabled = true;
            }
        }

        private void Control_OnData(BluetoothControl.EventType eventType, BluetoothControl.BluetoothDeviceData data, BluetoothControl.BluetoothDeviceData.DataType dataType = BluetoothControl.BluetoothDeviceData.DataType.Undefined)
        {
            try
            {
                if (eventType == BluetoothControl.EventType.Delete || eventType == BluetoothControl.EventType.New)
                    this.ParentControl?.Invoke(new Action(PopulateMenu));

                if (eventType == BluetoothControl.EventType.Change && interestedDataTypes.Contains(dataType)
                )
                    this.ParentControl?.Invoke(new Action(PopulateMenu));
            }
            catch (Exception) { }
        }

        private void PopulateMenu()
        {
            listView1.SuspendLayout();
            listView1.Items.Clear();
            listView1.Groups.Clear();

            var control = this.control;

            var groupControllers = new ListViewGroup("Bluetooth Adapters");
            var groupPaired = new ListViewGroup("Paired Devices (Click to Unpair)");
            var groupTrusted = new ListViewGroup("Trusted Devices (Click to Remove)");
            var groupDetected = new ListViewGroup("Detected Devices (Click to Pair)");
            var groupManagement = new ListViewGroup("Management");

            var controllers = control.Devices.Values.Where(e => e.Type == BluetoothControl.BluetoothDeviceType.Controller).OrderBy(e => e.Name);
            var devices = control.Devices.Values.Where(e => e.Type == BluetoothControl.BluetoothDeviceType.Device).OrderBy(e => e.Name);
            var pairedDevices = devices.Where(e => e.Paired == true).OrderBy(e => e.Name);
            var detectedDevices = devices.Where(e => e.Paired == false && e.Trusted == false).OrderBy(e => e.Name);
            var trustedDevices = devices.Where(e => e.Paired == false && e.Trusted == true).OrderBy(e => e.Name);

            if (devices.Count() == 0 && controllers.Count() == 0)
            {
                listView1.Items.Add("No Devices Detected");
                listView1.ResumeLayout();
                return;
            }

            if (controllers.Count() > 0)
            {
                listView1.Groups.Add(groupControllers);

                foreach (var controller in controllers)
                {
                    listView1.Items.Add(new ListViewItem()
                    {
                        Text = controller.Name == null ? controller.Address : $"{controller.Name} ({controller.Address})",
                        Group = groupControllers
                    });
                }
            }

            if (pairedDevices.Count() > 0)
            {
                listView1.Groups.Add(groupPaired);

                foreach (var device in pairedDevices)
                {
                    listView1.Items.Add(new ListViewItem()
                    {
                        Text = device.Name == null ? device.Address : $"{device.Name} ({device.Address})",
                        Group = groupPaired,
                        Tag = new MenuItemTag()
                        {
                            Device = device,
                            Control = control,
                            Handler = Unpair_Click
                        }
                    });
                }
            }

            if (detectedDevices.Count() > 0)
            {
                listView1.Groups.Add(groupDetected);

                foreach (var device in detectedDevices)
                {
                    listView1.Items.Add(new ListViewItem()
                    {
                        Text = device.Name == null ? device.Address : $"{device.Name} ({device.Address})",
                        Group = groupDetected,
                        Tag = new MenuItemTag()
                        {
                            Device = device,
                            Control = control,
                            Handler = Pair_Click
                        }
                    });
                }
            }

            if (trustedDevices.Count() > 0)
            {
                listView1.Groups.Add(groupTrusted);

                foreach (var device in trustedDevices)
                {
                    listView1.Items.Add(new ListViewItem()
                    {
                        Text = device.Name == null ? device.Address : $"{device.Name} ({device.Address})",
                        Group = groupTrusted,
                        Tag = new MenuItemTag()
                        {
                            Device = device,
                            Control = control,
                            Handler = Unpair_Click
                        }
                    });
                }
            }

            if (devices.Count() > 0)
            {
                listView1.Groups.Add(groupManagement);
                listView1.Items.Add(new ListViewItem()
                {
                    Text = "Remove all devices",
                    Group = groupManagement,
                    Tag = new MenuItemTag()
                    {
                        Handler = RemoveAll_Click
                    }
                });
            }
            listView1.ResumeLayout();
        }

        private void RemoveAll_Click(object sender, EventArgs e)
        {
            control.RemoveAllDevices();
        }
        private void Pair_Click(object sender, EventArgs e)
        {
            var tag = (MenuItemTag)(((ListViewItem)sender).Tag);

            tag.Device.Trusting = true;
            tag.Device.Pairing = false;
            tag.Control.TrustDevice(tag.Device.Address);
        }

        private void Unpair_Click(object sender, EventArgs e)
        {
            var tag = (MenuItemTag)(((ListViewItem)sender).Tag);

            tag.Control.DisconnectDevice(tag.Device.Address);
            tag.Control.UntrustDevice(tag.Device.Address);
            tag.Control.RemoveDevice(tag.Device.Address);
        }

        private void labelByLine_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "https://github.com/DanTheMan827",
                UseShellExecute = true
            });
        }

        private void labelInfo_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "https://github.com/DanTheMan827/BlueTool",
                UseShellExecute = true
            });
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = listView1.SelectedItems.Count > 0 ? listView1.SelectedItems[0] : null;

            if (selected != null)
            {
                if (selected.Tag is MenuItemTag && ((MenuItemTag)selected.Tag).Handler != null)
                {
                    listView1.Enabled = false;
                    (((MenuItemTag)selected.Tag).Handler)(selected, e);
                    listView1.Enabled = true;
                }
                selected.Selected = false;
            }
        }

        private void textBoxAddress_TextChanged(object sender, EventArgs e)
        {
            buttonConnection.Enabled = ParseAddress(textBoxAddress.Text) != null;
            Settings.Default.Address = textBoxAddress.Text;
            Settings.Default.Save();
        }
    }
}

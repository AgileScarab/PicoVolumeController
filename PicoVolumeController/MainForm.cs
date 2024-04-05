using CoreAudio;
using PicoVolumeController.Models;
using PicoVolumeController.Services;
using PicoVolumeController.Utils;
using PicoVolumeController.Win32;

namespace PicoVolumeController
{
    public partial class MainForm : Form
    {
        private ForegroundWindowTracker windowTracker;

        private readonly AudioSessionService sessionService;
        private readonly SerialPortService serialPortService = new();

        private readonly List<string>? processGroup1;
        private readonly List<string>? processGroup2;
        private string currentActiveProcess = "";

        private Dictionary<string, AudioSessionControl2> mAudioSessions = new();

        private readonly SettingsManager settingsManager;

        private readonly System.Windows.Forms.Timer CheckTimer = new();

        private bool isProcessingData = false;
        private bool running = false;
        public MainForm()
        {

            InitializeComponent();

            this.Icon = new Icon(Path.Combine(Application.StartupPath, "window.ico"));

            windowTracker = new ForegroundWindowTracker();
            windowTracker.ForegroundProcessChanged += ForegroundProcessChanged;
            windowTracker.Start();

            sessionService = new AudioSessionService();
            settingsManager = new SettingsManager("settings.json");
            //definitely a better way of doing this but i cba
            try
            {
                settingsManager = new SettingsManager("settings.json");
            }
            catch (Exception ex)
            {
                var result = MessageBox.Show("Could not load settings");
                if (result == DialogResult.OK || result == DialogResult.None)
                {
                    Application.Exit();
                }
            }
            if (settingsManager.Settings.group1 == null || settingsManager.Settings.group1 == null)
            {
                var result = MessageBox.Show("Could not load settings");
                if (result == DialogResult.OK || result == DialogResult.None)
                {
                    Application.Exit();
                }
            }

            processGroup1 = settingsManager.Settings.group1;
            processGroup2 = settingsManager.Settings.group2;

            processGroup1Box.DataSource = processGroup1;
            processGroup2Box.DataSource = processGroup2;

            mAudioSessions = sessionService.GetAudioSessions();

            CheckTimer.Interval = 500;
            CheckTimer.Tick += CheckTimer_Tick;
            CheckTimer.Start();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            comTextBox.Text = settingsManager.Settings.port;
        }

        private void CheckTimer_Tick(object? sender, EventArgs e)
        {
            mAudioSessions = sessionService.GetAudioSessions();
        }

        private void ForegroundProcessChanged(object sender, string e)
        {
            currentActiveProcess = e;
        }

        private void SerialPortService_DataReceived(object? sender, SerialData e)
        {

            if (isProcessingData)
            {
                return;
            }
            isProcessingData = true;

            if (e == null)
            {
                return;
            }
            float step = 0;

            switch (e.Action)
            {

                case EncoderAction.Increase:
                    step = 5.0f;
                    break;
                case EncoderAction.Decrease:
                    step = -5.0f;
                    break;
                case EncoderAction.Mute:
                    step = 0;
                    break;
            }

            if (e.Encoder == 1)
            {
                if (step != 0)
                {
                    sessionService.StepMasterVolume(step);
                }
                else
                {
                    sessionService.MuteUnmuteMasterVolume();
                }
            }

            if (e.Encoder == 2)
            {
                AudioSessionControl2? session = null;
                foreach (var processName in processGroup1)
                {
                    if (mAudioSessions.ContainsKey(processName))
                    {
                        session = mAudioSessions[processName];
                        if (session.State != AudioSessionState.AudioSessionStateActive) //shouldn't be necessary but for some reason is 
                        {
                            continue;
                        }
                        break;
                    }
                }
                if (session != null)
                {
                    if (step != 0)
                    {
                        AudioSessionService.StepSessionVolume(session, step);
                    }
                    else
                    {
                        AudioSessionService.MuteUnmuteSessionVolume(session);
                    }
                }

            }
            if (e.Encoder == 3)
            {

                AudioSessionControl2? session = null;
                if (foregroundCheckBox.Checked)
                {
                    if (mAudioSessions.ContainsKey(currentActiveProcess))
                    {
                        if (mAudioSessions[currentActiveProcess].State == AudioSessionState.AudioSessionStateActive) //actually may be a good idea here so we dont try to change audio on an inactive state
                        {
                            session = mAudioSessions[currentActiveProcess];
                        }
                    }
                }
                else
                {
                    foreach (var processName in processGroup2)
                    {
                        if (mAudioSessions.ContainsKey(processName))
                        {
                            session = mAudioSessions[processName];
                            if (session.State != AudioSessionState.AudioSessionStateActive) //shouldn't be necessary but for some reason is 
                            {
                                continue;
                            }
                            break;
                        }
                    }
                }

                if (session != null)
                {
                    if (step != 0)
                    {
                        AudioSessionService.StepSessionVolume(session, step);

                    }
                    else
                    {
                        AudioSessionService.MuteUnmuteSessionVolume(session);
                    }
                }

            }
            isProcessingData = false;
        }
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CheckTimer.Stop();
            windowTracker.Stop();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                trayIcon.Visible = true;
            }
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            trayIcon.Visible = false;
        }


        private void startButton_Click(object sender, EventArgs e)
        {
            var portName = comTextBox.Text.Trim();
            if (portName != null && !running)
            {
                running = true;
                if (!CheckTimer.Enabled)
                    CheckTimer.Start();
                serialPortService.Initialize(portName, 115200);
                serialPortService.DataReceived += SerialPortService_DataReceived;
                startButton.Text = "Stop";
            }
            else if (running)
            {
                running = false;
                if (CheckTimer.Enabled)
                    CheckTimer.Stop();
                serialPortService.DataReceived -= SerialPortService_DataReceived;
                serialPortService.Close();
                serialPortService.Dispose();
                startButton.Text = "Start";
            }
        }
    }
}

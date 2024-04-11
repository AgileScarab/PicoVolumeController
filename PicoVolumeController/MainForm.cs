using CoreAudio;
using PicoVolumeController.Models;
using PicoVolumeController.Services;
using PicoVolumeController.Utils;
using PicoVolumeController.Win32;
using System.Linq;
using System.Windows.Forms;

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

        private Dictionary<string, HashSet<AudioSessionControl2>> mAudioSessions = new();

        private readonly SettingsManager settingsManager;

        private readonly System.Windows.Forms.Timer CheckTimer = new();

        private bool isProcessingData = false;
        private bool running = false;
        public MainForm()
        {

            windowTracker = new ForegroundWindowTracker();
            windowTracker.ForegroundProcessChanged += ForegroundProcessChanged;
            windowTracker.Start();

            sessionService = new AudioSessionService();
            //there is definitely a better way of doing this but im the only one using it so i do not care
            try
            {
                settingsManager = new SettingsManager("settings.json");
            }
            catch (Exception)
            {
                MessageBox.Show("Could not load settings");
                Environment.Exit(1);
            }
            if (settingsManager.Settings == null || settingsManager.Settings == null)
            {
                MessageBox.Show("Settings file is not in formatted correctly");
                Environment.Exit(1);
            }
            else
            {
                InitializeComponent();
                this.Icon = new Icon(Path.Combine(Application.StartupPath, "window.ico"));
                processGroup1 = settingsManager.Settings.group1;
                processGroup2 = settingsManager.Settings.group2;

                processGroup1Box.DataSource = processGroup1;
                processGroup2Box.DataSource = processGroup2;

                mAudioSessions = sessionService.GetAudioSessions();

                CheckTimer.Interval = 500;
                CheckTimer.Tick += CheckTimer_Tick;
                CheckTimer.Start();
            }
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
            ProcessEncoder(e.Encoder, step);
            isProcessingData = false;
        }
        private HashSet<AudioSessionControl2>? GetActiveSessionList(List<string> processGroup, bool foreGround)
        {
            if (foreGround == true)
            {
                if (mAudioSessions.ContainsKey(currentActiveProcess))
                {
                    foreach (var aSession in mAudioSessions[currentActiveProcess])
                    {
                        if (aSession.State == AudioSessionState.AudioSessionStateActive)
                        {
                            return mAudioSessions[currentActiveProcess];
                        }
                    }
                }
            }
            foreach (var processName in processGroup)
            {
                if (mAudioSessions.ContainsKey(processName))
                {
                    foreach (var session in mAudioSessions[processName])
                    {
                        if (session.State == AudioSessionState.AudioSessionStateActive)
                        {
                            return mAudioSessions[processName];
                        }
                    }
                }
            }
            return null;
        }
        private void ProcessEncoder(int encoder, float step)
        {
            HashSet<AudioSessionControl2>? sessionList = null;
            switch (encoder)
            {
                case 1:
                    if (step != 0)
                    {
                        sessionService.StepMasterVolume(step);
                    }
                    else
                    {
                        sessionService.MuteUnmuteMasterVolume();
                    }
                    break;
                case 2:
                    sessionList = GetActiveSessionList(processGroup1, false);
                    break;
                case 3:
                    if (foregroundCheckBox.Checked)
                    {
                        sessionList = GetActiveSessionList(processGroup2, true);
                    }
                    else
                    {
                        sessionList = GetActiveSessionList(processGroup2, false);
                    }
                    break;
            }

            if (sessionList != null)
            {
                foreach (var session in sessionList)
                {
                    if (step != 0)
                    {
                        AudioSessionService.StepSessionVolume(session, step);
                        if (debugCheck.Checked)
                        {
                            debugRichTextBox.BeginInvoke(new Action(() =>
                            {
                                debugRichTextBox.AppendText($"Stepping volume for {session.DisplayName} exe: {AudioSessionService.GetProcess(session)?.ProcessName}{Environment.NewLine}");
                            }));
                        }
                    }
                    else
                    {
                        AudioSessionService.MuteUnmuteSessionVolume(session);
                    }
                }
            }
            else
            {
                if(debugCheck.Checked)
                {
                    debugRichTextBox.BeginInvoke(new Action(() =>
                    {
                        debugRichTextBox.AppendText($"Sessions was null{Environment.NewLine}");
                    }));
                }

            }
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
                try
                {                    
                    if (!CheckTimer.Enabled)
                        CheckTimer.Start();
                    serialPortService.Initialize(portName, 115200);
                    serialPortService.DataReceived += SerialPortService_DataReceived;
                    startButton.Text = "Stop";
                    running = true;
                }
                catch(FileNotFoundException)
                {
                    MessageBox.Show($"{portName} not found");
                }
                catch(UnauthorizedAccessException)
                {
                    MessageBox.Show($"{portName} is busy");
                }
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

        private void button1_Click(object sender, EventArgs e)
        {
            debugRichTextBox.Clear();
        }
    }
}

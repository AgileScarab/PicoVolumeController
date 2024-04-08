using CoreAudio;
using CoreAudio.Interfaces;
using System.Diagnostics;

namespace PicoVolumeController.Services
{
    public class AudioSessionService
    {
        private readonly MMDeviceEnumerator _enumerator;
        private MMDeviceCollection devices;
        private Dictionary<string, HashSet<AudioSessionControl2>> sessions;
        public AudioSessionService()
        {
            _enumerator = new MMDeviceEnumerator(Guid.Empty);
            sessions = new Dictionary<string, HashSet<AudioSessionControl2>>();
            devices = _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            foreach (var device in devices)
            {
#pragma warning disable CA1806
                devices.Append(device);
#pragma warning restore CA1806
                if (device.AudioSessionManager2 != null)
                {
                    device.AudioSessionManager2.OnSessionCreated += HandleSessionCreated;
                }
                if (device.AudioSessionManager2?.Sessions != null)
                {
                    foreach (var session in device.AudioSessionManager2.Sessions)
                    {
                        var processname = Process.GetProcessById((int)session.ProcessID).ProcessName;
                        if (!sessions.ContainsKey(processname))
                        {
                            sessions[processname] = new HashSet<AudioSessionControl2>();
                        }
                        sessions[processname].Add(session);
                        session.OnStateChanged += HandleSessionStateChanged;
                    }
                }
            }
        }

        private void HandleSessionStateChanged(object sender, AudioSessionState newState)
        {
            AudioSessionControl2 session = (AudioSessionControl2)sender;
            try
            {
                string processName = Process.GetProcessById((int)session.ProcessID).ProcessName;
                if (string.IsNullOrEmpty(processName))
                    return;
                switch (newState)
                {
                    case AudioSessionState.AudioSessionStateInactive:
                    case AudioSessionState.AudioSessionStateExpired:
                        if (sessions.ContainsKey(processName))
                        {
                            sessions[processName].Remove(session);
                        }
                        break;
                    case AudioSessionState.AudioSessionStateActive:
                        if (sessions.ContainsKey(processName))
                        {
                            sessions[processName].Add(session);
                        }
                        else if(!sessions.ContainsKey(processName))
                        {
                            sessions[processName] = new HashSet<AudioSessionControl2> { session};
                        }
                        break;
                    default:
                        break;
                }
            }
            catch
            {
                foreach(var key in sessions.Keys)
                {
                    switch(newState)
                    {
                        case AudioSessionState.AudioSessionStateInactive:
                        case AudioSessionState.AudioSessionStateExpired:
                            sessions[key].Remove(session);
                            break;
                    }
                }
            }
        }

        private void HandleSessionCreated(object sender, IAudioSessionControl2 newSession)
        {
            AudioSessionManager2 sessionManager = (AudioSessionManager2)sender;
            newSession.GetProcessId(out uint newSessionId);
            
            sessionManager.RefreshSessions();
            if (sessionManager.Sessions == null)
                return;
            foreach (var session in sessionManager.Sessions)
            {
                if (session.ProcessID == newSessionId)
                {
                    var processName = Process.GetProcessById((int)session.ProcessID).ProcessName;
                    if(!sessions.ContainsKey(processName))
                    {
                        sessions[processName] = new HashSet<AudioSessionControl2>();
                    }
                    sessions[processName].Add(session);
                    session.OnStateChanged += HandleSessionStateChanged;
                }
            }

        }

        public Dictionary<string, HashSet<AudioSessionControl2>> GetAudioSessions()
        {
            return sessions;
        }
        public static Process? GetProcess(AudioSessionControl2 session)
        {
            int processId = (int)session.ProcessID;
            try
            {
                return Process.GetProcessById(processId);
            }
            catch
            {
                return null;
            }
        }

        public static void StepSessionVolume(AudioSessionControl2 session, float step)
        {
            SimpleAudioVolume? vol = session.SimpleAudioVolume;
            if (vol == null)
                return;
            float currentVolume = vol.MasterVolume * 100;
            float newVolume = Math.Max(0, Math.Min(100, currentVolume + step));
            vol.MasterVolume = newVolume / 100;
        }


        public static void MuteUnmuteSessionVolume(AudioSessionControl2 session)
        {
            SimpleAudioVolume? vol = session.SimpleAudioVolume;
            if (vol == null)
                return;
            if (!vol.Mute)
                vol.Mute = true;
            else
                vol.Mute = false;
        }

        public void MuteUnmuteMasterVolume()
        {
            foreach (MMDevice device in devices)
            {
                if (device.AudioEndpointVolume != null)
                {
                    if (!device.AudioEndpointVolume.Mute)
                        device.AudioEndpointVolume.Mute = true;
                    else
                        device.AudioEndpointVolume.Mute = false;
                }
            }
        }

        public void StepMasterVolume(float step)
        {
            foreach (MMDevice device in devices)
            {
                if (device.AudioEndpointVolume != null)
                {
                    float currentVolume = device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
                    float newVolume = Math.Max(0, Math.Min(100, currentVolume + step));
                    device.AudioEndpointVolume.MasterVolumeLevelScalar = newVolume / 100;
                }
            }
        }
    }
}

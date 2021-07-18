using System;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Ssh;
using Crestron.SimplSharp.Ssh.Common;

namespace VirtualControlBeta
{
    public class DataEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    public class CiscoCodec
    {
        private SshClient _sshClient;
        private ShellStream _stream;

        public string cxHost { get; set; }
        public string cxUser { get; set; }
        public string cxPass { get; set; }

        public event EventHandler OnConnect;
        public event EventHandler OnDisconnect;
        public event EventHandler<DataEventArgs> OnDataReceived;

        public CiscoCodec(SshClient sshClient)
        {
            _sshClient = sshClient;
        }

        public void Connect()
        {
            var auth = new KeyboardInteractiveAuthenticationMethod(cxUser);
            auth.AuthenticationPrompt += HandleAuthentication;

            var conn = new ConnectionInfo(cxHost, cxUser, auth);

            _sshClient = new SshClient(conn);
            _sshClient.ErrorOccurred += ErrorOccurred;
            _sshClient.Connect();

            _stream = _sshClient.CreateShellStream("Terminal", 80, 24, 800, 600, 1024);
            _stream.DataReceived += _stream_DataReceived;
            _stream.ErrorOccurred += _stream_ErrorOccurred;

            if (OnConnect != null)
            {
                OnConnect(this, new EventArgs());
            }
        }

        public void Disconnect()
        {
            if (_sshClient != null)
            {
                if (_stream != null)
                {
                    _stream.Close();
                    _stream.Dispose();
                }

                _sshClient.Disconnect();
                _sshClient.Dispose();
                _sshClient = null;

                if (OnDisconnect != null)
                {
                    OnDisconnect(this, new EventArgs());
                }
            }
        }

        private void HandleAuthentication(object sender, AuthenticationPromptEventArgs args)
        {
            foreach (var prompt in args.Prompts)
            {
                if (prompt.Request.Contains("Password:"))
                    prompt.Response = cxPass;
            }
        }

        private void ErrorOccurred(object sender, ExceptionEventArgs e)
        {
            if (!_sshClient.IsConnected)
            {
                if (OnDisconnect != null)
                {
                    OnDisconnect(this, new EventArgs());
                }
            }
        }

        private void _stream_DataReceived(object sender, ShellDataEventArgs e)
        {
            var stream = (ShellStream)sender;
            string data = "";

            while (stream.DataAvailable)
            {
                data += stream.Read();
            }

            if (data != "")
            {
                if (OnDataReceived != null)
                {
                    OnDataReceived(this, new DataEventArgs() { Message = data });
                }
            }
        }

        private void _stream_ErrorOccurred(object sender, ExceptionEventArgs e)
        {
            Disconnect();
        }

        private void SendCommand(string cmd)
        {
            if (_sshClient.IsConnected && _stream.CanWrite)
            {
                _stream.WriteLine(cmd);
            }
        }
    }
}

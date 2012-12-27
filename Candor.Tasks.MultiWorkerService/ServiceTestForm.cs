using System;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using System.Timers;
using System.Threading;
using System.IO;
using System.Security;

namespace Candor.Tasks.MultiWorkerService
{
#if DEBUG
    public partial class ServiceTestForm : Form
    {
        ServiceBase service = null;

        public ServiceTestForm()
        {
            InitializeComponent();
        }

        #region State
        /// <summary>
        /// Since we can't have a ServiceController,
        /// determine our own state.
        /// </summary>
        enum State
        {
            Stopped,
            Running,
            Paused
        }
        State state = State.Stopped;
        /// <summary>
        /// Gets or sets service state. Allows start/stop/pause.
        /// </summary>
        State ServState
        {
            get { return state; }
            set
            {
                switch (value)
                {
                    case State.Paused:
                        if (state == State.Running)
                            InvokeServiceMember("OnPause");
                        else
                        {
                            pause.Enabled = false;
                            throw new ApplicationException("Can't pause unless running.");
                        }
                        break;
                    case State.Running:
                        if (state == State.Stopped)
                            InvokeServiceMember("OnStart", new string[] { "" });
                        else if (state == State.Paused)
                            InvokeServiceMember("OnContinue");
                        else
                            throw new ApplicationException("Can't start unless stopped.");
                        pause.Text = "Pause";
                        break;
                    case State.Stopped:
                        InvokeServiceMember("OnStop");
                        break;
                }
                state = value;
            }
        }
        #endregion

        /// <summary>
        /// Create a test form for the given service.
        /// </summary>
        /// <param name="serv"> Instance of a ServiceBase derivation. </param>
        public ServiceTestForm(ServiceBase serv)
        {
            service = serv;
            InitializeComponent();
        }
        void InvokeServiceMember(string name)
        {
            InvokeServiceMember(name, null);
        }
        void InvokeServiceMember(string name, object args)
        {
            InvokeServiceMember(name, new object[] { args });
        }
        void InvokeServiceMember(string name, object[] args)
        {
            Type serviceType = service.GetType();
            serviceType.InvokeMember(name,
            System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.InvokeMethod
            | System.Reflection.BindingFlags.NonPublic
            | System.Reflection.BindingFlags.Public,
            null,
            service,
            new object[] { args });
        }

        #region Event Handlers
        private void start_Click(object sender, EventArgs e)
        {
            try
            {
                this.Enabled = false;
                ServState = State.Running;
                output.Text = "Started";
                start.Enabled = false;
                stop.Enabled = true;
                pause.Enabled = true;
            }
            catch (Exception ex)
            {
                output.Text = ex.Message + "\r\n";
            }
            finally
            {
                this.Enabled = true;
            }
        }
        private void stop_Click(object sender, EventArgs e)
        {
            try
            {
                this.Enabled = false;
                ServState = State.Stopped;
                output.Text = "Stopped";
                start.Enabled = true;
                stop.Enabled = false;
                pause.Enabled = false;
            }
            catch (Exception ex)
            {
                output.Text = ex.Message + "\r\n";
            }
            finally
            {
                this.Enabled = true;
            }
        }
        private void pause_Click(object sender, EventArgs e)
        {
            try
            {
                this.Enabled = false;
                if (ServState == State.Paused)
                {
                    ServState = State.Running;
                    pause.Text = "Pause";
                    output.Text = "Resumed";
                }
                else if (ServState == State.Running)
                {
                    ServState = State.Paused;
                    pause.Text = "Continue";
                    output.Text = "Paused";
                }
                stop.Enabled = true;
                start.Enabled = false;
            }
            catch (Exception ex)
            {
                output.Text = ex.Message + "\r\n";
            }
            finally
            {
                this.Enabled = true;
            }
        }
        private void sendCommand_Click(object sender, EventArgs e)
        {
            try
            {
                InvokeServiceMember("OnCustomCommand", (int)command.Value);
            }
            catch (Exception ex)
            {
                output.Text = ex.Message + "\r\n";
            }
        }
        #endregion
    }
#endif
}

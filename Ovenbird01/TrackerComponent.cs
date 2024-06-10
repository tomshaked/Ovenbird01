using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using NatNetML;
using Rhino.Geometry;
using System.Timers;
using Ovenbird01.Properties;

namespace Ovenbird
{
    public class SimpleNatNetListenerComponent : GH_Component
    {
        private NatNetClientML mNatNet;
        private string latestData;
        private List<string> statusLog;
        private bool isConnected;
        private Timer updateTimer;
        private bool dataAvailable;
        private int currentInterval;

        public SimpleNatNetListenerComponent()
          : base("SimpleNatNetListener", "NatNet Listener",
              "Listens for data from OptiTrack NatNet server",
              "Ovenbird", "Communication")
        {
            statusLog = new List<string>();
            latestData = string.Empty;
            isConnected = false;
            dataAvailable = false;

            // Default interval
            currentInterval = 1000;
            updateTimer = new Timer(currentInterval);
            updateTimer.Elapsed += OnTimedEvent;
            updateTimer.AutoReset = true;
            updateTimer.Enabled = true;
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Activate", "Activate", "Activate the NatNet listener.", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Local IP", "Local IP", "IP address for the receiver.", GH_ParamAccess.item, "127.0.0.1");
            pManager.AddTextParameter("Server IP", "Server IP", "IP address for the server.", GH_ParamAccess.item, "127.0.0.1");
            pManager.AddIntegerParameter("Sample Rate", "Sample Rate", "Sampling rate (1: 1s, 2: 500ms, 3: 200ms, 4: 100ms, 5: 50ms)", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Status", "Status", "Status of the NatNet listener.", GH_ParamAccess.list);
            pManager.AddTextParameter("Data", "Data", "Latest NatNet data", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool activate = false;
            string localIP = "127.0.0.1";
            string serverIP = "127.0.0.1";
            int sampleRate = 1;

            if (!DA.GetData(0, ref activate)) return;
            if (!DA.GetData(1, ref localIP)) return;
            if (!DA.GetData(2, ref serverIP)) return;
            if (!DA.GetData(3, ref sampleRate)) return;

            int interval = GetIntervalFromSampleRate(sampleRate);
            if (interval != currentInterval)
            {
                updateTimer.Interval = interval;
                currentInterval = interval;
            }

            if (activate && !isConnected)
            {
                StartListening(localIP, serverIP);
            }
            else if (!activate && isConnected)
            {
                StopListening();
            }

            DA.SetDataList(0, statusLog);
            DA.SetData(1, latestData);
        }

        private int GetIntervalFromSampleRate(int sampleRate)
        {
            switch (sampleRate)
            {
                case 1: return 1000; // 1 second
                case 2: return 500;  // 500 milliseconds
                case 3: return 200;  // 200 milliseconds
                case 4: return 100;  // 100 milliseconds
                case 5: return 50;   // 50 milliseconds
                default: return 1000;
            }
        }

        private void StartListening(string localIP, string serverIP)
        {
            statusLog.Clear();
            statusLog.Add("Connecting to NatNet server...");

            mNatNet = new NatNetClientML(0); // Initialize the NatNet client with multicast connection type (0)
            mNatNet.OnFrameReady += OnFrameReady;

            int result = mNatNet.Initialize(localIP, serverIP);
            if (result == 0)
            {
                isConnected = true;
                statusLog.Add("Connected.");
                updateTimer.Start();
            }
            else
            {
                statusLog.Add("Connection failed with error code " + result);
            }
        }

        private void StopListening()
        {
            if (isConnected)
            {
                updateTimer.Stop();
                mNatNet.Uninitialize();
                isConnected = false;
                statusLog.Add("Disconnected.");
            }
        }

        private void OnFrameReady(FrameOfMocapData data, NatNetClientML client)
        {
            if (data.nRigidBodies > 0)
            {
                var rigidBody = data.RigidBodies[0]; // Only take the first rigid body data to minimize processing
                string dataString = $"RigidBody {rigidBody.ID}: pos=({rigidBody.x}, {rigidBody.y}, {rigidBody.z}) rot=({rigidBody.qx}, {rigidBody.qy}, {rigidBody.qz}, {rigidBody.qw})";
                latestData = dataString;
                dataAvailable = true;
            }
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (dataAvailable)
            {
                dataAvailable = false;
                Rhino.RhinoApp.InvokeOnUiThread((Action)delegate
                {
                    ExpireSolution(true);
                });
            }
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            updateTimer.Stop();
            StopListening();
            base.RemovedFromDocument(document);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Resources.rigidbody;
            }
        }

        public override Guid ComponentGuid => new Guid("F8A1F091-4A84-4D5C-B3E4-2C8B5A9BDA1E");
    }
}

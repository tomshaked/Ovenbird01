using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Grasshopper.Kernel;

namespace Ovenbird
{
    public class UDPListenerComponent : GH_Component
    {
        private UdpClient udpClient;
        private Thread listenerThread;
        private string latestData;
        private List<string> statusLog;
        private bool isListening;

        public UDPListenerComponent()
          : base("UDPListener", "UDP Listener",
              "Listens for UDP messages",
              "Ovenbird", "Communication")
        {
            statusLog = new List<string>();
            latestData = string.Empty;
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Activate", "Activate", "Activate the UDP listener.", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Status", "Status", "Status of the UDP listener.", GH_ParamAccess.list);
            pManager.AddTextParameter("Data", "Data", "Latest UDP data", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool activate = false;
            if (!DA.GetData(0, ref activate)) return;

            if (activate && !isListening)
            {
                StartListening();
            }
            else if (!activate && isListening)
            {
                StopListening();
            }

            DA.SetDataList(0, statusLog);
            DA.SetData(1, latestData);
        }

        private void StartListening()
        {
            statusLog.Clear();
            statusLog.Add("Starting UDP listener...");

            udpClient = new UdpClient(1511);
            isListening = true;

            listenerThread = new Thread(new ThreadStart(ListenForData));
            listenerThread.IsBackground = true;
            listenerThread.Start();

            statusLog.Add("UDP listener started.");
        }

        private void StopListening()
        {
            statusLog.Add("Stopping UDP listener...");

            isListening = false;
            udpClient.Close();
            listenerThread.Join();

            statusLog.Add("UDP listener stopped.");
        }

        private void ListenForData()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 1511);

            try
            {
                while (isListening)
                {
                    byte[] receiveBytes = udpClient.Receive(ref remoteEndPoint);
                    string receiveString = Encoding.ASCII.GetString(receiveBytes);

                    lock (latestData)
                    {
                        latestData = receiveString;
                    }

                    Rhino.RhinoApp.InvokeOnUiThread((Action)delegate
                    {
                        ExpireSolution(true);
                    });
                }
            }
            catch (Exception ex)
            {
                statusLog.Add("Error: " + ex.Message);
            }
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("F8A1F091-4A84-4D5C-B3E4-2C8B5A9BDA1E");
    }
}

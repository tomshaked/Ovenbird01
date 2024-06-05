using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using NatNetML;
using Rhino.Geometry;

namespace Ovenbird01
{
    public class TrackerComponent : GH_Component
    {
        // NatNet client and data structures
        private static NatNetClientML mNatNet;
        private static string mStrLocalIP = "127.0.0.1";   // Local IP address
        private static string mStrServerIP = "127.0.0.1";  // Server IP address
        private static ConnectionType mConnectionType = ConnectionType.Multicast;
        private static List<RigidBody> mRigidBodies = new List<RigidBody>();
        private static bool mAssetChanged = false;
        private static List<Point3d> rbPositions = new List<Point3d>();
        private static bool dataReceived = false;

        public TrackerComponent()
          : base("Ovenbird01", "Tracker",
            "Motive frame data broadcast",
            "Ovenbird", "OptiTrack")
        {
            InitializeNatNet();
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            // Register any input parameters if needed
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("RigidBody Positions", "RBPos", "Positions of tracked rigid bodies", GH_ParamAccess.list);
            pManager.AddTextParameter("Status", "Status", "Connection status", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (mNatNet == null || mAssetChanged)
            {
                ConnectToServer(mStrServerIP, mStrLocalIP, mConnectionType);
                FetchDataDescriptor();
            }

            if (dataReceived)
            {
                DA.SetDataList(0, rbPositions);
                DA.SetData(1, "Connected to server");
                dataReceived = false;
            }
            else
            {
                DA.SetDataList(0, new List<Point3d>());
                DA.SetData(1, "No data received");
            }
        }

        private void InitializeNatNet()
        {
            mNatNet = new NatNetClientML();
            mNatNet.OnFrameReady += FetchFrameData;
        }

        private void ConnectToServer(string serverIPAddress, string localIPAddress, ConnectionType connectionType)
        {
            NatNetClientML.ConnectParams connectParams = new NatNetClientML.ConnectParams
            {
                ConnectionType = connectionType,
                ServerAddress = serverIPAddress,
                LocalAddress = localIPAddress
            };

            mNatNet.Connect(connectParams);
        }

        private void FetchFrameData(FrameOfMocapData data, NatNetClientML client)
        {
            if (data.bTrackingModelsChanged || data.nRigidBodies != mRigidBodies.Count)
            {
                mAssetChanged = true;
            }

            rbPositions.Clear();
            foreach (var rb in mRigidBodies)
            {
                foreach (var rbData in data.RigidBodies)
                {
                    if (rb.ID == rbData.ID && rbData.Tracked)
                    {
                        rbPositions.Add(new Point3d(rbData.x, rbData.y, rbData.z));
                    }
                }
            }
            dataReceived = true;
        }

        private void FetchDataDescriptor()
        {
            List<DataDescriptor> dataDescriptors;
            mNatNet.GetDataDescriptions(out dataDescriptors);

            foreach (var descriptor in dataDescriptors)
            {
                if (descriptor.type == (int)DataDescriptorType.eRigidbodyData)
                {
                    mRigidBodies.Add((RigidBody)descriptor);
                }
            }

            mAssetChanged = false;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Properties.Resources.rigidbody;
            }
        }

        public override Guid ComponentGuid => new Guid("8D4B7B98-77C6-4D8B-9CB1-AA364A938CCE");
    }
}

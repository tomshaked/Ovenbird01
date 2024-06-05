using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Ovenbird01
{
    public class TrackerInfo : GH_AssemblyInfo
    {
        public override string Name => "Tracker";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("49EAAFC7-85A7-4A01-8A42-8EB12942CE14");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}
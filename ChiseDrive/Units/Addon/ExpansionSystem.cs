using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChiseDrive.Units
{
    public class ExpansionSystem : System
    {
        public override string Name { get; set; }
        public override void Dispose() { }
        public override void Update(Time elapsed) 
        {
            foreach (Addon a in Addons) a.Update(elapsed);
        }

        static public ExpansionSystem TryBuildSystem(String name,
            List<AttachmentPoint> attachments)
        {
            ExpansionSystem system = new ExpansionSystem();
            for (int i = 0; i < 30; i++)
            {
                for (int j = 0; j < attachments.Count; j++)
                {
                    string testname = name;
                    if (i > 0) testname += i;

                    if (attachments[j].Name == testname)
                    {
                        system.Addons.Add(new Addon(attachments[j]));
                    }
                }
            }
            if (system.Addons.Count > 0) return system;
            return null;
        }

        public List<Addon> Addons = new List<Addon>();

        ExpansionSystem()
        {
        }
    }
}
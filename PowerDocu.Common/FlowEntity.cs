using System.Collections.Generic;

namespace PowerDocu.Common
{
    public class FlowEntity
    {
        public string ID;
        public string Name;
        public string FileName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ID)) { return ID; }
                if (Name.Length < 40)
                {
                    return Name;
                }
                var nameWithOutUid = Name[..^37];
                return nameWithOutUid;
            }
        }

        public string Description;
        public Trigger trigger;
        public ActionGraph actions = new ActionGraph();
        public List<ConnectionReference> connectionReferences = new List<ConnectionReference>();

        public FlowEntity()
        {
        }

        public void addTrigger(string name)
        {
            trigger = new Trigger(name);
        }

        public override string ToString()
        {
            return "Flow " + Name + " (" + ID + ")";
        }
    }
}
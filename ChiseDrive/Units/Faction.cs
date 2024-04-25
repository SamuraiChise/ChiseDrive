using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChiseDrive.Units
{
    public enum Alignment
    {
        None,
        Neutral,
        Friendly,
        Hostile
    };
    
    public class Faction
    {
        Dictionary<string, Alignment> Alignments = new Dictionary<string, Alignment>();

        public string Name { get; set; }

        public Faction(string name)
        {
            Name = name;
        }

        public void SetAlignment(string key, Alignment value)
        {
            if (Alignments.ContainsKey(key)) Alignments[key] = value;
            else Alignments.Add(key, value);
        }
        public Alignment GetAlignment(Faction faction)
        {
            if (faction == null) return Alignment.None;
            if (Alignments.ContainsKey(faction.Name)) return Alignments[faction.Name];
            else return Alignment.Neutral;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
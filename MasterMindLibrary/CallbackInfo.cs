using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace MasterMindLibrary
{
    [DataContract]
    public class CallbackInfo
    {
        [DataMember] public List<Colors> correctSequence { get; private set; }
        [DataMember] public string name { get; private set; }
        [DataMember] public bool someoneWon { get; private set; }

        public CallbackInfo(List<Colors> c, string n, bool s = false)
        {
            correctSequence = c;
            name = n;
            someoneWon = s;
        }
    }
}

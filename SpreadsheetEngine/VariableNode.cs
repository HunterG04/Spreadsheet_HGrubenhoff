using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CptS322
{
    class VariableNode : Node
    {
        private string mVarName;

        public VariableNode(string varName) // value will be held in dictionary
        {
            mVarName = varName;
        }

        public string VarName
        {
            get { return mVarName; }
            set { mVarName = value; }
        }
    }
}

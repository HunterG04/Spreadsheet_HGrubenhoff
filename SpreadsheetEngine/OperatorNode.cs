using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CptS322
{
    class OperatorNode : Node
    {
        private Node mLeftNode = null;
        private Node mRightNode = null;
        private int mPrecedence = 0; //() (* /) (+ -)
        private string mValue;

        public OperatorNode(string value)
        {
            mValue = value; // operator
        }

        public Node LeftNode
        {
            get { return mLeftNode; }
            set { mLeftNode = value; }
        }
        public Node RightNode
        {
            get { return mRightNode; }
            set { mRightNode = value; }
        }
        public string Value
        {
            get { return mValue; }
            set { mValue = value; }
        }

        public int Precedence
        {
            get { return mPrecedence; }
        }
    }
}

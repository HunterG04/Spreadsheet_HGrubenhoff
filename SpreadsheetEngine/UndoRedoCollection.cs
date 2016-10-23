using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CptS322
{ 
    //base class to be inherited from depending on which command is being called (revert text, color, border, etc)
    public class UndoRedoCollection
    {
        protected string mTask; // holds actual task in english form for user compliance

        protected Stack<string> mPrevText = new Stack<string>();
        protected Stack<string> mCurText = new Stack<string>();

        protected Stack<int> mPrevColor = new Stack<int>();
        protected Stack<int> mCurColor = new Stack<int>();

        protected Stack<SpreadsheetCell> mPrevCell = new Stack<SpreadsheetCell>();
        protected Stack<SpreadsheetCell> mCurCell = new Stack<SpreadsheetCell>();

        public virtual void Undo() // virtual function to be implemented by the different types of commands (text, color, etc)
        {
            //will revert change back to previous state
        }
        public virtual void Redo()
        {
            //will change state of cell back to state before undo was called
        }

        public string Task
        {
            get { return mTask; }
        }
    }

    //class to restore a change in text
    public class RestoreText : UndoRedoCollection
    {
        //constructor
        public RestoreText(string task, Stack<string> prevText, Stack<string> curText, 
            Stack<SpreadsheetCell> prevCell, Stack<SpreadsheetCell> curCell)
        {
            mTask = task;

            mPrevText = prevText;
            mCurText = curText;

            mPrevCell = prevCell;
            mCurCell = curCell;
        }

        public override void Undo()
        {
            Stack<SpreadsheetCell> tempCell = new Stack<SpreadsheetCell>(mPrevCell); // create copy of previous cells
            Stack<string> tempText = new Stack<string>(mPrevText); // create copy of old text

            while (mPrevCell.Count > 0)
            {
                mPrevCell.Pop().Text = mPrevText.Pop(); // set cell text back to previous text
            }

            mPrevText = tempText; // save for redos
            mPrevCell = tempCell;
        }

        public override void Redo()
        {
            Stack<SpreadsheetCell> tempCell = new Stack<SpreadsheetCell>(mCurCell); // create copy of previous cells
            Stack<string> tempText = new Stack<string>(mCurText); // create copy of old text

            while (mCurCell.Count > 0)
            {
                mCurCell.Pop().Text = mCurText.Pop(); // set cell text back to previous text
            }

            mCurText = tempText; // save for redos
            mCurCell = tempCell;
        }
    }

    //class to restore background color change
    public class RestoreBG : UndoRedoCollection
    {
        //constructor
        public RestoreBG(string task, Stack<SpreadsheetCell> prevCell, Stack<SpreadsheetCell> curCell,
            Stack<int> prevColor, Stack<int> curColor)
        {
            mTask = task;

            mPrevCell = prevCell;
            mCurCell = curCell;

            mPrevColor = prevColor;
            mCurColor = curColor;
        }

        public override void Undo()
        {
            Stack<SpreadsheetCell> tempCell = new Stack<SpreadsheetCell>(mPrevCell); // create copy of previous cells
            Stack<int> tempCol = new Stack<int>(mPrevColor); // create copy of old colors

            while (mPrevCell.Count > 0)
            {
                if (mPrevColor.Peek() == 0) // check to see if color is orignal, unchanged white
                {
                    mPrevCell.Pop().BGColor = -1; // basic white color
                    mPrevColor.Pop();
                }
                else
                {
                    mPrevCell.Pop().BGColor = mPrevColor.Pop(); // set cell color back to old cell color
                }
                
            }

            mPrevColor = tempCol; // save for redos
            mPrevCell = tempCell;
        }

        public override void Redo()
        {
            Stack<SpreadsheetCell> tempCell = new Stack<SpreadsheetCell>(mPrevCell); // create copy of previous cells
            Stack<int> tempCol = new Stack<int>(mCurColor); // create copy of cur colors

            while (mCurCell.Count > 0)
            {
                mCurCell.Pop().BGColor = mCurColor.Pop(); // set cell text back to newer color
            }

            mCurColor = tempCol; // save for redos
            mCurCell = tempCell;
        }
    }
}

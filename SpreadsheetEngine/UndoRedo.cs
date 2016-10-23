using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CptS322
{
    public class UndoRedo
    {
        private Stack<UndoRedoCollection> mUndo = new Stack<UndoRedoCollection>();
        private Stack<UndoRedoCollection> mRedo = new Stack<UndoRedoCollection>();

        public void AddUndo(UndoRedoCollection item)
        {
            mUndo.Push(item);
            mRedo.Clear(); // clear redo since new things were inputted into undo stack
        }

        public UndoRedoCollection performUndo()
        {
            UndoRedoCollection temp = mUndo.Pop(); // pop top of undo stack
            mRedo.Push(temp); // add undo to redo
            return temp; //return top undo item to execute
        }

        public UndoRedoCollection performRedo() // pop off item at top of redo and push it onto undo
        {
            UndoRedoCollection temp = mRedo.Pop();
            mUndo.Push(temp);
            return temp; // return item that was on top of redo stack
        }

        public bool IsUndoEmpty() // true if undo stack is empty
        {
            if (mUndo.Count == 0)
            {
                return true;
            }

            return false;
        }

        public bool IsRedoEmpty()
        {
            if (mRedo.Count == 0)
            {
                return true;
            }

            return false;
        }

        public string GetUndoMessage() // returns top items text or task
        {
            UndoRedoCollection temp = mUndo.Peek();
            return temp.Task;
        }

        public string getRedoMessage() // same as above but for redo stack
        {
            UndoRedoCollection temp = mRedo.Peek();
            return temp.Task;
        }
    }
}

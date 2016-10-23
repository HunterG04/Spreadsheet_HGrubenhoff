using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpreadsheetNamespace;
using System.ComponentModel;
using CptS322;

/*
Name: Hunter Grubenhoff
ID: 11417582
CptS 322
*/

namespace NormalCellNamespace
{
    public class NormalCell : SpreadsheetCell
    {
        public NormalCell (int row, int column)
        {
            mText = "";
            mValue = "0";
            mRowIndex = row;
            mColumnIndex = column;
            string col;
            string numRow;
            col = GetColumnName(column); // converts column int to string A-Z
            numRow = (row + 1).ToString(); // converts number of row to string
            mName = col + numRow; // (A-Z)+(1-50)
            mTree = new ExpTree(mFormula, this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Threading.Tasks;
using System.IO;
using CptS322;
using System.ComponentModel;
using NormalCellNamespace;

/*
Name: Hunter Grubenhoff
ID: 11417582
CptS 322
*/

namespace SpreadsheetNamespace
{
    public class Spreadsheet
    {
        private SpreadsheetCell[,] Table; // 2D array that holds all the cells
        public Dictionary<string, HashSet<SpreadsheetCell>> refTable = new Dictionary<string, HashSet<SpreadsheetCell>>();
        //protected HashSet<SpreadsheetCell> mRefCells = new HashSet<SpreadsheetCell>();
        protected int mRowCount;
        protected int mColumnCount;
        private Random RanNum = new Random();
        private int RandRow = 0;
        private int RandCol = 0;

        protected UndoRedo mUndoRedo = new UndoRedo();

        //constructor
        public Spreadsheet(int rows, int columns)
        {
            Table = new SpreadsheetCell[rows, columns]; // creates array of x rows and y columns
            mRowCount = rows;
            mColumnCount = columns;

            for (int col = 0; col < mColumnCount; col++)
            {
                for (int row = 0; row < mRowCount; row++)
                {
                    Table[row, col] = new NormalCell(row, col); // initialize each cell
                    refTable.Add(Table[row, col].Name, new HashSet<SpreadsheetCell>());
                    Table[row, col].PropertyChanged += PropertyChangeOccur; // subscribe each cell to prop change
                }
            }
            
        }

        //property changed notifier
        public event PropertyChangedEventHandler CellPropertyChanged;

        private void NotifyCellPropertyChanged(object sender, string propertyName)
        {
            if (CellPropertyChanged != null)
            {
                CellPropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void PropertyChangeOccur (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Text")
            {
                SpreadsheetCell temp = (NormalCell)sender;

                if (temp.Text.Length != 0)
                {
                    if (temp.Text[0] == '=') //expression
                    {
                        temp.Formula = temp.Text.Substring(1);
                        
                        //update cell checks for self reference
                        updateCell(temp, true);

                    }
                    else // just a string
                    {
                        temp.setValue(temp.Text);
                        temp.Formula = temp.Text;
                        updateCell(temp, false);
                    }
                }
                else //empty string
                {
                    temp.setValue("");
                    temp.Formula = "";
                    updateCell(temp, false);
                }

                NotifyCellPropertyChanged(sender, "Value"); // fire off property changed event for text
            }
            else if (e.PropertyName == "BGColor")
            {
                NotifyCellPropertyChanged(sender, "BGColor"); // fire off property changed event for color
            }
        }

        //Returns cell from 2D array
        public SpreadsheetCell GetCell(int row, int column)
        {
            try
            {
                if (Table[row, column] != null)
                {
                    return Table[row, column]; //returns the cell at the given location
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private int RowCount
        {
            get { return RowCount; }
        }

        private int ColumnCount
        {
            get { return ColumnCount; }
        }

        public void updateCell(SpreadsheetCell cell, bool Formula)
        {
            if (cell.mTree != null)
            {
                if (cell.mTree.getVarNames() != null)
                {
                    foreach (string refed in cell.mTree.getVarNames())
                    {
                        try
                        {
                            refTable[refed].Remove(cell); // check if not bad reference
                        }
                        catch (Exception e)
                        {
                            break;
                        }
                    }
                }
            }
            ExpTree tree = null;
            if (cell.Text == "")
            {
                tree = new ExpTree(); // in case undo has to revert cell back to blank space
            }
            else
            {
                try
                {
                    if (Formula == true)
                    {
                        tree = new ExpTree(cell.Formula, cell);  // if random variables are entered or bad input
                    }
                    else
                    {
                        cell.setValue(cell.Formula);
                    }
                }
                catch(Exception e)
                {
                    if (Formula == false)
                    {
                        cell.setValue(cell.Text);
                    }
                    else
                    {
                        cell.setValue("!(BadInput)");   // display bad input if tree cant be built properly (bad var, weird symbols)
                        return;
                    }
                }
            }

            string[] vars = null;
            if (Formula == true)
            {
                vars = tree.getVarNames();
            }

                if (vars != null)
                {
                    foreach (string var in vars) // give values to variables
                    {
                        string row = var.Substring(1);
                        int col = ((int)var[0] - 65);

                        try
                        {
                            tree.SetVar(var, Convert.ToDouble(Table[Convert.ToInt32(row) - 1, col].Value));
                        }
                        catch (Exception e)
                        {
                            //tree.SetVar(var, 0);
                            if (Formula == false)
                            {
                                cell.setValue(cell.Text);
                            }
                            else
                            {
                                cell.setValue("!(BadReference)");
                            }
                        }
                    }
                }
                cell.mTree = tree;
                string n = cell.Name;

                bool badRef = false;
            if (Formula == true)
            {
                if (cell.mTree.getVarNames() != null)
                {
                    foreach (string refed in cell.mTree.getVarNames())
                    {
                        try
                        {
                            refTable[refed].Add(cell);
                        }
                        catch (Exception e)
                        {
                            //cell.setValue("!(BadReference)");
                            badRef = true;
                            break;
                        }
                    }

                }
            }

            if (Formula == true)
            {
                if (checkSelfRef(cell.mTree.getVarNames(), cell))
                {
                    cell.setValue("!(SelfReference)");
                    return;
                }

                if (checkCircVars(cell.mTree.getVarNames(), cell) == true)
                {
                    cell.setValue("!(CircReference)");
                    badRef = true;
                }
            }

                if (cell.Text == "")
                {
                    cell.setValue("");
                }
                else if (Formula == true && badRef != true)
                {
                    cell.setValue(cell.mTree.Eval().ToString());
                }
                string val = cell.Value;

                foreach (SpreadsheetCell c in refTable[cell.Name].ToList())
                {
                    if (badRef == true)
                    {
                        break;
                    }
                    if (c.Text[0] == '=')
                    {
                        updateCell(c, true);
                    }
                    else
                    {
                        updateCell(c, false);
                    }
                }
            
        }

        //Code for undo and redo stacks

        public void AddUndo(UndoRedoCollection item)
        {
            mUndoRedo.AddUndo(item);
        }

        public void Undo()
        {
            mUndoRedo.performUndo().Undo(); // perform undo of item at top of undo stack
        }

        public void Redo()
        {
            mUndoRedo.performRedo().Redo(); // perform redo of item at top of redo stack
        }

        public bool CanUndo()
        {
            if (mUndoRedo.IsUndoEmpty() == true) // empty so cant undo
            {
                return false;
            }

            return true; // not empty
        }

        public bool CanRedo()
        {
            if (mUndoRedo.IsRedoEmpty() == true)
            {
                return false;
            }

            return true; // not empty
        }

        //returns task in string form for menu
        public string GetUndoTask()
        {
            return mUndoRedo.GetUndoMessage();
        }

        public string GetRedoTask()
        {
            return mUndoRedo.getRedoMessage();
        }

        public void Save (Stream dest)
        {
            // source: http://www.java2s.com/Code/CSharpAPI/System.Xml/XmlWriterCreateFileStream.htm
            XmlWriter xml = XmlWriter.Create(dest);

            xml.WriteStartDocument();
            xml.WriteStartElement("Spreadsheet");

            foreach (SpreadsheetCell cell in Table) // go through every cell in the logic layer
            {
                if (cell.BGColor != -1 || cell.Value != "" || cell.Text != "") // check if bg color, text, or value has been changed and needs to be saved
                {
                    xml.WriteStartElement("Cell"); // creates starting element tag, Cell for start
                    xml.WriteElementString("Value", cell.Value.ToString()); // save value
                    xml.WriteElementString("Text", cell.Text.ToString()); // save text
                    xml.WriteElementString("BGColor", cell.BGColor.ToString()); // text bg color
                    xml.WriteElementString("Column", cell.ColumnIndex.ToString()); // save column 
                    xml.WriteElementString("Row", cell.RowIndex.ToString()); // save row
                    xml.WriteEndElement(); // end
                }
            }

            xml.WriteEndElement(); // end outer
            //xml.Flush();
            xml.Close();
        }

        public void Load (Stream source)
        {
            //source: http://stackoverflow.com/questions/566167/query-an-xdocument-for-elements-by-name-at-any-depth

            XDocument xml = XDocument.Load(source);

            foreach (XElement e in xml.Root.Elements("Cell")) //root = "spreadsheet" so go thru every element under "Cell"
            {
                SpreadsheetCell cell = Table[int.Parse(e.Element("Row").Value.ToString()), int.Parse(e.Element("Column").Value.ToString())]; //gets cell from logic table using value from elements xml

                cell.Text = e.Element("Text").Value.ToString(); //update text -> formula -> value
                cell.BGColor = int.Parse(e.Element("BGColor").Value.ToString()); // update cell color
            }
            
        }

        //HW10 extra code
        //from class
        private bool CheckCircRef(SpreadsheetCell cell, SpreadsheetCell cell2)
        {
            if (refTable[cell2.Name].Contains(cell))
            { return true; }

            Stack<SpreadsheetCell> temp = new Stack<SpreadsheetCell>();

            foreach (string name in refTable.Keys)
            {
                if (refTable[name].Contains(cell))
                {
                    temp.Push(GetCell(Convert.ToInt32(name.Substring(1)) - 1, Convert.ToInt32(name[0] - 65)));
                }
            }

            while (temp.Count > 0)
            {
                if (CheckCircRef(temp.Pop(), cell2))
                {
                    return true;
                }
            }

            return false;
      
        }

        private bool checkCircVars(string[] vars, SpreadsheetCell cell)
        {
            bool circ = false;
            if (vars != null)
            {
                foreach (string var in vars)
                {
                    circ = CheckCircRef(GetCell(Convert.ToInt32(var.Substring(1)) - 1, Convert.ToInt32(var[0]) - 65), cell);
                    if (circ)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //check if var is = to cell name
        private bool checkSelfRef (string[] vars, SpreadsheetCell cell)
        {
            if (vars != null)
            {
                return vars.Contains(cell.Name); // true if self ref
            }
            else
            {
                return false;
            }
        }
        //Demo button function
        public void demo ()
        {
            for (int i = 0; i < 50; i++) // 50 random hello worlds
            {
                RandCol = RanNum.Next(26);
                RandRow = RanNum.Next(50);
                Table[RandRow, RandCol].Text = "Hello World";
            }

            for (int i = 0; i < 50; i++) // change every row of B to statement
            {
                Table[i, 1].Text = "This is Cell B" + (i + 1).ToString();
            }

            for (int i = 0; i < 50; i++)
            {
                Table[i, 0].Text = "=B" + (i + 1).ToString();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CptS322;
using SpreadsheetNamespace;
using NormalCellNamespace;

/*
Name: Hunter Grubenhoff
ID: 11417582
CptS 322
*/

namespace Spreadsheet_HGrubenhoff
{
    public partial class Form1 : Form
    {
        Spreadsheet spreadsheet;

        public Form1()
        {
            InitializeComponent();
            spreadsheet = new Spreadsheet(50, 26); // 26 col, 50 rows
            spreadsheet.CellPropertyChanged += CellPropertyChangeOccur;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            

            //add columns auto
            char Column = 'A';
            while (Column != ('Z' + 1)) // A-Z
            {
                dataGridView1.Columns.Add("Column", Column.ToString());
                Column++; // next value on ASCII table
            }

            //add rows auto
            int Row = 0;

            dataGridView1.Rows.Add(50); //creates 50 rows

            for (Row = 0; Row < 50; Row++)
            {
                dataGridView1.Rows[Row].HeaderCell.Value = (Row + 1).ToString(); // names the rows
            }
            dataGridView1.RowHeadersWidth = 50; // resize the header to fit the numbers
            
        }

        private void button1_Click(object sender, EventArgs e) // demo button
        {
            spreadsheet.demo();
        }

        private void CellPropertyChangeOccur(object sender, PropertyChangedEventArgs e)
        {
            SpreadsheetCell temp = (SpreadsheetCell)sender;
            if (e.PropertyName == "Value")
            {
                dataGridView1.Rows[temp.RowIndex].Cells[temp.ColumnIndex].Value = temp.Value;
            }
            else if (e.PropertyName == "BGColor")
            {
                //converts int in cell to color and sets cell
                dataGridView1.Rows[temp.RowIndex].Cells[temp.ColumnIndex].Style.BackColor = Color.FromArgb(temp.BGColor);
            }
        }

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            SpreadsheetCell temp = spreadsheet.GetCell(e.RowIndex, e.ColumnIndex);
            dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = temp.Text; // sets ui cell to display text
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            SpreadsheetCell temp = spreadsheet.GetCell(e.RowIndex, e.ColumnIndex);
            string s = temp.Text;// saves text before it changes
            temp.Text = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            foreach (string name in spreadsheet.refTable.Keys) // goes thru every cell with a value
            {
                foreach (SpreadsheetCell c in spreadsheet.refTable[name].ToList()) // loops thru every cell that relies on temp
                {
                    dataGridView1.Rows[c.RowIndex].Cells[c.ColumnIndex].Value = c.Value;// sets ui cell back to value
                }
            }

            Stack<SpreadsheetCell> currentCells = new Stack<SpreadsheetCell>(); // holds data for updated cells
            Stack<SpreadsheetCell> prevCells = new Stack<SpreadsheetCell>(); // holds old data in case of undo

            Stack<string> currText = new Stack<string>();
            Stack<string> prevText = new Stack<string>();

            SpreadsheetCell cell = spreadsheet.GetCell(e.RowIndex, e.ColumnIndex);

            prevText.Push(s); // pushes old text onto stack

            currentCells.Push(cell); // pushs cell onto stack
            prevCells.Push(cell);

            //cell.Text = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

            currText.Push(cell.Text);

            //create new restore text command
            RestoreText cmd = new RestoreText("Text Change", prevText, currText, prevCells, currentCells); // constructor
            spreadsheet.AddUndo(cmd); // add command to spreadsheets inner undo stack
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        // will enter when user click choose background color button on menu strip under cell
        private void chooseBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog colorMenu = new ColorDialog();
            
            if (colorMenu.ShowDialog() != DialogResult.OK) // display color wheel/menu and check if result is good
            {
                return;
            }

            Stack<SpreadsheetCell> currentCells = new Stack<SpreadsheetCell>(); // holds data for updated cells
            Stack<SpreadsheetCell> prevCells = new Stack<SpreadsheetCell>(); // holds old data in case of undo

            Stack<int> currColor = new Stack<int>(); // holds ints for current colors
            Stack<int> prevColor = new Stack<int>();

            foreach (DataGridViewCell c in dataGridView1.SelectedCells) //go thru every slected cell
            {
                SpreadsheetCell temp = spreadsheet.GetCell(c.RowIndex, c.ColumnIndex); //get cell from logic layer
                prevCells.Push(temp); // add all cells to prev list
                currentCells.Push(temp); // add all cells to cur list
                prevColor.Push(temp.BGColor); // save old color
                temp.BGColor = colorMenu.Color.ToArgb(); // update color in logic layer
                currColor.Push(temp.BGColor); // saves new color
            }

            //create command to restore bg color
            RestoreBG cmd = new RestoreBG("BackGround Color Change", prevCells, currentCells, prevColor, currColor);
            spreadsheet.AddUndo(cmd); 
        }

        //when the user first clicks edit, will enable or disable redo and undo buttons
        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (spreadsheet.CanUndo() == false) // cant undo because stack is empty
            {
                undoToolStripMenuItem.Enabled = false; // turn off button
            }
            else
            {
                undoToolStripMenuItem.Enabled = true; // turn on button
                undoToolStripMenuItem.Text = "Undo";
                undoToolStripMenuItem.Text += " " + spreadsheet.GetUndoTask(); // show user what the undo task is
            }

            if (spreadsheet.CanRedo() == false)
            {
                redoToolStripMenuItem.Enabled = false;
            }
            else
            {
                redoToolStripMenuItem.Enabled = true;
                redoToolStripMenuItem.Text = "Redo";
                redoToolStripMenuItem.Text += " " + spreadsheet.GetRedoTask(); // show user redo task
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (spreadsheet.CanUndo() == true)
            {
                spreadsheet.Undo();
            }
            else
            {
                return;
            }
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (spreadsheet.CanRedo() == true)
            {
                spreadsheet.Redo();
            }
            else
            {
                return;
            }
        }

        //source: http://stackoverflow.com/questions/13788156/looping-through-datagridview-cells
        private void ResetSpreadsheetUI()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Value = ""; //reset every cell text
                    cell.Style.BackColor = Color.FromArgb(-1); // back to white
                }
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog menu = new OpenFileDialog();

            if (menu.ShowDialog() == DialogResult.OK) // file ok to open
            {
                spreadsheet = new Spreadsheet(50, 26); //erase current spreadsheet logic layer
                spreadsheet.CellPropertyChanged += CellPropertyChangeOccur; //subscribe

                ResetSpreadsheetUI(); //erase UI

                FileStream openFile = new FileStream(menu.FileName, FileMode.Open, FileAccess.Read);

                spreadsheet.Load(openFile);

                openFile.Close();
                openFile.Dispose();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog menu = new SaveFileDialog();
            if (menu.ShowDialog() == DialogResult.OK)
            {
                FileStream saveFile = new FileStream(menu.FileName, FileMode.Create, FileAccess.Write); // open file for saving

                spreadsheet.Save(saveFile);
                saveFile.Close();
                saveFile.Dispose();
            }
        }
    }
}

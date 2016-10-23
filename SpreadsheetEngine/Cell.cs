using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

/*
Name: Hunter Grubenhoff
ID: 11417582
CptS 322
*/

namespace CptS322
{
    public abstract class SpreadsheetCell : INotifyPropertyChanged
    {
        protected string mText;
        protected string mValue;
        protected int mRowIndex;
        protected int mColumnIndex;
        protected string mFormula;
        protected string mName;
        protected int mBGColor;
        internal ExpTree mTree;
 

        //source https://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged(v=vs.110).aspx
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public int RowIndex // read only property
        {
            get { return mRowIndex; }
        }

        public int ColumnIndex // read only property
        {
            get { return mColumnIndex; }            
        }

        //Text property
        public string Text
        {
            get { return mText; }
            set
            {
                if (mText == value)
                {
                    //ignore it
                }
                else
                {
                    mText = value;
                    NotifyPropertyChanged("Text");
                }
            }
        }

        public string Name
        {
            get { return mName; }
        }

        public string Formula
        {
            get { return mFormula; }
            set { mFormula = value; }
        }

        //BGColor property
        public int BGColor
        {
            get { return mBGColor; }
            set
            {
                if (mBGColor == value) // color is same, do nothing
                {

                }
                else
                {
                    mBGColor = value; // update color value
                    NotifyPropertyChanged("BGColor"); // fire off property changed event
                }
            }
        }

        //Value getter and setter
        public string Value
        {
            get { return mValue; }
        }
        internal void setValue(string newValue)
        {
            mValue = newValue;
        }

        //source http://stackoverflow.com/questions/10373561/convert-a-number-to-a-letter-in-c-sharp-for-use-in-microsoft-excel
        public static string GetColumnName(int index)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var value = "";

            if (index >= letters.Length)
                value += letters[index / letters.Length - 1];

            value += letters[index % letters.Length];

            return value;
        }
    }
}

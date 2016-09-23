using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InvertIndexWindow
{
    public partial class Form1 : Form
    {
        private Index index;
        private OpenFileDialog openDirectoryDialog;
        private List<string> bookList;

        public Form1()
        {
            InitializeComponent();
            //InitializeOpenDirectoryDialog();
            index = new Index();
            textBox1.KeyDown += TextBoxKeyUp;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void TextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            foreach (var elem in index.Search(textBox1.Text))
            {
                listBox1.Items.Add(elem);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            index.CollectionsToDefault();

            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "XML files (*.xml)|*.xml";
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    label1.Text = openFileDialog1.FileName;
                    index.LoadIndex(openFileDialog1.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            index.CollectionsToDefault();

            bookList = new List<string>();
            GetDirectory();
            //label2.Text = "Done";
        }

        private void GetDirectory()
        {
            InitializeOpenDirectoryDialog();

            DialogResult dr = openDirectoryDialog.ShowDialog();
            List<string> files = new List<string>();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                // Read the files
                foreach (string file in openDirectoryDialog.FileNames)
                {
                    files.Add(file);
                }
                bookList = files;
                SaveIndex();
            }


        }

        private void SaveIndex()
        {
            label2.Text = "Loading ... Please wait";

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "XML files (*.xml)|*.xml";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                index.CreateIndexFile(bookList, saveFileDialog1.FileName);
                sw.Stop();
                label2.Text = (sw.ElapsedMilliseconds / 1000.0).ToString() + " seconds";
            }
        }

        private void InitializeOpenDirectoryDialog()
        {
            openDirectoryDialog = new OpenFileDialog();

            // Set the file dialog to filter for graphics files.
            openDirectoryDialog.Filter = "FB2 files(*.fb2) | *.fb2";

            // Allow the user to select multiple images.
            openDirectoryDialog.Multiselect = true;
            openDirectoryDialog.Title = "Books Browser";
        }

        
    }
}

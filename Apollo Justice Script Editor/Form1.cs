using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace Apollo_Justice_Script_Editor
{
    public partial class Form1 : Form
    {
        MessageDrawer md;
        Graphics gr;
        string filePath;
        bool loaded = false;
        int numoffound = 0;
        bool infinding = false;
        int[] fs = null;
        public Form1()
        {
            InitializeComponent();
            richTextBox1.Click += On_Click;
            saveToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.Enabled = false;
            richTextBox1.Enabled = false;
            groupBox1.Enabled = false;
        }

        public void Open()
        {
            string scpath = "";
            loaded = false;
            using (OpenFileDialog openFileDialog = new OpenFileDialog()) //Getting file's path
            {
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = "Open script file";
                openFileDialog.Filter = "Text file (*.txt)|*.txt|Any file (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                DialogResult dr = openFileDialog.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    scpath = openFileDialog.FileName;
                }
                else if (dr == DialogResult.Cancel)
                {
                    loaded = true;
                    return;
                }
            }
            richTextBox1.Lines = File.ReadAllLines(scpath);
            md = new MessageDrawer();
            gr = pictureBox1.CreateGraphics();
            loaded = true;
            filePath = scpath;
            saveToolStripMenuItem.Enabled = true; //updating all UI elements
            saveAsToolStripMenuItem.Enabled = true;
            richTextBox1.ScrollBars = (RichTextBoxScrollBars)ScrollBars.Vertical;
            richTextBox1.Enabled = true;
            groupBox1.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void On_Click(object sender, EventArgs e)
        {
            if (!loaded)
                return;
            new Thread(new ThreadStart(ShowMessage)).Start();
        }

        async void ShowMessage()
        {
            string dial = ScriptHelper.GetMessage(richTextBox1.Lines, richTextBox1.SelectionStart);
            if (dial == null)
                return;
            short[][] shorts = ScriptHelper.GetMessageShorts(dial);
            if (shorts == null)
                return;
            md.DrawMessage(shorts, gr);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (!loaded)
                return;
            if (infinding)
            {
                ResetFinding();
            }
            new Thread(new ThreadStart(ShowMessage)).Start();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) //saving into last file
        {
            File.WriteAllLines(filePath, richTextBox1.Lines);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string scpath = "";
            using (SaveFileDialog sfd = new SaveFileDialog()) //getting file's path
            {
                sfd.RestoreDirectory = true;
                sfd.Title = "Save script file as";
                sfd.Filter = "Text file (*.txt)|*.txt|Any file (*.*)|*.*";
                sfd.FilterIndex = 1;
                DialogResult dr = sfd.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    scpath = sfd.FileName;
                }
                else if (dr == DialogResult.Cancel)
                {
                    return;
                }
            }
            filePath = scpath;
            File.WriteAllLines(filePath, richTextBox1.Lines); //writing lines in file
        }

        public int[] Find(string seek) //finding text in file
        {
            infinding = true;
            List<int> sels = new List<int>();
            string[] text = richTextBox1.Lines;
            int len = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i].ToUpper().Contains(seek.ToUpper()))
                {
                    sels.Add(len + text[i].IndexOf(seek));
                }
                len += text[i].Length + 2;
            }
            return sels.ToArray();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int[] sels = Find(textBox2.Text);
            if (sels == null || sels.Length == 0) //if text was not found, show the message
            {
                MessageBox.Show("Matches not found");
                return;
            }
            for (int i = 0; i < sels.Length; i++)
            {
                if (sels[i] >= richTextBox1.SelectionStart)
                {
                    GotoFound(sels, i);
                    break;
                }
            }
            fs = sels;
            button4.Enabled = true;
        }

        void GotoFound(int[] sels, int ind) //replacing cursor to a found text
        {
            richTextBox1.SelectionStart = sels[ind];
            numoffound = ind;
            if (numoffound >= sels.Length - 1 && numoffound != 0)
            {
                button3.Enabled = true;
                button2.Enabled = false;
            }
            else if (numoffound >= sels.Length - 1 && numoffound == 0)
            {
                button3.Enabled = false;
                button2.Enabled = false;
            }
            else if (numoffound == 0)
            {
                button2.Enabled = true;
                button3.Enabled = false;
            }
            else
            {
                button2.Enabled = true;
                button3.Enabled = true;
            }
            richTextBox1.Focus();
            richTextBox1.ScrollToCaret();
        }

        private void button2_Click(object sender, EventArgs e) //Next button
        {
            numoffound++;
            GotoFound(fs, numoffound);
        }

        private void button3_Click(object sender, EventArgs e) //Previous button
        {
            numoffound--;
            GotoFound(fs, numoffound);
        }

        void ResetFinding() //Resetting finding
        {
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            numoffound = 0;
            fs = null;
            infinding = false;
        }

        private void button4_Click(object sender, EventArgs e) //Replace button
        {
            int cur = -1;
            if (!fs.Contains(richTextBox1.SelectionStart))
            {
                button1_Click(sender, e);
            }
            else
            {
                string[] text = richTextBox1.Lines;
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i].Contains(textBox2.Text))
                    {
                        cur++;
                        if (cur == numoffound)
                        {
                            int t = text[i].IndexOf(textBox2.Text);
                            string temp = text[i].Remove(t, textBox2.Text.Length);
                            text[i] = temp.Insert(t, textBox3.Text);
                            loaded = false;
                            richTextBox1.Lines = text;
                            loaded = true;
                            richTextBox1.SelectionStart = fs[numoffound];
                            button4.Enabled = false;
                            button1_Click(sender, e);
                            break;
                        }
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e) //Replace all button
        {
            int selc = richTextBox1.SelectionStart;
            string[] text = richTextBox1.Lines;
            for (int i = 0; i < text.Length; i++)
            {
                text[i] = text[i].Replace(textBox2.Text, textBox3.Text);
            }
            loaded = false;
            richTextBox1.Lines = text;
            loaded = true;
            richTextBox1.SelectionStart = selc;
            richTextBox1.Focus();
            richTextBox1.ScrollToCaret();
        }

        private void textBox2_TextChanged(object sender, EventArgs e) //updating UI elements after writing something in find textbox
        {
            button1.Enabled = textBox2.Text != null || textBox2.Text != "";
            button5.Enabled = textBox2.Text != null || textBox2.Text != "";
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = textBox2.Text != null || textBox2.Text != "";
            button5.Enabled = textBox2.Text != null || textBox2.Text != "";
        }

        private void buttonExtractReplies_Click(object sender, EventArgs e)
        {
            if (!loaded)
                return;

            string[] data = richTextBox1.Lines;
            List<string> replies = ExtractAllReplies(data);
            listBoxReplies.Items.Clear();
            listBoxReplies.Items.AddRange(replies.ToArray());
        }

        private List<string> ExtractAllReplies(string[] data)
        {
            List<string> replies = new List<string>();
            int index = 0;
            string tag = "";
            bool inTag = false;
            string temp = "";

            for (int i = 0; i < data.Length; i++)
            {
                char[] st = data[i].ToCharArray();
                for (int j = 0; j < st.Length; j++)
                {
                    if (inTag)
                    {
                        tag += st[j];
                        if (st[j] == '>')
                        {
                            inTag = false;
                            if (ScriptHelper.TagFinish(tag))
                            {
                                replies.Add(temp);
                                temp = "";
                            }
                            else if (ScriptHelper.TagNext(tag))
                            {
                                temp += tag;
                            }
                        }
                    }
                    else
                    {
                        if (st[j] == '<')
                        {
                            inTag = true;
                            tag = "<";
                        }
                        else if (st[j] == '>')
                        {
                            temp = "";
                        }
                        else
                        {
                            temp += st[j];
                        }
                    }
                    index++;
                }
                index += 2;
            }
            return replies;
        }

        private void listBoxReplies_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxReplies.SelectedIndex == -1 || !loaded)
                return;

            string selectedReply = listBoxReplies.SelectedItem.ToString();
            int startIndex = richTextBox1.Text.IndexOf(selectedReply);

            if (startIndex != -1)
            {
                HighlightText(startIndex, selectedReply.Length);
                richTextBox1.ScrollToCaret();
            }
        }

        private void HighlightText(int startIndex, int length)
        {
            // Ensure the indices are within the text bounds
            if (startIndex >= 0 && startIndex + length <= richTextBox1.Text.Length)
            {
                richTextBox1.Select(startIndex, length); // Select the text
                richTextBox1.SelectionBackColor = Color.Red; // Change the background color of the selected text
                richTextBox1.DeselectAll(); // Deselect the text to remove the highlight
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.Msagl.Drawing;

namespace UnlinkendIn
{
    public partial class Form1 : Form
    {
        /*
        KAMUS:
        numVar      : int                       { banyak variable }
        list        : array of string           { berisi nama-nama variabel }
        dictionary  : dictionary<string, int>   { berisi pasangan <variabel, index> untuk memudahkan pencarian index }
        matrix      : bool[numVar][numVar]      { adjacency matrix }
        */

        private int numVar;
        private string[] list;
        private Dictionary<string, int> dictionary;
        private bool[,] matrix;
        private Graph graph;                    // untuk visualisasi

        public string[] convertStack(Stack<string> convert)
        {
            var S = new Stack<string>(new Stack<string>(convert));
            string[] result = new string[S.Count];
            int i = 0;
            while (S.Count > 0)
            {
                result[i] = S.Pop();
                i++;
            }
            return result;
        }

        public Form1()                          // menampilkan layout utama program
        {
            InitializeComponent();
            OpenFileDialog browse = new OpenFileDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)              // menekan tombol browse
        {
            OpenFileDialog browse = new OpenFileDialog();
            browse.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (browse.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                comboBoxFitur.Enabled = true;
            label10.Text = System.IO.Path.GetFileName(browse.FileName);
            ReadFile reader = new ReadFile(browse.FileName);

            // membaca isi file yang telah dibrowse
            numVar = reader.GetNumVar();
            list = reader.GetVarList();
            dictionary = reader.GetDictionary();
            matrix = reader.GetMatrix();

            // menambahkan opsi akun pada choose account dan explore friend with
            comboBoxAkunAwal.Items.Clear();
            comboBoxAkunAkhir.Items.Clear();
            foreach (string a in list)
            {
                comboBoxAkunAwal.Items.Add(a);
                comboBoxAkunAkhir.Items.Add(a);
            }

            //membuat objek graph
            graph = new Microsoft.Msagl.Drawing.Graph("graph");

            //membuat graf dan menghubungkan simpul-simpul yang terkoneksi 
            for (int i = 0; i < numVar; i++)
            {
                for (int j = 0; j < i + 1; j++)
                {
                    if (matrix[i, j] == true)
                    {
                        string node1 = dictionary.FirstOrDefault(x => x.Value == i).Key;
                        string node2 = dictionary.FirstOrDefault(x => x.Value == j).Key;
                        graph.AddEdge(node1, node2).Attr.ArrowheadAtTarget = ArrowStyle.None;
                    }
                }
            }
            //menampilkan graph pada viewer
            gViewer.Graph = graph;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)         // memilih fitur yang tersedia
        {
            string fitur = comboBoxFitur.SelectedItem.ToString();

            // memilih fitur friend recommendation
            if (fitur == "Friend Recommendation")
            {
                // mengaktifkan toolbox yang digunakan dan mengnonaktifkan toolbox yang tidak digunakan
                radioButtonDFS.Enabled = false;
                radioButtonBFS.Enabled = true;
                radioButtonDFS.Checked = false;
                radioButtonBFS.Checked = true;
                comboBoxAkunAwal.Enabled = true;
                comboBoxAkunAkhir.Enabled = false;
                comboBoxAkunAkhir.ResetText();
            }

            // memilih fitur explorer friends
            if (fitur == "Explorer Friends")
            {
                // mengaktifkan toolbox yang digunakan dan mengnonaktifkan toolbox yang tidak digunakan
                radioButtonDFS.Enabled = true;
                radioButtonBFS.Enabled = true;
                radioButtonDFS.Checked = true;
                radioButtonBFS.Checked = false;
                comboBoxAkunAwal.Enabled = true;
                comboBoxAkunAkhir.Enabled = true;
                comboBoxAkunAwal.ResetText();
                comboBoxAkunAkhir.ResetText();
            }

            // memilih fitur kosong / tidak memilih fitur
            if (fitur == "")
            {
                // mengaktifkan toolbox yang digunakan dan mengnonaktifkan toolbox yang tidak digunakan
                radioButtonDFS.Enabled = false;
                radioButtonBFS.Enabled = false;
                radioButtonDFS.Checked = false;
                radioButtonBFS.Checked = false;
                comboBoxAkunAwal.Enabled = false;
                comboBoxAkunAkhir.Enabled = false;
                comboBoxAkunAwal.ResetText();
                comboBoxAkunAkhir.ResetText();
            }
        }

        private void button2_Click(object sender, EventArgs e)              // menekan tombol submit
        {
            // menampilkan text box sebagai tempat keluaran program
            textBox2.Visible = true;
            textBox2.ScrollBars = ScrollBars.Both;
            textBox2.Clear();

            // membuat simpul berwarna putih kembali
            foreach (string cek in list)
            {
                graph.FindNode(cek).Attr.FillColor = Microsoft.Msagl.Drawing.Color.White;
            }

            // menampilkan output fitur rekomendasi teman
            if (comboBoxFitur.SelectedItem.ToString() == "Friend Recommendation")
            {
                string akun = comboBoxAkunAwal.SelectedItem.ToString();
                Friend_Recommendation acc = new Friend_Recommendation(akun, numVar, list, dictionary, matrix);
                acc.process();
                string[,] mutual = acc.get_mutual_friends();
                int[] link = acc.get_total_linked();
                // masih belum terurut dengan jumlah mutual friends terbesar
                textBox2.Text += "Daftar rekomendasi teman untuk akun ";
                textBox2.Text += akun;
                textBox2.Text += " :";
                textBox2.Text += Environment.NewLine;
                for (int i = 0; i < numVar; i++)
                {
                    if (link[i] > 0)
                    {
                        textBox2.Text += "Nama akun: ";
                        textBox2.Text += list[i];
                        textBox2.Text += Environment.NewLine;
                        textBox2.Text += link[i];
                        textBox2.Text += " mutual friends: ";
                        textBox2.Text += Environment.NewLine;

                        for (int j = 0; j < link[i]; j++)
                        {
                            textBox2.Text += mutual[i, j];
                            textBox2.Text += Environment.NewLine;
                        }
                        textBox2.Text += Environment.NewLine;
                    }
                }
            }

            // menampilkan output fitur eplorer friends
            else if (comboBoxFitur.SelectedItem.ToString() == "Explorer Friends")
            {
                // membaca akun awal dan akun tujuan
                string awal = comboBoxAkunAwal.Text;
                string akhir = comboBoxAkunAkhir.Text;

                // menggunakan algoritma BFS
                if (radioButtonBFS.Checked == true)
                {
                    // menjalankan algoritma BFS yang ada pada BFS.cs
                    BFS friendRecom = new BFS(awal, akhir, numVar, list, dictionary, matrix);
                    Stack<string> path = new Stack<string>();
                    path = friendRecom.ConstructPath();
                    int degree = path.Count - 1;
                    whiteLine();

                    // menampilkan output
                    // tidak ada jalur koneksi yang tersedia
                    if (path.Count == 0)
                    {
                        textBox2.Text += "Tidak ada jalur koneksi yang tersedia";
                        textBox2.Text += Environment.NewLine;
                        textBox2.Text += "Anda harus memulai koneksi baru itu sendiri.";
                    }
                    // kedua akun sudah berteman
                    else if (degree == 0)
                    {
                        textBox2.Text += "Kedua akun sudah terkoneksi/terhubung.";
                        graph.FindNode(awal).Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
                        graph.FindNode(akhir).Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
                        colorLine(awal, akhir);
                    }
                    // selain kondisi di atas
                    else
                    {
                        blackLine();
                        string[] pathArray = convertStack(path);
                        for (int currNode = 0; currNode < pathArray.Length - 1; currNode++)
                        {
                            colorLine(pathArray[currNode], pathArray[currNode + 1]);
                        }

                        textBox2.Text += degree;
                        if (degree == 1)
                        {
                            textBox2.Text += "st";
                        }
                        else if (degree == 2)
                        {
                            textBox2.Text += "nd";
                        }
                        else if (degree == 3)
                        {
                            textBox2.Text += "rd";
                        }
                        else
                        {
                            textBox2.Text += "th";
                        }
                        textBox2.Text += " Degree";
                        textBox2.Text += Environment.NewLine;
                        textBox2.Text += "Alternatif Jalur yang Tersedia";
                        textBox2.Text += Environment.NewLine;

                        while (path.Count > 1)
                        {
                            string curr = path.Pop();
                            textBox2.Text += curr + "-";
                            graph.FindNode(curr).Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
                        }
                        string end = path.Pop();
                        textBox2.Text += end;
                        graph.FindNode(end).Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
                    }

                }

                // algoritma DFS
                else if (radioButtonDFS.Checked == true)
                {
                    // menjalankan algoritma DFS yang ada pada DFS.cs
                    DFS friendRecom = new DFS(awal, akhir, numVar, list, dictionary, matrix);
                    Stack<string> path = new Stack<string>();
                    path = friendRecom.ConstructPath();
                    int degree = path.Count - 2;
                    whiteLine();

                    // menampilkan output
                    // tidak ada jalur koneksi yang tersedia
                    if (path.Count == 0)
                    {
                        textBox2.Text += "Tidak ada jalur koneksi yang tersedia";
                        textBox2.Text += Environment.NewLine;
                        textBox2.Text += "Anda harus memulai koneksi baru itu sendiri.";
                    }
                    // kedua akun sudah berteman
                    else if (degree == 0)
                    {
                        textBox2.Text += "Kedua akun sudah terkoneksi/terhubung.";
                        graph.FindNode(awal).Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
                        graph.FindNode(akhir).Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
                        colorLine(awal, akhir);
                    }
                    // selain kondisi di atas
                    else
                    {
                        blackLine();
                        string[] pathArray = convertStack(path);
                        for (int currNode = 0; currNode < pathArray.Length - 1; currNode++)
                        {
                            colorLine(pathArray[currNode], pathArray[currNode + 1]);
                        }
                        textBox2.Text += degree;
                        if (degree == 1)
                        {
                            textBox2.Text += "st";
                        }
                        else if (degree == 2)
                        {
                            textBox2.Text += "nd";
                        }
                        else if (degree == 3)
                        {
                            textBox2.Text += "rd";
                        }
                        else
                        {
                            textBox2.Text += "th";
                        }
                        textBox2.Text += " Degree";
                        textBox2.Text += Environment.NewLine;
                        textBox2.Text += "Alternatif Jalur yang Tersedia";
                        textBox2.Text += Environment.NewLine;
                        while (path.Count > 1)
                        {
                            string curr = path.Pop();
                            textBox2.Text += curr + "-";
                            graph.FindNode(curr).Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
                        }
                        string end = path.Pop();
                        textBox2.Text += end;
                        graph.FindNode(end).Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
                    }
                }
                gViewer.Graph = graph;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)         // memilih akun utama atau awal
        {
            // tidak menampilkan akun yang sudah dipilih pada akun tujuan atau akhir
            comboBoxAkunAkhir.Items.Clear();
            foreach (string a in list)
            {
                comboBoxAkunAkhir.Items.Add(a);
            }
            comboBoxAkunAkhir.Items.Remove(comboBoxAkunAwal.SelectedItem);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)         // memilih akun tujuan atau akhir
        {
            // tidak menampilkan akun yang sudah dipilih pada akun utama atau awal
            comboBoxAkunAwal.Items.Clear();
            foreach (string a in list)
            {
                comboBoxAkunAwal.Items.Add(a);
            }
            comboBoxAkunAwal.Items.Remove(comboBoxAkunAkhir.SelectedItem);
        }

        private void button3_Click(object sender, EventArgs e)                          // menekan tombol clear
        {
            // melakukan riset pada program dan menghapus semua masukan pengguna
            comboBoxAkunAwal.Items.Clear();
            comboBoxAkunAkhir.Items.Clear();
            comboBoxAkunAwal.ResetText();
            comboBoxAkunAkhir.ResetText();
            comboBoxFitur.ResetText();
            comboBoxAkunAwal.SelectedIndex = -1;
            comboBoxAkunAkhir.SelectedIndex = -1;
            comboBoxFitur.SelectedIndex = 0;
            label10.Text = "";
            textBox2.Visible = false;
            comboBoxFitur.Enabled = false;
            //create a graph object 
            Microsoft.Msagl.Drawing.Graph delgraph = new Microsoft.Msagl.Drawing.Graph("graph");
            //bind the graph to the viewer 
            gViewer.Graph = delgraph;

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            // telah memilih salah satu algoritma agar dapat memilih akun
            comboBoxAkunAwal.Enabled = true;
            comboBoxAkunAkhir.Enabled = true;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            // telah memilih salah satu algoritma agar dapat memilih akun
            comboBoxAkunAwal.Enabled = true;
            comboBoxAkunAkhir.Enabled = true;
        }

        private void whiteLine()
        {
            Microsoft.Msagl.Drawing.Edge[] edges = graph.Edges.ToArray();
            for (int i = 0; i < edges.Length; i++)
            {
                edges.ElementAt(i).Attr.Color = Microsoft.Msagl.Drawing.Color.White;
            }
        }

        private void blackLine()
        {
            Microsoft.Msagl.Drawing.Edge[] edges = graph.Edges.ToArray();
            for (int i = 0; i < edges.Length; i++)
            {
                edges.ElementAt(i).Attr.Color = Microsoft.Msagl.Drawing.Color.Black;
            }
        }

        private void colorLine(string node1, string node2)
        {
            Microsoft.Msagl.Drawing.Edge[] edges = graph.Edges.ToArray();
            int i;
            int j = -1;
            for (i = 0; i < edges.Length; i++)
            {
                if (((edges[i].Source.Equals(node1)) && (edges[i].Target.Equals(node2))) || ((edges[i].Source.Equals(node2)) && (edges[i].Target.Equals(node1))))
                {
                    j = i;
                }
            }
            if (j != -1)
            {
                edges.ElementAt(j).Attr.Color = Microsoft.Msagl.Drawing.Color.Green;
            }
            else
            {
                edges.ElementAt(j).Attr.Color = Microsoft.Msagl.Drawing.Color.Black;
            }

        }
    }
}

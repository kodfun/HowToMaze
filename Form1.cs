using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Drawing.Drawing2D;

namespace howto_maze
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private int Xmin, Ymin, CellWid, CellHgt;

        private void btnCreate_Click(object sender, EventArgs e)
        {
            // Figure out the drawing geometry.
            int wid = int.Parse(txtWidth.Text);
            int hgt = int.Parse(txtHeight.Text);

            CellWid = picMaze.ClientSize.Width / (wid + 2);
            CellHgt = picMaze.ClientSize.Height / (hgt + 2);
            if (CellWid > CellHgt) CellWid = CellHgt;
            else CellHgt = CellWid;
            Xmin = (picMaze.ClientSize.Width - wid * CellWid) / 2;
            Ymin = (picMaze.ClientSize.Height - hgt * CellHgt) / 2;

            // Build the maze nodes.
            MazeNode[,] nodes = MakeNodes(wid, hgt);

            // Build the spanning tree.
            FindSpanningTree(nodes[0, 0]);

            // Display the maze.
            DisplayMaze(nodes);
            DisplayMazeText(nodes);
        }

        // Make the network of MazeNodes.
        private MazeNode[,] MakeNodes(int wid, int hgt)
        {
            // Make the nodes.
            MazeNode[,] nodes = new MazeNode[hgt, wid];
            for (int r = 0; r < hgt; r++)
            {
                int y = Ymin + CellHgt * r;
                for (int c = 0; c < wid; c++)
                {
                    int x = Xmin + CellWid * c;
                    nodes[r, c] = new MazeNode(
                        x, y, CellWid, CellHgt, c , r);
                }
            }

            // Initialize the nodes' neighbors.
            for (int r = 0; r < hgt; r++)
            {
                for (int c = 0; c < wid; c++)
                {
                    if (r > 0)
                        nodes[r, c].Neighbors[MazeNode.North] = nodes[r - 1, c];
                    if (r < hgt - 1)
                        nodes[r, c].Neighbors[MazeNode.South] = nodes[r + 1, c];
                    if (c > 0)
                        nodes[r, c].Neighbors[MazeNode.West] = nodes[r, c - 1];
                    if (c < wid - 1)
                        nodes[r, c].Neighbors[MazeNode.East] = nodes[r, c + 1];
                }
            }

            // Return the nodes.
            return nodes;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            btnCreate.PerformClick();
            txtMaze.Text = txtMaze.Text.Replace(" ", "");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtMaze.Text);
        }

        // Build a spanning tree with the indicated root node.
        private void FindSpanningTree(MazeNode root)
        {
            Random rand = new Random();

            // Set the root node's predecessor so we know it's in the tree.
            root.Predecessor = root;

            // Make a list of candidate links.
            List<MazeLink> links = new List<MazeLink>();

            // Add the root's links to the links list.
            foreach (MazeNode neighbor in root.Neighbors)
            {
                if (neighbor != null)
                    links.Add(new MazeLink(root, neighbor));
            }

            // Add the other nodes to the tree.
            while (links.Count > 0)
            {
                // Pick a random link.
                int link_num = rand.Next(0, links.Count);
                MazeLink link = links[link_num];
                links.RemoveAt(link_num);

                // Add this link to the tree.
                MazeNode to_node = link.ToNode;
                link.ToNode.Predecessor = link.FromNode;

                // Remove any links from the list that point
                // to nodes that are already in the tree.
                // (That will be the newly added node.)
                for (int i = links.Count - 1; i >= 0; i--)
                {
                    if (links[i].ToNode.Predecessor != null)
                        links.RemoveAt(i);
                }

                // Add to_node's links to the links list.
                foreach (MazeNode neighbor in to_node.Neighbors)
                {
                    if ((neighbor != null) && (neighbor.Predecessor == null))
                        links.Add(new MazeLink(to_node, neighbor));
                }
            }
        }

        // Display the maze in the picMaze PictureBox.
        private void DisplayMaze(MazeNode[,] nodes)
        {
            int hgt = nodes.GetUpperBound(0) + 1;
            int wid = nodes.GetUpperBound(1) + 1;
            Bitmap bm = new Bitmap(
                picMaze.ClientSize.Width,
                picMaze.ClientSize.Height);
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                for (int r = 0; r < hgt; r++)
                {
                    for (int c = 0; c < wid; c++)
                    {
                        //nodes[r, c].DrawCenter(gr, Brushes.Red);
                        nodes[r, c].DrawWalls(gr, Pens.Black);
                        //nodes[r, c].DrawNeighborLinks(gr, Pens.Black);
                        //nodes[r, c].DrawBoundingBox(gr, Pens.Blue);
                        //nodes[r, c].DrawPredecessorLink(gr, Pens.LightGray);
                    }
                }
            }

            picMaze.Image = bm;
        }

        // Display the maze in the TextBox.
        private void DisplayMazeText(MazeNode[,] nodes)
        {
            int hgt = nodes.GetLength(0);
            int wid = nodes.GetLength(1);
            string maze = "";

            for (int y = 0; y < hgt; y++)
            {
                // mevcut hucre ve sağ duvar
                for (int x = 0; x < wid; x++)
                {
                    MazeNode current = nodes[y, x];
                    maze += "0 ";
                    var east = current.Neighbors[MazeNode.East];
                    if (east == null) continue;
                    if (east.Predecessor == current || east == current.Predecessor)
                    {
                        maze += "0 ";
                    }
                    else
                    {
                        maze += "1 ";
                    }
                    // maze += current.ToString() + " ";
                }
                maze += Environment.NewLine;

                // altduvar
                for (int x = 0; x < wid; x++)
                {
                    MazeNode current = nodes[y, x];
                    var south = current.Neighbors[MazeNode.South];
                    if (south == null) continue;
                    if (south.Predecessor == current || south == current.Predecessor)
                    {
                        maze += "0 1 ";
                    }
                    else
                    {
                        maze += "1 1 ";
                    }
                    // maze += current.ToString() + " ";
                }
                maze = maze.Remove(maze.Length - 2);
                maze += Environment.NewLine;
            }

            txtMaze.Text = maze;
        }
    }
}

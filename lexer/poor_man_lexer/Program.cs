/*base code for the "poor man's lexer" sourced from "Paul Hollingsworth" at
http://stackoverflow.com/questions/673113/poor-mans-lexer-for-c-sharp
base code for 'process start' block of code sourced from "Stormenet" at
http://stackoverflow.com/questions/878632/best-way-to-call-external-program-in-c-sharp-and-parse-output
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Drawing;

namespace poor_man_lexer
{
    class Program
    {
        private static int add_flg;
        private static int rem_flg;
        private static int bad_rem_flg;

        //used to account for the fact that the data structure tokens are repeated twice at initialization
        //ex. List<string> test = new List<string>();
        private static int second_pass = 1;

        private static int enq_flg;
        private static int deq_flg;
        private static int bad_deq_flg;

        private static int deq_counter = 0;

        private static int push_flg;
        private static int pop_flg;

        private static int node_count = 0;
        private static int dataIdentifier; //0 is List, 1 is Queue, 2 is Stack 

        //[i,0] is node name, [i,1] is for making nodes invisible, [i,2] is for making dangling pointers red, [i,3] is for connecting nodes after a good remove
        static string[,] nodes = new string[20, 4];

        //array of strings to hold the chopped up input file as separate strings in order to produce sequential png's
        static List<string> chunks = new List<string>();

        static void List(Lexer l)
        {
            if (add_flg == 1)
            {
                //this line removes the quotes and adds the "node" to the txt file                       
                nodes[node_count - 1, 0] = l.TokenContents.Replace("\"", "");
                add_flg = 0;
            }

            if (rem_flg == 1)
            {
                string tmp_token = l.TokenContents.Replace("\"", "");
                for (int i = 0; i < node_count; i++)
                {
                    if (tmp_token.CompareTo(nodes[i, 0].ToString()) == 0)
                    {
                        nodes[i, 1] = "[style=\"invis\"]";
                        nodes[i, 2] = "[style=\"invis\"]";
                        if (i > 0)
                        {
                            //nodes[(i - 1), 2] = "[style=\"invis\"]";
                            nodes[(i - 1), 3] = "1";
                        }
                    }
                }

                rem_flg = 0;
            }

            if (bad_rem_flg == 1)
            {
                string tmp_token = l.TokenContents.Replace("\"", "");
                for (int i = 0; i < node_count; i++)
                {
                    if (tmp_token.CompareTo(nodes[i, 0].ToString()) == 0)
                    {
                        nodes[i, 1] = "[style=\"invis\"]";
                        nodes[i, 2] = "[style=\"invis\"]";
                        if (i > 0)
                        {
                            if(nodes[(i-1), 1] == "[style=\"invis\"]")
                                nodes[(i - 1), 2] = "[color=\"invis\"]";
                            else
                                nodes[(i - 1), 2] = "[color=\"red\"]";
                        }
                    }
                }

                bad_rem_flg = 0;
            }
        }

        static void Queue(Lexer l)
        {
            if(enq_flg == 1)
            {
                nodes[node_count - 1, 0] = l.TokenContents.Replace("\"", "");
                enq_flg = 0;

            }

            if(deq_flg == 1)
            {
                string tmp_token = l.TokenContents.Replace("\"", "");

                for(int i = 0; i < node_count; i++)
                {
                    nodes[i, 0] = nodes[(i + 1), 0];
                }

                nodes[(node_count - 1), 0] = null;

                node_count--;

                deq_flg = 0;
            }

            if(bad_deq_flg == 1)
            {
                string tmp_token = l.TokenContents.Replace("\"", "");             

                nodes[deq_counter, 0] = "_____";                
                
                deq_counter++;
                bad_deq_flg = 0;
            }
        }

        static void Stack(Lexer l)
        {
            if (push_flg == 1)
            {
                nodes[node_count - 1, 0] = l.TokenContents.Replace("\"", "");
                push_flg = 0;
            }

            if (pop_flg == 1)
            {
                string tmp_token = l.TokenContents.Replace("\"", "");

                Console.WriteLine("pop node count: {0}", node_count);

                nodes[node_count-1, 0] = "_____";

                node_count--;

                pop_flg = 0;
            }
        }

        static string buildList(string text)
        {
            //list nodes at the top of the file
            for (int i = 0; i < node_count; i++)
            {
                text = text + nodes[i, 0] + nodes[i, 1] + ";";
            }

            //list edge connections
            for (int i = 0; i < node_count - 1; i++)
            {
                if (nodes[i, 3] == "1")
                {
                    if (nodes[(i + 2), 0] != null)
                    {
                        text = text + nodes[i, 0] + "->" + nodes[(i + 2), 0] + nodes[i, 2] + ";";
                    }
                    else
                    {
                        text = text + nodes[(i+1), 0] + ";";
                    }
                }
                else
                {
                    text = text + nodes[i, 0] + "->" + nodes[(i + 1), 0] + nodes[i, 2] + ";";
                }
            }

            return text;
        }

        static string buildQueue(string text)
        {
            for (int i = 0; i < node_count; i++)
            {
                text = text + nodes[i, 0];
                if(i < node_count - 1)
                    text = text + " | <f" + (i+1) + "> ";
            }

            text = text + "\"];";

            return text;
        }

        static string buildStack(string text)
        {
            for (int i = node_count-1; i >= 0; i--)
            {
                text = text + nodes[i, 0];
                if (i > 0)
                    text = text + " | <f" + (i + 1) + "> ";
            }

            text = text + "\"]; rankdir=LR;";
            return text;
        }


        static void Main(string[] args)
        {
            if(args == null || args.Length == 0)
            {
                Console.WriteLine("Please choose a file as a parameter.");
                return;
            }

            //string sample = @"( one (two 456 -43.2 "" \"" quoted"" ))";

            //List string
            //string sample = @"class Sample_C#_input{static void main(){List<string> test = new List<string>();test.Add(""node1"");test.Add(""node2"");test.Add(""node3"");test.Remove(""node2"");}}";

            //Queue string
            //string sample = @"class sampletest{static void main(){Queue<string> test = new Queue<string>();test.Enqueue(""node1"");test.Enqueue(""node2"");test.Enqueue(""node3"");test.Dequeue();test.Dequeue();test.Enqueue(""node4"");test.Dequeue();test.Enqueue(""node5"");}}";

            //Stack string
            //string sample = @"class sampletest{static void main(){ Stack<string> test = new Stack<string>();test.Push(""node1"");test.Push(""node2"");test.Push(""node3"");test.Pop();test.Pop();test.Push(""node4"");test.Push(""node5"");}}";            

            var defs = new TokenDefinition[]
            {
                // Thanks to [steven levithan][2] for this great quoted string
                        // regex
                new TokenDefinition(@"List", "LIST"),
                new TokenDefinition(@"Add", "ADD"),
                new TokenDefinition(@"Remove", "REMOVE"),
                new TokenDefinition(@"badRemove", "BADREMOVE"),

                new TokenDefinition(@"Queue", "QUEUE"),
                new TokenDefinition(@"Enqueue", "ENQUEUE"),
                new TokenDefinition(@"Dequeue", "DEQUEUE"),
                new TokenDefinition(@"badDequeue", "BADDEQUEUE"),

                new TokenDefinition(@"Stack", "STACK"),
                new TokenDefinition(@"Push", "PUSH"),
                new TokenDefinition(@"Pop", "POP"),

                new TokenDefinition(@"([""'])(?:\\\1|.)*?\1", "QUOTED-STRING"),
                // Thanks to http://www.regular-expressions.info/floatingpoint.html
                new TokenDefinition(@"[-+]?\d*\.\d+([eE][-+]?\d+)?", "FLOAT"),
                new TokenDefinition(@"[-+]?\d+", "INT"),
                new TokenDefinition(@"#t", "TRUE"),
                new TokenDefinition(@"#f", "FALSE"),
                new TokenDefinition(@"[*<>\?\-+/A-Za-z->!]+", "SYMBOL"),
                new TokenDefinition(@"\.", "DOT"),
                new TokenDefinition(@"\(|\{", "LEFT"),
                new TokenDefinition(@"\)|\}", "RIGHT"),
                new TokenDefinition(@"\s", "SPACE"),
                new TokenDefinition(@";", "SEMI-COLON"),
                new TokenDefinition(@"_", "UNDERSCORE"),
                new TokenDefinition(@"\#", "POUND"),
                new TokenDefinition(@"=", "EQUALS")
            };

            var sample = File.ReadAllText(args[0]);

            string path = Directory.GetCurrentDirectory();

            Array.ForEach(Directory.GetFiles(path + "\\Output\\png\\"), File.Delete);
            Array.ForEach(Directory.GetFiles(path + "\\Output\\txt\\"), File.Delete);

            //parse input file into strings delimited by each semicolon
            char[] delimiter = { ';' };

            string[] snippets = sample.Split(delimiter);

            Console.WriteLine("length of snippets: {0}", snippets.Length);
            //Console.WriteLine("snippets[8]: {0}", snippets[8]);

            chunks.Add(snippets[0] + " " + snippets[1] + " ");

            for (int i = 0; i < snippets.Length - 2; i++)
            {

                add_flg = 0;
                rem_flg = 0;
                bad_rem_flg = 0;
                second_pass = 1;
                enq_flg = 0;
                deq_flg = 0;
                bad_deq_flg = 0;
                deq_counter = 0;
                push_flg = 0;
                pop_flg = 0;
                node_count = 0;

                //Console.WriteLine(snippets[i]);

                if(i > 0)
                    chunks.Insert(i, chunks[i - 1] + " " + snippets[i + 1] + " ");

                Console.WriteLine("snippet: {0}", snippets[i + 1]);
                Console.WriteLine("chunk: {0}", chunks[i]);

                TextReader r = new StringReader(chunks[i]);
                Lexer l = new Lexer(r, defs);

                string dot_file = path + "\\Output\\txt\\" + i + ".txt";

                Console.WriteLine("The current file to write to is {0}", dot_file);

                string text = "digraph G {";

                while (l.Next())
                {
                    //Console.WriteLine("Token: {0} Contents: {1}", l.Token, l.TokenContents);
                    if (l.Token.Equals("SYMBOL"))
                    {
                        Console.WriteLine("symbol: {0}", l.TokenContents);
                    }

                    if (l.Token.Equals("LIST"))
                    {
                        Console.WriteLine("List: {0}", l.TokenContents);
                        dataIdentifier = 0;

                    }

                    if (l.Token.Equals("ADD"))
                    {
                        Console.WriteLine("Add: {0}", l.TokenContents);

                        node_count++;
                        add_flg = 1;
                    }

                    if (l.Token.Equals("REMOVE"))
                    {
                        Console.WriteLine("Remove: {0}", l.TokenContents);

                        rem_flg = 1;
                    }

                    if (l.Token.Equals("BADREMOVE"))
                    {
                        Console.WriteLine("badRemove: {0}", l.TokenContents);

                        bad_rem_flg = 1;
                    }

                    if (l.Token.Equals("QUEUE"))
                    {
                        Console.WriteLine("Queue: {0}", l.TokenContents);

                        if (second_pass == 0)
                            text = text + "node [shape=\"record\"]; stack[style=filled, label=\"<f0> ";
                        dataIdentifier = 1;

                        second_pass--;
                    }

                    if (l.Token.Equals("ENQUEUE"))
                    {
                        Console.WriteLine("Enqueue: {0}", l.TokenContents);

                        node_count++;
                        enq_flg = 1;
                    }

                    if (l.Token.Equals("DEQUEUE"))
                    {
                        Console.WriteLine("Dequeue: {0}", l.TokenContents);

                        deq_flg = 1;

                        Queue(l);
                    }

                    if (l.Token.Equals("BADDEQUEUE"))
                    {
                        Console.WriteLine("badDequeue: {0}", l.TokenContents);

                        bad_deq_flg = 1;

                        Queue(l);
                    }

                    if (l.Token.Equals("STACK"))
                    {
                        Console.WriteLine("Stack: {0}", l.TokenContents);

                        if (second_pass == 0)
                            text = text + "node [shape=\"record\"]; stack[style=filled, label=\"<f0> ";

                        dataIdentifier = 2;

                        second_pass--;
                    }

                    if (l.Token.Equals("PUSH"))
                    {
                        Console.WriteLine("Push: {0}", l.TokenContents);

                        node_count++;
                        push_flg = 1;
                    }

                    if (l.Token.Equals("POP"))
                    {
                        Console.WriteLine("Pop: {0}", l.TokenContents);

                        pop_flg = 1;

                        Stack(l);
                    }

                    if (l.Token.Equals("QUOTED-STRING"))
                    {
                        Console.WriteLine("identifier: {0}", l.TokenContents);

                        switch (dataIdentifier)
                        {
                            case 0:
                                List(l);
                                break;
                            case 1:
                                Queue(l);
                                break;
                            case 2:
                                Stack(l);
                                break;
                            default:
                                break;
                        }
                    }
                }

                switch (dataIdentifier)
                {
                    case 0:
                        text = buildList(text);
                        break;
                    case 1:
                        text = buildQueue(text);
                        break;
                    case 2:
                        text = buildStack(text);
                        break;
                    default:
                        break;
                }

                text = text + "}";

                System.IO.File.WriteAllText(dot_file, text);

                Console.WriteLine("dot file: {0}", dot_file);

                //'process start' block
                Process p = new Process();
                p.StartInfo.FileName = path + "\\Graphviz\\bin\\dot.exe";
                p.StartInfo.Arguments = "-Tpng " + dot_file + " -o " + path + "\\Output\\png\\" + i + ".png";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                //Console.ReadKey();
            }
        }
    }
}

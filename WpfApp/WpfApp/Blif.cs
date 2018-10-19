using System.Collections.Generic;
using System.Linq;

namespace WpfApp
{
    class NamesStr
    {
        public List<string> Names { get; set; }
        public List<char> NamesBool { get; set; }

        public NamesStr()
        {
            Names = new List<string>();
            NamesBool = new List<char>();
        }
    }  

    class Blif
    {
        public List<string> Inputs_z { get; set; }
        public List<string> Inputs_x { get; set; }
        public List<string> Inputs_e { get; set; }
        public List<string> Outputs_ { get; set; }
        public List<NamesStr> NamesAndBoolVectors { get; set; } // lines from file "*.blif" => ".names x y z"

        public Blif()
        {
            Inputs_z = new List<string>();
            Inputs_x = new List<string>();
            Inputs_e = new List<string>();
            Outputs_ = new List<string>();
            NamesAndBoolVectors = new List<NamesStr>();
        }

        public void SetData(string fileName)
        {
            using (System.IO.StreamReader fileBlif = new System.IO.StreamReader(@"..\..\Blif\" + fileName + ".blif"))            
            {
                string temp = fileBlif.ReadLine();
                temp = fileBlif.ReadLine();
                temp = fileBlif.ReadLine().Trim();

                List<string> inputs = new List<string>();
                List<string> listByStrFile = temp.Split(' ').ToList();

                listByStrFile.RemoveAt(0);
                while (listByStrFile[0] != ".outputs")
                    FormingListForClass(ref inputs, ref listByStrFile, fileBlif);

                listByStrFile.RemoveAt(0);
                while (listByStrFile[0] != ".names")
                {
                    Outputs_.AddRange(listByStrFile);
                    temp = fileBlif.ReadLine().Trim();
                    listByStrFile = temp.Split(' ').ToList();
                }

                Outputs_.RemoveAll(item => item == "\\" || item == " ");
                inputs.RemoveAll(item => item == "\\" || item == " ");

                //forming inputs_x и inputs_z
                Inputs_z.AddRange(inputs.GetRange(0, Outputs_.Count));
                Inputs_x.AddRange(inputs.GetRange(Outputs_.Count, inputs.Count - Outputs_.Count));

                FormingListForNamesStrs(listByStrFile, fileBlif);
            }

            //forming inputs_e         
            for (int i = 0; i < NamesAndBoolVectors.Count; i++)
                if (NamesAndBoolVectors[i].Names[NamesAndBoolVectors[i].Names.Count - 1][0] == 'n' && NamesAndBoolVectors[i].Names[NamesAndBoolVectors[i].Names.Count - 1].Length > 1)
                    Inputs_e.Add(NamesAndBoolVectors[i].Names[NamesAndBoolVectors[i].Names.Count - 1]);
        }

        void FormingListForClass(ref List<string> formingList, ref List<string> listByStrFile, System.IO.StreamReader fileBlif)
        {
            formingList.AddRange(listByStrFile);
            string temp = fileBlif.ReadLine().Trim();
            listByStrFile = temp.Split(' ').ToList();
        }

        void FormingListForNamesStrs(List<string> listByStrFile, System.IO.StreamReader fileBlif)
        {
            string temp;
            while (listByStrFile[0] != ".end"){
                listByStrFile.RemoveAt(0);
                temp = fileBlif.ReadLine().Trim();
                string[] str = temp.Split(' ');

                List<char> flag = new List<char>();
                for (int j = 0; j < str.Length; j++)
                    for (int k = 0; k < str[j].Length; k++)
                        flag.Add(str[j][k]);
                NamesAndBoolVectors.Add(new NamesStr { Names = listByStrFile, NamesBool = flag });

                temp = fileBlif.ReadLine().Trim();
                listByStrFile = temp.Split(' ').ToList();
            }
        }
    }
    }


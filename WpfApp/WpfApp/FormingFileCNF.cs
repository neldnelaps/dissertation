using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace WpfApp
{
    class FormingFileCNF
    {
        public string Cnf { get; set; } //1 0 1 2 0 2 0 -5 0 ...
        public string CnfSolver { get; set; } //1 0 2 0 3 0 -4 0 ...
        public string SAT { get; set; } // sat or unsat

        public void FormingFileCNFFormat(CnfResolution cnf_resol)
        {
            try
            {
                StreamWriter fileCNF = new StreamWriter(@"..\..\binSatSolver\input.cnf");
                string str;
                for (int i = 0; i < cnf_resol._CnfResolution.Count; i++)
                {
                    for (int j = 0; j < cnf_resol._CnfResolution[i].Count; j++)
                    {
                        var itemCnfRes = cnf_resol._CnfResolution[i].ElementAt(j);
                        str = (itemCnfRes.Key + 1).ToString() + " ";
                        if (itemCnfRes.Key < cnf_resol.Blif.Inputs_z.Count)
                            j++;
                        else if (itemCnfRes.Value == '0')
                            Cnf += "-" + str;
                        else if (itemCnfRes.Value == '1')
                            Cnf += str;
                        else
                            Cnf += str + "0 " + "-" + str;
                    }
                    if (i == cnf_resol._CnfResolution.Count - 1)
                        Cnf += "0";
                    else
                        Cnf += "0 ";
                }
                fileCNF.WriteLine(Cnf);
                fileCNF.Close();
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("file was not written" + e.Message);
            }
        }

        public void FormingFileOutputCNF(string nameSatSolver, CnfResolution cnf_resol)
        {
            string path = Directory.GetCurrentDirectory();
            string[] list = path.Split('\\');
            string strPath = "", sone = "";

            sone += list[0][0];
            strPath += sone.ToLower();

            for (int i = 1; i < list.Length - 2; i++)
                strPath += "/" + list[i];

            try
            {
                ProcessForUbuntuExeSatSolver(strPath, nameSatSolver);

                using (System.IO.StreamReader output = new System.IO.StreamReader(@"..\..\binSatSolver\output.txt"))
                {
                    int indexZ = 0;
                    SAT = output.ReadLine();
                    
                    if (SAT == "s SATISFIABLE")
                        foreach (KeyValuePair<int, char> item in cnf_resol.Cnf_K)
                        {
                            if (item.Value == '0')
                                CnfSolver += '-' + (indexZ + 1).ToString() + ' ';
                            else
                                CnfSolver += (indexZ + 1).ToString() + ' ';
                            indexZ++;
                        }

                    string line = output.ReadLine();

                    while (line != null)
                    {
                        string[] lineSp = line.Split(' ');

                        for (int i = cnf_resol.Cnf_K.Count + 1; i < lineSp.Length; i++)
                            CnfSolver += lineSp[i] + ' ';

                        line = output.ReadLine();
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("An error occurred trying to print / The file could not be read" + e.Message);
            }
        }

        void ProcessForUbuntuExeSatSolver(string str, string nameSatSolver)
        {
            Process process = new Process();
            process.StartInfo.FileName = @"..\..\binSatSolver\ubuntu.exe";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.EnableRaisingEvents = true;
            process.Start();

            if (nameSatSolver == "riss7")
                process.StandardInput.Write("cd /mnt/" + str + "/binSatSolver && ./riss-core input.cnf output.txt");
            else
                process.StandardInput.Write("cd /mnt/" + str + "/binSatSolver && ./candy input.cnf output.txt");

            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();
        }
    }
}

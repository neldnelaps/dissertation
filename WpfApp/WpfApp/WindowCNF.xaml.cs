using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Diagnostics;
using System.Linq;

namespace WpfApp
{
    public partial class WindowCNF : Window
    {
        static CnfResolution cnf_resol = new CnfResolution();

        public WindowCNF(string fileName)
        {
            InitializeComponent();

            cnf_resol.Blif.SetData(fileName);

            cnf_resol.FormCnfResolutionListNamesIndex(cnf_resol.Blif.Inputs_z);
            cnf_resol.FormCnfResolutionListNamesIndex(cnf_resol.Blif.Inputs_x);
            cnf_resol.FormCnfResolutionListNamesIndex(cnf_resol.Blif.Inputs_e);
            cnf_resol.FormCnfResolutionListNamesIndex(cnf_resol.Blif.Outputs_);

            cnf_resol.ShapingCNFResolution(cnf_resol.Blif);
            CreateTableCnfRes(dataGridCnf);

            CNFResult.IsEnabled = false;
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow windowCNF = new MainWindow
            {
                Owner = this
            };
            cnf_resol = new CnfResolution() ?? null;
            windowCNF.Show();
            Close();
        }

        private void ButtonSimplification_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch stopWatch = new Stopwatch();

            textBoxResult.Text += $"Count C0 before: {cnf_resol.CnfResolutionBefore.Count}{Environment.NewLine}";

            stopWatch.Start();
            cnf_resol.Reduction(cnf_resol.Blif);
            stopWatch.Stop();

            string strK = "";
            foreach (KeyValuePair<int, char> item in cnf_resol.Cnf_K)
                strK += item.Value;
            textBoxResult.Text += $"K = {strK}{Environment.NewLine}";
          
            TimeSpan ts = stopWatch.Elapsed;

            textBoxResult.Text += $"RunTime: {stopWatch.Elapsed.TotalSeconds}{Environment.NewLine}";
            textBoxResult.Text += $"Count C after: {cnf_resol._CnfResolution.Count}{Environment.NewLine}";

            FormingFileCNF fileCNF = new FormingFileCNF();
            fileCNF.FormingFileCNFFormat(cnf_resol);

            textBoxResult.Text += $"CNF C0: {fileCNF.Cnf}{Environment.NewLine}";
            fileCNF.FormingFileOutputCNF(comboBoxSatSolver.Text, cnf_resol);

            textBoxResult.Text += $"CNF C0:(SAT solver) {fileCNF.SAT} {fileCNF.CnfSolver}{Environment.NewLine}";
            //if (fileCNF.SAT == "s SATISFIABLE")
            //{
            //    if (cnf_resol.CheckForCorrectSequence(fileCNF.CnfSolver))
            //        textBoxResult.Text += "Sequence Correct!";
            //    else
            //        textBoxResult.Text += "The sequence is not correct";              
            //}
            cnf_resol.RemoveOfEmptyCnf();
            CreateTableCnfRes(dataGridCnfResol);
            CNFResult.IsEnabled = true;
            buttonSimplification.IsEnabled = false;
        }

        public void CreateTableCnfRes(DataGrid dataGrid)
        {
            int  countWhile = 0, numberRows = cnf_resol._CnfResolution.Count;
            string strNames = "";
         
            strNames += NameOfTheFirstLineOfVariables(cnf_resol.Blif.Inputs_z, 'z', ref countWhile);
            strNames += NameOfTheFirstLineOfVariables(cnf_resol.Blif.Inputs_x, 'x', ref countWhile);
            strNames += NameOfTheFirstLineOfVariables(cnf_resol.Blif.Inputs_e, 'e', ref countWhile);
            strNames += NameOfTheFirstLineOfVariables(cnf_resol.Blif.Outputs_, 'y', ref countWhile);
            
            string[] strSplit = strNames.Trim().Split(' ');

            for (int i = 0; i < strSplit.Length; i++)
            {
                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = strSplit[i],
                    Binding = new Binding("[" + i + "]")
                });
            }
          
            string[] value;
            List<object> rows = new List<object>();

            for (int i = 0; i < numberRows; i++)
            {
                value = new string[cnf_resol.ListAllVariablesWithIndex.Count];

                List<int> listIndexdForICnfResol = new List<int>();
                foreach (KeyValuePair<int, char> cnf in cnf_resol._CnfResolution[i])
                    listIndexdForICnfResol.Add(cnf.Key);

                for (int j = 0, k = 0; j < cnf_resol.ListAllVariablesWithIndex.Count; j++)
                {
                    if (listIndexdForICnfResol[k] == j)
                    {
                        int K = cnf_resol.IndexFind(i, j);
                        var itemCnfRes = cnf_resol._CnfResolution[i].ElementAt(K);
                        value[j] = itemCnfRes.Value.ToString();
                        k++;
                        if (k >= cnf_resol._CnfResolution[i].Count)
                            k = 0;
                    }
                    else
                        value[j] = ("-").ToString();                
                }
                rows.Add(value);            
            }
            dataGrid.ItemsSource = rows;
        }

        public string NameOfTheFirstLineOfVariables(List<string> list, char nameVariables, ref int countWhile)//z0z1z2...x0x1x2...e0e1e2...y0y1y2...
        {
            string strNames = "";
            for (int j = 0; j < list.Count; j++)
            {
                strNames += nameVariables + j.ToString() + ' ';
                cnf_resol.ListAllVariablesWithIndex[countWhile] = nameVariables + j.ToString();
                countWhile++;
            }
            return strNames;
        }

    }  
}


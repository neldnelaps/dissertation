using System.Collections.Generic;
using System.Linq;

namespace WpfApp
{
    class CnfResolution
    {
        public Blif Blif { get; set; }
        public List<Dictionary<int, char>> _CnfResolution { get; set; }
        public List<Dictionary<int, char>> CnfResolutionBefore { get; set; }
        public List<string> ListAllVariablesWithIndex { get; set; }
        public Dictionary<int, char> Cnf_K { get; set; }

        public CnfResolution()
        {
            Blif = new Blif();
            _CnfResolution = new List<Dictionary<int, char>>();
            CnfResolutionBefore = new List<Dictionary<int, char>>();
            ListAllVariablesWithIndex = new List<string>();           
            Cnf_K = new Dictionary<int, char>();
        }

        public void FormCnfResolutionListNamesIndex(List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
                this.ListAllVariablesWithIndex.Add(list[i]);
        }

        public void ShapingCNFResolution(Blif blif)
        {
            int lstIndex = 0;

            for (int j = 0; j < blif.NamesAndBoolVectors.Count; j++)
            {
                lstIndex = this.ListAllVariablesWithIndex.IndexOf(blif.NamesAndBoolVectors[j].Names[blif.NamesAndBoolVectors[j].Names.Count - 1]);
                if (lstIndex >= this.ListAllVariablesWithIndex.Count - blif.Outputs_.Count && blif.NamesAndBoolVectors[j].Names.Count == 2 && blif.NamesAndBoolVectors[j].NamesBool[0] == '0')
                {
                    Dictionary<int, char> keyValues = new Dictionary<int, char>
                    {
                        { this.ListAllVariablesWithIndex.IndexOf(blif.NamesAndBoolVectors[j].Names[0]), blif.NamesAndBoolVectors[j].NamesBool[0] },
                        { lstIndex, '-' }
                    };
                    this._CnfResolution.Add(keyValues);
                }
                if (blif.NamesAndBoolVectors[j].Names.Count == 3)
                {
                    ForJStrInBlifForminKeyValue(j);
                    ForJStrInBlifForminKeyValueZeroOne(j);
                }
            }

            CnfResolutionBefore = new List<Dictionary<int, char>>(this._CnfResolution);
        }

        public void ForJStrInBlifForminKeyValue(int j)
        {
            int index = 0;

            for (int k = 0; k < Blif.NamesAndBoolVectors[j].Names.Count - 1; k++)
            {
                index = this.ListAllVariablesWithIndex.IndexOf(Blif.NamesAndBoolVectors[j].Names[k]);
                Dictionary<int, char> keyValues = new Dictionary<int, char> { { index, Blif.NamesAndBoolVectors[j].NamesBool[k] } };
                index = this.ListAllVariablesWithIndex.IndexOf(Blif.NamesAndBoolVectors[j].Names[Blif.NamesAndBoolVectors[j].Names.Count - 1]);
                Condition(Blif.NamesAndBoolVectors[j].NamesBool[Blif.NamesAndBoolVectors[j].Names.Count - 1], index, ref keyValues, '0', '1');
                this._CnfResolution.Add(keyValues);
            }
        }

        public void ForJStrInBlifForminKeyValueZeroOne(int j)
        {
            int index = 0;
            Dictionary<int, char> keyValuesNew = new Dictionary<int, char>();

            for (int k = 0; k < Blif.NamesAndBoolVectors[j].Names.Count - 1; k++)
            {
                index = this.ListAllVariablesWithIndex.IndexOf(Blif.NamesAndBoolVectors[j].Names[k]);
                Condition(Blif.NamesAndBoolVectors[j].NamesBool[k], index, ref keyValuesNew, '0', '1');
            }

            index = this.ListAllVariablesWithIndex.IndexOf(Blif.NamesAndBoolVectors[j].Names[Blif.NamesAndBoolVectors[j].Names.Count - 1]);
            Condition(Blif.NamesAndBoolVectors[j].NamesBool[Blif.NamesAndBoolVectors[j].Names.Count - 1], index, ref keyValuesNew, '1', '0');
            this._CnfResolution.Add(keyValuesNew);
        }

        public void Condition(
            char comparison, 
            int index, 
            ref Dictionary<int, char> keyValues, 
            char zero, char one)
        {
            if (comparison == '1')
                keyValues.Add(index, zero);
            else
                keyValues.Add(index, one);
        }

        public void Reduction(Blif blif)
        {
            bool check = Check_cnf_K_ones_zeros(blif.Inputs_z.Count);
            this.FormationOfConjunctionKForLastBlock(blif.Outputs_.Count, check); // forming the conjunction K for the last block z1 = 0 ... zk = 0 and deleting executable lines
            for (int i = 0; i < this._CnfResolution.Count; i++)
            {           
                foreach (KeyValuePair<int, char> item in this._CnfResolution[i])
                {
                    if (item.Key < blif.Outputs_.Count)
                    {
                        if (check)
                            ReductionCondition(ref i, item, '1', '0');// Z1=0, Z2=0...
                        else
                            ReductionCondition(ref i, item, '0', '1');//Z1=1,Z2=1....
                        break;
                    }
                }
            }
            ReductionRules();
        } // simplification for the first block replacement z1 = 1, z2 = 1 ... or z1 = 0, z2 = 0 ...

        public bool Check_cnf_K_ones_zeros(int zCount)
        {
            int sumOnes = 0, sumZeros = 0;

            for (int i = 0; i < this._CnfResolution.Count; i++)
            {
                if (this._CnfResolution[i].Count == 2)
                    foreach (KeyValuePair<int, char> item in this._CnfResolution[i])
                    {
                        if (item.Key <= zCount)
                        {
                            if (item.Value == '1')
                                sumOnes++;
                            if (item.Value == '0')
                                sumZeros++;
                        }
                    }
            }
            if (sumOnes > sumZeros)
                return true;
            return false;
        }

        public void FormationOfConjunctionKForLastBlock(int count, bool check)
        {
            if (check)
            {
                for (int i = 0; i < count; i++)
                    this.Cnf_K.Add(this.ListAllVariablesWithIndex.Count - i - 1, '0');//!z1!z2!z3...
                return;
            }
            for (int i = 0; i < count; i++)
                this.Cnf_K.Add(this.ListAllVariablesWithIndex.Count - i - 1, '1');//z1z2z3...
        }

        public void ReductionCondition(
            ref int i, 
            KeyValuePair<int, char> item, 
            char one, 
            char two)
        {
            if (item.Value == one)
            { //// Z1=0, Z2=0...
                this._CnfResolution[i].Remove(item.Key);
                i--;
                return;
            }
            if (item.Value == two)
            {
                this._CnfResolution.RemoveAt(i);
                i--;
                return;
            }
        }

        public void ReductionRules()
        {
            int numberStr = 0;
            List<Dictionary<int, char>> keyValuePairs = new List<Dictionary<int, char>>(this._CnfResolution);
            

            for (int i = 0; i < this._CnfResolution.Count; i++)
            {            
                if (this._CnfResolution[i].Count == 0)
                {
                    this._CnfResolution.RemoveAt(i);
                    i--;
                }
                if (this._CnfResolution[i].Count == 1)
                {
                    numberStr = this.RulesOfSimplification(i); 
                    if (i >= numberStr && numberStr != 0)
                        i -= numberStr;
                }
                if (this._CnfResolution[i].Count == 2)
                {
                    numberStr = 0;
                    foreach (KeyValuePair<int, char> item in this._CnfResolution[i])
                    {
                        if (item.Value == '-')
                            numberStr = this.RulesOfSimplification(i);
                        if (i >= numberStr && numberStr != 0)
                            i -= numberStr;
                    }
                }
                if (i == this._CnfResolution.Count - 1 && !Enumerable.SequenceEqual(keyValuePairs, this._CnfResolution))
                { 
                    i = -1;
                    keyValuePairs.Clear();
                    keyValuePairs = new List<Dictionary<int, char>>(this._CnfResolution);
                }
            }
        }

        public int RulesOfSimplification(int index)// application of simplification rules A(A+X)=A, A(!A+X)=AX, AA=A. Returns the number of rows deleted. 
        {
            int nubmerStr = 0;
            var keyIndexFirst = this._CnfResolution[index].Keys.First();
            var valueIndexFirst = this._CnfResolution[index].Values.First();

            if (valueIndexFirst == '-')
                return 0;

            for (int i = 0; i < this._CnfResolution.Count; i++)
            {
                if (i == index)
                    i++;
                if (i < this._CnfResolution.Count)
                {
                    foreach (KeyValuePair<int, char> item in this._CnfResolution[i])
                    {
                        if (item.Value == valueIndexFirst && item.Key == keyIndexFirst)
                        {
                            this._CnfResolution.RemoveAt(i);
                            index--;
                            nubmerStr++;
                            i = i - 1;
                            break;
                        }
                        if (item.Value != valueIndexFirst && item.Key == keyIndexFirst && this._CnfResolution[i].Count != 1)
                        {
                            this._CnfResolution[i].Remove(item.Key);
                            break;
                        }
                        if (item.Value != valueIndexFirst && item.Key == keyIndexFirst && this._CnfResolution[i].Count == 1)
                        {
                            this._CnfResolution.RemoveAt(i);
                            this._CnfResolution[index].Remove(keyIndexFirst);
                            this._CnfResolution[index].Add(keyIndexFirst, '-');
                            return nubmerStr++;
                        }
                    }
                }
            }
            return nubmerStr;
        }

        public int IndexFind(int i, int inFind)
        {
            int j = 0;
            foreach (KeyValuePair<int, char> item in this._CnfResolution[i])
            {
                if (item.Key == inFind)
                    return j;
                j++;
            }
            return 0;
        }

        public void RemoveOfEmptyCnf()
        {
            for (int i = 0; i < this._CnfResolution.Count; i++)
            {
                int k = 0;

                foreach (KeyValuePair<int, char> item in this._CnfResolution[i])
                {
                    if (item.Value == '-')
                        k++;
                    else
                        break;
                }
                if (k == this._CnfResolution[i].Count)
                {
                    this._CnfResolution.RemoveAt(i);
                    i--;
                }
            }
        }        
    }
}

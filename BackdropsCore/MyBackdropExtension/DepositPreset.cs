using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaywardExtensions;

namespace BackdropsCore
{
    class DepositPreset
    {
        public int minDeposits = 0;
        public int maxDeposits = 2;

        public Dictionary<DepositeType, float> depositeWeights;
        public Dictionary<DepositeType, float> operationsWeights;

        public int minMiningOperations = 0;
        public int maxMiningOperations = 0;

        public DepositPreset()
        {
            depositeWeights = new Dictionary<DepositeType, float>();
            operationsWeights = new Dictionary<DepositeType, float>();
        }

        public void AddDepositeChance(DepositeType deposite, float weight)
        {
            if (!depositeWeights.ContainsKey(deposite))
            {
                depositeWeights.Add(deposite, weight);
            }
            else
            {
                depositeWeights[deposite] += weight;
            }
        }

        public void AddOperationChance(DepositeType deposite, float weight)
        {
            if (!operationsWeights.ContainsKey(deposite))
            {
                operationsWeights.Add(deposite, weight);
            }
            else
            {
                operationsWeights[deposite] += weight;
            }
        }
    }
}

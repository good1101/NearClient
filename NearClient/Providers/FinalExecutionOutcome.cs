using System.Collections.Generic;

namespace NearClient.Providers
{
    public class FinalExecutionOutcome
    {
        public ExecutionOutcomeWithId[] Receipts { get; set; }
        public FinalExecutionStatus Status { get; set; }
        public FinalExecutionStatusBasic StatusBasic { get; set; }
        public ExecutionOutcomeWithId Transaction { get; set; }
        public AccessKey AccessKey { get; set; }

        public static FinalExecutionOutcome FromDynamicJsonObject(dynamic jsonObject)
        {
            var receipts = new List<ExecutionOutcomeWithId>();
            var recOut = jsonObject.receipts_outcome;
            foreach (var receipt in recOut)
            {
                receipts.Add(ExecutionOutcomeWithId.FromDynamicJsonObject(receipt));
            }
            var result = new FinalExecutionOutcome()
            {
                Receipts = receipts.ToArray(),
                Status = FinalExecutionStatus.FromDynamicJsonObject(jsonObject.status),
                Transaction =  ExecutionOutcomeWithId.FromDynamicJsonObject(jsonObject.transaction_outcome)
            };
            return result;
        }
    }
}